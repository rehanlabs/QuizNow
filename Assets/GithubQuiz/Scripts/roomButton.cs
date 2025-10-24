using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class roomButton : MonoBehaviour
{
    public TMP_Text buttonText;

    private RoomInfo info;
    public void SetButtonDetails(RoomInfo inputInfo)
    {
        info = inputInfo;

        buttonText.text = info.Name;
    }
    public void OpenRoom()
    {
        multiplayerLobby.instance.JoinRoom(info);
    }
}
