using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FishNet.Object;

[RequireComponent(typeof(BoxCollider2D))]
public class NetSwordInteractuable : NetworkBehaviour
{
    public UnityEvent Hit;
    public UnityEvent StateChange;
    public bool state;

    [Range(0,1)]
    public float dropChances;
    public GameObject dropPF;

    Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }
    public void AnimatorSetBoolTrue(string boolName)
    {
        anim.SetBool(boolName, true);
    }
    public void AnimatorSetBoolFalse(string boolName)
    {
        anim.SetBool(boolName, false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Espada"))
        {
            if (collision.transform.root.GetComponent<NetPlayerMoveRPG>() == NetPlayerMoveRPG.LocalPlayer)
            {
                print("Espada");
                OnSwordHit();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void OnSwordHit()
    {
        if (!IsServerStarted)
        {
            return;
        }
        try
        {
            Hit.Invoke();
            if(dropChances > 0)
            {
                SpawnDrop();
            }
        }
        catch
        {
            print("No hay eventos");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnStateChanged()
    {
        state = !state;
        try
        {
            StateChange.Invoke();
        }
        catch
        {
            print("No hay eventos");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AutoDestroyAfterAnim()
    {
        AutoDestroy();
    }

    private void AutoDestroy()
    {
        if (IsServerInitialized)
        {
            Despawn(); // Despawn en red
        }
    }

    private void SpawnDrop()
    {
        // Genera un drop aleatorio en la posición del arbusto.
        if (Random.value < dropChances) 
        {
            GameObject drop = Instantiate(dropPF, transform.position, Quaternion.identity);

            NetworkObject networkObject = drop.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                Spawn(networkObject); // Usa Spawn del NetworkObject para sincronizarlo en la red.
            }
        }
    }
}
