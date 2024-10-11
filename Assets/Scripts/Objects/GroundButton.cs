using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GroundButton : MonoBehaviour
{
    public Sprite buttonUp;
    public Sprite buttonDown;
    SpriteRenderer sptr;

    public UnityEvent OnButtonPressed;
    public UnityEvent OnButtonRelease;

    public bool state;
    private void Start()
    {
        sptr = GetComponent<SpriteRenderer>();
        sptr.sprite = buttonUp;
        state = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        sptr.sprite = buttonDown;
        state = true;
        OnButtonPressed.Invoke();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        sptr.sprite = buttonUp;
        state = false;
        OnButtonRelease.Invoke();
    }

}
