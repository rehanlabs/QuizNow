using System;
using System.Threading.Tasks;
using QuizGame.Services;
using UnityEngine;

namespace QuizGame
{
    public class Matchmaker
    {
        private readonly INetworkService network;
        private readonly IAuthService auth;
        private readonly IQuizService quiz;

        public event Action OnMatchFound;
        public event Action OnAISelected;

        public Matchmaker(INetworkService network, IAuthService auth, IQuizService quiz)
        {
            this.network = network;
            this.auth = auth;
            this.quiz = quiz;
        }

        public async Task StartMatchmakingAsync(string topicId, int expectedPlayers = 2, int joinTimeoutMs = 8000)
        {
            // bool connected = await network.ConnectAsync(auth.DisplayName);
            // if (!connected)
            // {
            //     Debug.LogError("Failed to connect to network service");
            //     return;
            // }

            Debug.Log($"Looking for match on topic: {topicId}");
            await network.FindOrCreateRoomAsync(topicId, expectedPlayers, joinTimeoutMs);

            if (network.PlayerCount >= expectedPlayers)
            {
                Debug.Log("Opponent found — starting multiplayer match");
                OnMatchFound?.Invoke();
            }
            else
            {
                Debug.Log("No opponent — starting PvAI match");
                OnAISelected?.Invoke();
            }
        }
    }
}
