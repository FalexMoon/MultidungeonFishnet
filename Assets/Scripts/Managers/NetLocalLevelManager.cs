using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetLocalLevelManager : NetworkBehaviour
{

    public static NetLocalLevelManager LocalManager { get; private set; }

    public int objectiveToWin;
    public int cantClientes;
    private void Start()
    {
        GameEvents.instance.OnServerConnectionStarted.AddListener(InitializeServer);
    }

    private void OnDestroy()
    {
        if (GameEvents.instance)
        {
            GameEvents.instance.OnServerConnectionStarted.RemoveListener(InitializeServer);
        }
    }

    void InitializeServer()
    {
        cantClientes = 0;
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        LocalManager = this;
        OnConnectedClient();
    }

    public void CheckVictory(NetInventory playerInventory)
    {
        if (playerInventory.coins.Value >= objectiveToWin)
        {
            RPCServerWinCheck(playerInventory.Owner); 
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnConnectedClient()
    {
        print("Se ha conectado un cliente");
        cantClientes++;
        if(cantClientes > 1)
        {
            RPCComenzarPartida();
        }
    }

    [ObserversRpc]
    private void RPCComenzarPartida()
    {
        NetPlayerUI.LocalPlayerUI.HideLobbyUI();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RPCServerWinCheck(NetworkConnection winner)
    {
        RPCNotifyWinning(winner);
    }

    [ObserversRpc]
    private void RPCNotifyWinning(NetworkConnection winner)
    {
        if (NetworkManager.ClientManager.Connection == winner)
        {
            print("Ganaste");
            NetPlayerUI.LocalPlayerUI.ShowGameOver(true, winner);
        }
        else
        {
            print("Perdiste");
            NetPlayerUI.LocalPlayerUI.ShowGameOver(false, winner);

        }
    }

}
