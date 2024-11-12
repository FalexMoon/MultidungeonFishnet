using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetInventory : NetworkBehaviour
{
    public readonly SyncVar<int> coins = new SyncVar<int>();
    NetPlayerUI ui;


    private void Awake()
    {
        coins.OnChange += OnCoinsValueChanged;
    }

    void OnCoinsValueChanged(int before, int after, bool server)
    {
        if (!server)
        {
            UpdateUI();
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
        }
    }

    public void InitializeUI(NetPlayerUI setUI)
    {
        ui = setUI;
        UpdateUI();
    }

    public void LocalGetCoin(int cantCoins)
    {
        GetCoin(this, cantCoins);
    }

    [ServerRpc]
    public void GetCoin(NetInventory inventory, int cantCoins)
    {
        inventory.coins.Value += cantCoins;
    }

    void UpdateUI()
    {
        if (IsOwner)
        {
            if (coins.Value > 0)
            {
                NetLocalLevelManager.LocalManager.CheckVictory(this);
            }
            ui.UpdateCantCoinsUI(coins.Value);
        }
    }
}
