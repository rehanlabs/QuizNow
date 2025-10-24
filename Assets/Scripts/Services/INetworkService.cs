using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace QuizGame.Services
{
    public interface INetworkService
    {
        bool IsConnected { get; }
        bool IsHost { get; }
        int PlayerCount { get; }

        Task<bool> ConnectAsync(string nickname);
        Task<bool> FindOrCreateRoomAsync(string topicId, int expectedPlayers = 2, int joinTimeoutMs = 10000);
        void LeaveRoom();

        void SendEvent(byte code, object content, bool reliable = true);
        event Action<byte, object> OnEventReceived;
        event Action OnAllPlayersReady;
    }

    // Offline fallback used for PvAI or no SDK installed. Simulates a 2P room with host only.
    public class OfflineNetworkService : INetworkService
    {
        public bool IsConnected { get; private set; }
        public bool IsHost => true;
        public int PlayerCount => 1;

        public event Action<byte, object> OnEventReceived;
        public event Action OnAllPlayersReady;

        public async Task<bool> ConnectAsync(string nickname)
        {
            await Task.Delay(100);
            IsConnected = true;
            return true;
        }

        public async Task<bool> FindOrCreateRoomAsync(string topicId, int expectedPlayers = 2, int joinTimeoutMs = 1000)
        {
            await Task.Delay(joinTimeoutMs);
            OnAllPlayersReady?.Invoke(); // single player ready
            return true;
        }

        public void LeaveRoom() { }

        public void SendEvent(byte code, object content, bool reliable = true)
        {
            // loopback to self
            OnEventReceived?.Invoke(code, content);
        }
    }
}
