using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
public class finalScore : MonoBehaviourPunCallbacks
{
    public TMP_Text winner;
    public TMP_Text p1score;
    public TMP_Text p2score;
    
    // Start is called before the first frame update
    void Awake()
    {
        if(PhotonNetwork.PlayerList.Length==2)
        {
            if ((int)PhotonNetwork.PlayerList[1].CustomProperties["score"] > (int)PhotonNetwork.PlayerList[0].CustomProperties["score"])
            {
                winner.text = "Player " + PhotonNetwork.PlayerList[1].NickName + " Wins";
            }
            else if ((int)PhotonNetwork.PlayerList[1].CustomProperties["score"] < (int)PhotonNetwork.PlayerList[0].CustomProperties["score"])
            {
                winner.text = "Player " + PhotonNetwork.PlayerList[0].NickName + " Wins";
            }
            else
            {
                //winner.text = "Its a Draw";
                if ((int)PhotonNetwork.PlayerList[1].CustomProperties["score"] > (int)PhotonNetwork.PlayerList[0].CustomProperties["score"])
                {
                    winner.text = "Player " + PhotonNetwork.PlayerList[1].NickName + " Wins Because he/she answered quickly";
                }
                if ((int)PhotonNetwork.PlayerList[1].CustomProperties["score"] < (int)PhotonNetwork.PlayerList[0].CustomProperties["score"])
                {
                    winner.text = "Player " + PhotonNetwork.PlayerList[0].NickName + " Wins Because he/she answered quickly";
                }
                else
                {
                    winner.text = "Its a Draw";
                }
            }
            p1score.text = PhotonNetwork.PlayerList[0].NickName + " Scores:" + PhotonNetwork.PlayerList[0].CustomProperties["score"].ToString();
            p2score.text = PhotonNetwork.PlayerList[1].NickName + " Scores:" + PhotonNetwork.PlayerList[1].CustomProperties["score"].ToString();
        }
        else
        {
            winner.text = "Player " + PhotonNetwork.PlayerList[0].NickName + " Wins";
            p1score.text = PhotonNetwork.PlayerList[0].NickName + " Scores:" + PhotonNetwork.PlayerList[0].CustomProperties["score"].ToString();
        }
        
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.CloseConnection(PhotonNetwork.LocalPlayer);
        SceneManager.LoadScene("homeScreen");
        //CloseMenus();
        //loadingText.text = "Leaving Room";
        //loadingScreen.SetActive(true);
    }
}
