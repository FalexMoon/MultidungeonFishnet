using FishNet.Transporting.KCP.Edgegap;
using FishNet.Transporting.KCP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Transporting;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class EdgegapAutoConnect : MonoBehaviour
{
    public static ApiResponse apiResponse;
    bool isLocalHost;
    uint localUserToken;
    string actualSessionId;
    [SerializeField] EdgegapKcpTransport kcpTransport;
    [SerializeField] string relayToken;
    [SerializeField] string edgegapBaseURL = "https://api.edgegap.com/v1";
    HttpClient httpClient = new HttpClient();

    [Header("UI")]
    public GameObject connectingUI;


    private void Start()
    {
        kcpTransport.OnServerConnectionState += OnServerConnectionStateChange;
        kcpTransport.OnClientConnectionState += OnClientConnectionStateChange;

        if (apiResponse == null)
        {
            return;
        }

        uint userToken = 0;

        if (apiResponse.session_users != null)
        {
            userToken = apiResponse.session_users[0].authorization_token;
            isLocalHost = true;
        }
        else
        {
            userToken = apiResponse.session_user.authorization_token;
            isLocalHost = false;
        }

        //int tokentoUse = apiResponse.session_users.Length - 1;
        EdgegapRelayData relayData = new EdgegapRelayData(
            apiResponse.relay.ip,
            apiResponse.relay.ports.server.port,
            apiResponse.relay.ports.client.port,
        userToken,
            apiResponse.authorization_token);

        kcpTransport.SetEdgegapRelayData(relayData);

        actualSessionId = apiResponse.session_id;
        localUserToken = userToken;

        if (isLocalHost)//Si el user es el primero, se une tambien como host
        {
            kcpTransport.StartConnection(true); //Nos conectamos al server como Host
        }
        kcpTransport.StartConnection(false); // Nos conectamos al server como cliente
        
    }

    void OnServerConnectionStateChange(ServerConnectionStateArgs args)
    {
        switch (args.ConnectionState)
        {
            case LocalConnectionState.Stopped:
                print("Server Detenido");
                break;
            case LocalConnectionState.Starting:
                print("Server Iniciando");
                break;
            case LocalConnectionState.Started:
                print("Server Iniciado");
                GameEvents.instance.OnServerConnectionStarted.Invoke();
                break;
            case LocalConnectionState.Stopping:
                print("Server Deteniendose");
                break;
        }
    }

    void OnClientConnectionStateChange(ClientConnectionStateArgs args)
    {
        switch (args.ConnectionState)
        {
            case LocalConnectionState.Stopped:
                print("Cliente Desconectado");
                SalirDePartida();
                break;
            case LocalConnectionState.Starting:
                print("Cliente Iniciando");
                connectingUI.SetActive(true);
                break;
            case LocalConnectionState.Started:
                print("Cliente Conectado");
                connectingUI.SetActive(false);
                break;
            case LocalConnectionState.Stopping:
                print("Cliente Deteniendose");
                break;
        }
    }
    async Task SalirDePartida()
    {
        if (!string.IsNullOrWhiteSpace(actualSessionId))
        {
            if (isLocalHost)
            {
                await BorrarPartida(actualSessionId);
            }
            else
            {
               await AbandonarPartida();

            }
            actualSessionId = null;
            isLocalHost = false;
            localUserToken = 0;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");

    }

    async Task AbandonarPartida()
    {
        LeaveSession leaveSession = new LeaveSession()
        {
            session_id = actualSessionId,
            authorization_token = localUserToken
        };

        string leaveSessionJson = JsonUtility.ToJson(leaveSession);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", relayToken);
        HttpContent content = new StringContent(leaveSessionJson, Encoding.UTF8, "application/json");
        await httpClient.PostAsync($"{edgegapBaseURL}/relays/sessions:revoke-user", content);

    }
    async Task BorrarPartida(string session_id)
    {

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", relayToken);
        HttpResponseMessage responseMessage = await httpClient.DeleteAsync($"{edgegapBaseURL}/relays/sessions/{session_id}");
    }

}
