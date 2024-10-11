using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class SwordInteractuable : MonoBehaviour
{
    public UnityEvent Hit;
    public UnityEvent StateChange;
    public bool state;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Espada"))
        {
            print("Espada");
            OnSwordHit();
        }
    }

    void OnSwordHit()
    {
        try
        {
            Hit.Invoke();
        }
        catch
        {
            print("No hay eventos");
        }
    }

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
    public void AutoDestroyAfterAnim()
    {
        Destroy(gameObject);
    }
    public void AutoDestroy()
    {
        Destroy(gameObject);
    }
}
