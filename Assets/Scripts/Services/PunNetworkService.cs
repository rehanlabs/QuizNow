#if PHOTON_INSTALLED
using System;
using System.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;
using QuizNet;

namespace QuizGame.Services
{
    public class PunNetworkService : MonoBehaviourPunCallbacks, INetworkService, IOnEventCallback
    {
        public bool IsConnected => PhotonNetwork.IsConnectedAndReady;
        public bool IsHost => PhotonNetwork.IsMasterClient;
        public int PlayerCount => PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;

        // Added by rehan
        private string _pendingTopicId;
        private byte _pendingMaxPlayers;


        public event Action<byte, object> OnEventReceived;
        public event Action OnAllPlayersReady;

        private TaskCompletionSource<bool> _connectionTcs;
        private TaskCompletionSource<bool> _lobbyTcs;
        private TaskCompletionSource<bool> _roomTcs;
        private TaskCompletionSource<bool> _leftRoomTcs;

        #region Public API
        public async Task<bool> ConnectAsync(string nickname)
        {
            PhotonNetwork.NickName = nickname;
            Debug.Log("Photon AppId: " + PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime);
            Debug.Log("Photon Region: " + PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion);

            if (!PhotonNetwork.IsConnected)
            {
                _connectionTcs = new TaskCompletionSource<bool>();
                _lobbyTcs = new TaskCompletionSource<bool>(); // we’ll also wait for lobby
                StartPhotonConnection();

                // wait until lobby is joined (since it's automatic now)
                return await _lobbyTcs.Task;
            }

            return true;
        }

        public async Task<bool> JoinLobbyAsync()
        {
            if (!PhotonNetwork.InLobby)
            {
                _lobbyTcs = new TaskCompletionSource<bool>();
                PhotonNetwork.JoinLobby();
                return await _lobbyTcs.Task;
            }

            return true;
        }

        public async Task<bool> FindOrCreateRoomAsync(string topicId, int expectedPlayers, int joinTimeoutMs = 5000)
        {
            // If we are in a room (e.g., returning from a previous match), leave it first to avoid race conditions
            if (PhotonNetwork.InRoom)
            {
                _leftRoomTcs = new TaskCompletionSource<bool>();
                PhotonNetwork.LeaveRoom();
                await _leftRoomTcs.Task;
            }

            // Ensure connected
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                await ConnectAsync(PhotonNetwork.NickName ?? "Player");
            }

            // Ensure in lobby before matchmaking
            if (!PhotonNetwork.InLobby)
            {
                await JoinLobbyAsync();
            }

            // Cache topic + expected players for retry in OnJoinRandomFailed
            _pendingTopicId = topicId;
            _pendingMaxPlayers = (byte)expectedPlayers;

            // Set up a new completion source
            _roomTcs = new TaskCompletionSource<bool>();

            // Try to join a room that has the same topic
            var customProps = new ExitGames.Client.Photon.Hashtable { { "topic", topicId } };
            PhotonNetwork.JoinRandomRoom(customProps, _pendingMaxPlayers);

            // Wait until joined or failed
            var completed = await Task.WhenAny(_roomTcs.Task, Task.Delay(joinTimeoutMs));

            if (completed == _roomTcs.Task)
                return _roomTcs.Task.Result;

            Debug.LogError("Room join/create timed out.");
            return false;
        }


        public void LeaveRoom() => PhotonNetwork.LeaveRoom();

        public void SendEvent(byte code, object content, bool reliable = true)
        {
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var sendOptions = new SendOptions { Reliability = reliable };
            PhotonNetwork.RaiseEvent(code, content, raiseEventOptions, sendOptions);
        }
        #endregion

        #region Connection Helpers
        private void StartPhotonConnection()
        {
            Debug.Log("Connecting using Photon settings...");
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.ConnectUsingSettings();
        }
        #endregion

        #region Photon Callbacks

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"Player joined. Count: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
            if (PhotonNetwork.CurrentRoom.PlayerCount >= PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                // Close the room to prevent additional players from joining
                if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                }
                OnAllPlayersReady?.Invoke();
            }
        }

        public void OnEvent(EventData photonEvent) =>
            OnEventReceived?.Invoke(photonEvent.Code, photonEvent.CustomData);

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Photon Master Server, joining lobby...");
            _connectionTcs?.TrySetResult(true);

            // Auto join lobby right away
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("Joined Lobby");
            _lobbyTcs?.TrySetResult(true);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom?.Name}");
            _roomTcs?.TrySetResult(true);

            // If we just filled the room as the second player, fire ready now.
            if (PhotonNetwork.CurrentRoom.PlayerCount >= PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                // Close the room if it is now full
                if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                }
                OnAllPlayersReady?.Invoke();
            }
        }

        public override void OnLeftRoom()
        {
            Debug.Log("Left room");
            _leftRoomTcs?.TrySetResult(true);
            _roomTcs = null;
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.LogWarning($"JoinRandomRoom failed: {message}");

            // Create a room for this topic with the expected capacity
            string roomName = Guid.NewGuid().ToString();
            var roomOptions = new RoomOptions
            {
                MaxPlayers = _pendingMaxPlayers > 0 ? _pendingMaxPlayers : (byte)2,
                CustomRoomProperties = new Hashtable { { "topic", _pendingTopicId ?? "default" } },
                CustomRoomPropertiesForLobby = new string[] { "topic" }
            };

            PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
        }

        public override void OnCreatedRoom()
        {
            Debug.Log($"Created new room: {PhotonNetwork.CurrentRoom?.Name}");
            _roomTcs?.TrySetResult(true);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogError("Disconnected from Photon: " + cause);

            _connectionTcs?.TrySetResult(false);
            _lobbyTcs?.TrySetResult(false);
            _roomTcs?.TrySetResult(false);

            _pendingTopicId = null;
            _pendingMaxPlayers = 0;

        }
        #endregion
    }
}
#endif
