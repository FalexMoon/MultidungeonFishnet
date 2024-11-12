using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEvents : MonoBehaviour
{
    public static GameEvents instance { get; private set; }

    public UnityEvent OnLocalPlayerSpawn;

    public UnityEvent OnServerConnectionStarted;
    //public UnityEvent OnClientConnectionStarted;

    public void Awake()
    {
        instance = this;
    }
}
