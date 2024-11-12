using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class NetCamera : MonoBehaviour
{
    CinemachineVirtualCamera cam;

    private void Start()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
        GameEvents.instance.OnLocalPlayerSpawn.AddListener(AssingPlayerToFollow);
    }

    private void OnDestroy()
    {
        if (GameEvents.instance)
        {
            GameEvents.instance.OnLocalPlayerSpawn.RemoveListener(AssingPlayerToFollow);
        }
    }

    void AssingPlayerToFollow()
    {
        cam.Follow = NetPlayerMoveRPG.LocalPlayer.transform;
    }
}
