using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro; 
using Photon.Realtime;
using UnityEngine.UI;
public class multiplayerLobby : MonoBehaviourPunCallbacks
{
    public GameObject createRoomPanel;
    public GameObject joinRoom;
    public GameObject mainMenu;
    public GameObject joinLobby;
    public GameObject loadingScreen;
    public GameObject namePanel;
    public TMP_InputField inputNameText;
    public TMP_Text roomName;
    public TMP_Text loadingText;
    public TMP_InputField nameInput;
    public static bool hasSetNick;
    public TMP_Text roomNameText, playerNameLabel;
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();
    public GameObject startButton;
    public static multiplayerLobby instance;
    public GameObject roomBrowserScreen;
    public roomButton theRoomButton;
    private List<roomButton> allRoomButtons = new List<roomButton>();
    public string levelToPlay = "SampleScene";
    public TMP_Text playerCount;
    public TMP_Text player1;
    public TMP_Text player2;
    public GameObject p1image;
    public GameObject p2image;
    private void Awake()
    {
        instance = this;

    }
    // Start is called before the first frame update
    void Start()
    {
        //PlayerPrefs.DeleteAll();
        if (PlayerPrefs.HasKey("name"))
        {
            CloseMenus();
            loadingScreen.SetActive(true);
            loadingText.text = "Connecting To Network...";

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else
        {
            //PlayerPrefs.SetInt("name", 1);
            namePanel.SetActive(true);
            //saveName();
        }

    }
    public void saveName()
    {
        if (inputNameText.text.Length > 0)
        {
            PlayerPrefs.SetString("name", inputNameText.text);
            loadNetwork();
        }
        else
        {

            return;

        }
    }
    public void loadNetwork()
    {
        CloseMenus();
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting To Network...";

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public override void OnConnectedToMaster()
    {


        PhotonNetwork.JoinLobby();

        PhotonNetwork.AutomaticallySyncScene = true;

        loadingText.text = "Joining Lobby...";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        mainMenu.SetActive(true);

        PhotonNetwork.NickName = PlayerPrefs.GetString("name");

        if (!hasSetNick)
        {
            CloseMenus();
            mainMenu.SetActive(true);

            if (PlayerPrefs.HasKey("playerName"))
            {
                nameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }
    void CloseMenus()
    {
        createRoomPanel.SetActive(false);
        joinRoom.SetActive(false);
        mainMenu.SetActive(false);
        joinLobby.SetActive(false);
        loadingScreen.SetActive(false);
        namePanel.SetActive(false);
        //loadingScreen.SetActive(false);
        //menuButtons.SetActive(false);
        //createRoomScreen.SetActive(false);
        //roomScreen.SetActive(false);
        //errorScreen.SetActive(false);
        //roomBrowserScreen.SetActive(false);
        //nameInputScreen.SetActive(false);
    }
    public void OpenRoomBrowser()
    {
        CloseMenus();
        createRoomPanel.SetActive(true);
    }
    public void joinLobbyPanel()
    {
        CloseMenus();
        joinLobby.SetActive(true);
    }
    public void OpenRooms()
    {
        CloseMenus();
        joinRoom.SetActive(true);
    }
    public void openMainMenu()
    {
        CloseMenus();
        mainMenu.SetActive(true);
    }
    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomName.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 2;
            options.IsOpen = true;
            PhotonNetwork.CreateRoom(roomName.text, options);

            CloseMenus();
            loadingText.text = "Creating Room...";
            loadingScreen.SetActive(true);
        }
    }
    public override void OnJoinedRoom()
    {
        CloseMenus();
        joinRoom.SetActive(true);

        roomName.text = PhotonNetwork.CurrentRoom.Name;

        ListAllPlayers();

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }
    private void ListAllPlayers()
    {
        foreach (TMP_Text player in allPlayerNames)
        {
            Destroy(player.gameObject);
        }
        allPlayerNames.Clear();

        Player[] players = PhotonNetwork.PlayerList;
        if (players.Length == 1)
        {
            player1.text = players[0].NickName;
            p2image.SetActive(false);
        }
        else if (players.Length > 0)
        {
            player1.text = players[0].NickName;
            p2image.SetActive(true);
            player2.text = players[1].NickName;
        }
        for (int i = 0; i < players.Length; i++)
        {
            TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);

            allPlayerNames.Add(newPlayerLabel);
            //player1.text = players[0].NickName;
            //player2.text = players[1].NickName;
        }
    }
    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);

        CloseMenus();
        loadingText.text = "Joining Room";
        loadingScreen.SetActive(true);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);

        allPlayerNames.Add(newPlayerLabel);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        loadingText.text = "Leaving Room";
        loadingScreen.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        CloseMenus();
        mainMenu.SetActive(true);
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (roomButton rb in allRoomButtons)
        {
            Destroy(rb.gameObject);
        }
        allRoomButtons.Clear();

        theRoomButton.gameObject.SetActive(false);

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                roomButton newButton = Instantiate(theRoomButton, theRoomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);

                allRoomButtons.Add(newButton);
            }
        }
    }
    public void StartGame()
    {

        if (PhotonNetwork.PlayerList.Length == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.LoadLevel("SampleScene");
        }
        else
        {
            playerCount.text = "You need atleast " + PhotonNetwork.CurrentRoom.MaxPlayers + " players to start the game";
        }


        //PhotonNetwork.LoadLevel(allMaps[Random.Range(0, allMaps.Length)]);
    }
    public void QuickJoin()
    {
        PhotonNetwork.JoinRandomRoom();
        CloseMenus();
        loadingText.text = "Joining Room";
        loadingScreen.SetActive(true);

    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions options = new RoomOptions();
        options.IsOpen = true;
        options.MaxPlayers = 2;
        PhotonNetwork.CreateRoom("Test", options);

        CloseMenus();
        loadingText.text = "No Rooms, Creating Room";
        loadingScreen.SetActive(true);
    }
    void Update()
    {
        if (PhotonNetwork.PlayerList.Length == 1)
        {
            player1.text = PhotonNetwork.PlayerList[0].NickName;
            p2image.SetActive(false);
        }
        else if (PhotonNetwork.PlayerList.Length > 0)
        {
            player1.text = PhotonNetwork.PlayerList[0].NickName;
            p2image.SetActive(true);
            player2.text = PhotonNetwork.PlayerList[1].NickName;
        }
    }
}
