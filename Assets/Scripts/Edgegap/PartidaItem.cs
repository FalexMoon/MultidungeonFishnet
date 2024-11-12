using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartidaItem : MonoBehaviour
{
    public TextMeshProUGUI nombrePartidaTxt;
    public TextMeshProUGUI numPlayersTxt;
    EdgegapRelayManager edgegapRelayManager;

    public void Setup(ApiResponse apiResponse, EdgegapRelayManager relayManager)
    {
        edgegapRelayManager = relayManager;
        nombrePartidaTxt.text = apiResponse.session_id;
        numPlayersTxt.text = apiResponse.session_users.Length.ToString();
    }

    public async void JoinPartida()
    {
        await edgegapRelayManager.UnirPartida(nombrePartidaTxt.text);
    }
}
