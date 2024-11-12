using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollectableObject : NetworkBehaviour
{

    public UnityEvent OnCollected;

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
        
        if (collision.CompareTag("Player"))
        {
            if (collision.GetComponent<NetPlayerMoveRPG>() == NetPlayerMoveRPG.LocalPlayer)
            {
                print("Collected");
                OnCollected.Invoke();
                NetPlayerMoveRPG.LocalPlayer.GetComponent<NetInventory>().LocalGetCoin(1);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AutoDestroyAfterAnim()
    {
        Despawn(gameObject);
    }
}
