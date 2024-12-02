using FishNet.Managing.Scened;
using FishNet.Transporting;
using FishNet.Transporting.KCP.Edgegap;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class EdgegapRelayManager : MonoBehaviour
{
    [SerializeField]string relayToken;
    [SerializeField] string edgegapBaseURL = "https://api.edgegap.com/v1";
    [SerializeField] EdgegapKcpTransport kcpTransport;

    [SerializeField] GameObject partidasUIGO;
    [SerializeField] GameObject conectandoUIGO;
    [SerializeField] Transform partidaItemContainer;
    [SerializeField] GameObject partidaItemGO;

    bool isLocalHost;
    string actualSessionId;
    uint localUserToken;

    HttpClient httpClient = new HttpClient();

    void ActualizarListaPartidasUI(Sessions sessions)
    {
        foreach (Transform child in partidaItemContainer)
        {
            Destroy(child.gameObject);
        }

        foreach(ApiResponse partidaData in sessions.sessions)
        {
            GameObject item = Instantiate(partidaItemGO, partidaItemContainer);
            PartidaItem partidaItem = item.GetComponent<PartidaItem>();
            partidaItem.Setup(partidaData, this);
        }
    }

    private void Start()
    {
        kcpTransport.OnServerConnectionState += OnServerConnectionStateChange;
        kcpTransport.OnClientConnectionState += OnClientConnectionStateChange;
        RefreshPartidas();
        EdgegapAutoConnect.apiResponse = null;

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
                conectandoUIGO.SetActive(false);
                NetPlayerUI.LocalPlayerUI.HideAllPlayerUI();
                partidasUIGO.SetActive(true);
                SalirDePartida();
                RefreshPartidas();
                break;
            case LocalConnectionState.Starting:
                conectandoUIGO.SetActive(true);
                NetPlayerUI.LocalPlayerUI.HideGameOverUI();
                print("Cliente Iniciando");
                break;
            case LocalConnectionState.Started:
                partidasUIGO.SetActive(false);
                print("Cliente Conectado");
                break;
            case LocalConnectionState.Stopping:
                print("Cliente Deteniendose");
                break;
        }
    }

    private void SalirDePartida()
    {
        if (!string.IsNullOrWhiteSpace(actualSessionId))
        {
            if (isLocalHost)
            {
                BorrarPartida(actualSessionId);
            }
            else
            {
                AbandonarPartida();

            }
            actualSessionId = null;
            isLocalHost = false;
            localUserToken = 0;
        }
    }
    private void OnApplicationQuit()
    {
        SalirDePartida();
    }

    public async void CrearPartida()
    {
        await CrearPartidaAsync();
    }


    public async Task CrearPartidaAsync()
    {
        //Mandamos un Get para obtener nuestra IP
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", relayToken);
        HttpResponseMessage responseMessage = await httpClient.GetAsync($"{edgegapBaseURL}/ip");
        string response = await responseMessage.Content.ReadAsStringAsync();
        UserIP userIP = JsonUtility.FromJson<UserIP>(response);

        // Creamos un usuario con la IP que nos devolvio
        Users users = new Users();
        users.users = new List<User>();
        users.users.Add(new User() { ip = userIP.public_ip });
        

        //Inicializamos un server mandando los usuarios
        string usersJson = JsonUtility.ToJson(users);
        HttpContent usersContent = new StringContent(usersJson, Encoding.UTF8, "application/json");
        responseMessage = await httpClient.PostAsync($"{edgegapBaseURL}/relays/sessions", usersContent);
        response = await responseMessage.Content.ReadAsStringAsync();
        
        //Obtenemos los resultados del server creado
        ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(response);
        print($"Sesion creada, ID: {apiResponse.session_id}");

        //Esperamos hasta que el server este listo
        while (!apiResponse.ready)
        {
            await Task.Delay(2500);
            responseMessage = await httpClient.GetAsync($"{edgegapBaseURL}/relays/sessions/{apiResponse.session_id}");
            response = await responseMessage.Content.ReadAsStringAsync();
            apiResponse = JsonUtility.FromJson<ApiResponse>(response);
        }

        //ConnectarAPartida(apiResponse); //Si es que el UI esta en la misma escena del juego

        EdgegapAutoConnect.apiResponse = apiResponse;//Ese code es si el NetworkManager esta en otra escena
        UnityEngine.SceneManagement.SceneManager.LoadScene("Relay");
    }

    void ConnectarAPartida(ApiResponse apiResponse)
    {
        uint userToken = 0;

        if(apiResponse.session_users != null)
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

    [ContextMenu("Obtener Lista Partidas")]
    public async void RefreshPartidas()
    {
        await GetTodasLasPartidasAsync();
    }
    async Task GetTodasLasPartidasAsync()
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", relayToken);
        HttpResponseMessage responseMessage = await httpClient.GetAsync($"{edgegapBaseURL}/relays/sessions");
        string response = await responseMessage.Content.ReadAsStringAsync();
        
        Sessions sessions = JsonUtility.FromJson<Sessions>(response);

        ActualizarListaPartidasUI(sessions);
    }

    public async Task UnirPartida(string session_Id)
    {
        //Mandamos un Get para obtener nuestra IP
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", relayToken);
        HttpResponseMessage responseMessage = await httpClient.GetAsync($"{edgegapBaseURL}/ip");
        string response = await responseMessage.Content.ReadAsStringAsync();
        UserIP userIP = JsonUtility.FromJson<UserIP>(response);

        // Creamos un usuario con la IP que nos devolvio
        JoinSession joinSession = new JoinSession()
        {
            session_id = session_Id,
            user_ip = userIP.public_ip
        };


        //Inicializamos un server mandando los usuarios
        string usersJson = JsonUtility.ToJson(joinSession);
        HttpContent usersContent = new StringContent(usersJson, Encoding.UTF8, "application/json");
        responseMessage = await httpClient.PostAsync($"{edgegapBaseURL}/relays/sessions:authorize-user", usersContent);
        response = await responseMessage.Content.ReadAsStringAsync();

        //Obtenemos los resultados del server creado
        ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(response);

        //ConnectarAPartida(apiResponse); //Si es que el UI esta en la misma escena del juego

        EdgegapAutoConnect.apiResponse = apiResponse;//Ese code es si el NetworkManager esta en otra escena
        UnityEngine.SceneManagement.SceneManager.LoadScene("Relay");

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


    [ContextMenu("Borrar las Partidas")]
    async void DevBorrarTodasLasPartidas()
    {
        //Se obtienen todas las partidas
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", relayToken);
        HttpResponseMessage responseMessage = await httpClient.GetAsync($"{edgegapBaseURL}/relays/sessions");
        string response = await responseMessage.Content.ReadAsStringAsync();

        Sessions sessions = JsonUtility.FromJson<Sessions>(response);

        //Se borran de una en una
        foreach(ApiResponse session in sessions.sessions)
        {
            await BorrarPartida(session.session_id);
        }
        print("Se han borrado todas las partidas");
    }

    async Task BorrarPartida(string session_id)
    {
        
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", relayToken);
        HttpResponseMessage responseMessage = await httpClient.DeleteAsync($"{edgegapBaseURL}/relays/sessions/{session_id}");
    }
}
