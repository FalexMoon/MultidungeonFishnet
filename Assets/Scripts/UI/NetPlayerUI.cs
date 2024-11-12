using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetPlayerUI : MonoBehaviour
{

    public static NetPlayerUI LocalPlayerUI { get; private set; }

    public TextMeshProUGUI cantCoinsTxt;


    public GameObject lobbyUI;

    public GameObject gameOverUI;
    public TextMeshProUGUI gameOverTitleTxt;
    public TextMeshProUGUI gameOverDescriptionTxt;

    NetworkManager networkManager;

    private void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        LocalPlayerUI = this;
        GameEvents.instance.OnLocalPlayerSpawn.AddListener(AssingPlayerUI);
    }

    private void OnDestroy()
    {
        GameEvents.instance.OnLocalPlayerSpawn.RemoveListener(AssingPlayerUI);
    }

    public void UpdateCantCoinsUI(int newCantCoins)
    {
        cantCoinsTxt.text = newCantCoins.ToString();
    }
    public void AssingPlayerUI()
    {
        NetPlayerMoveRPG.LocalPlayer.GetComponent<NetInventory>().InitializeUI(this);

        ShowLobbyUI();
    }
    public void HideAllPlayerUI()
    {
        HideLobbyUI();
        HideGameOverUI();
    }
    public void ShowLobbyUI()
    {
        lobbyUI.SetActive(true);
        NetPlayerMoveRPG.LocalPlayer.Freeze();
    }
    public void HideLobbyUI()
    {
        lobbyUI.SetActive(false);
        NetPlayerMoveRPG.LocalPlayer.UnFreeze();
    }

    public void HideGameOverUI()
    {

        gameOverUI.SetActive(false);
    }
    public void ShowGameOver(bool win, NetworkConnection winner)
    {
        NetPlayerMoveRPG.LocalPlayer.Freeze();
        gameOverUI.SetActive(true);
        if (win)
        {
            gameOverTitleTxt.text = "Ganaste";
            gameOverDescriptionTxt.text = "Felicidades, fuiste el primero en conseguir todas las monedas";
        }
        else
        {
            gameOverTitleTxt.text = "Perdiste";
            gameOverDescriptionTxt.text = $"Fuiste vencido por el jugador {winner.ClientId}";

        }
    }

    public void DisconnectClient()
    {
        if (NetPlayerMoveRPG.LocalPlayer.IsServerInitialized)
        {
            networkManager.ServerManager.StopConnection(true);
        }
        else
        {
            networkManager.ClientManager.StopConnection();
        }
    }
}
