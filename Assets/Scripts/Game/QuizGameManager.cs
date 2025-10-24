using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using QuizGame.Models;
using QuizGame.Services;
using QuizNet;
using Newtonsoft.Json;
using Ami.BroAudio;

namespace QuizGame.Game
{
    public class QuizGameManager : MonoBehaviour
    {
        public INetworkService Network;
        public bool IsOnline => Network != null && !(Network is OfflineNetworkService);

        private string localPlayerId;
        private string opponentPlayerId = "Opponent"; // placeholder until known

        public List<QuestionData> CurrentQuestions = new List<QuestionData>();
        public int CurrentIndex { get; private set; }
        public int LocalScore { get; private set; }
        public int OpponentScore { get; private set; }

        public float QuestionTimeSeconds = 12f;
        public float RemainingTime { get; private set; }
        public float RevealDelaySeconds = 2f;

        // Events
        public event Action<QuestionData, int> OnQuestion;
        public event Action<float> OnTimer;
        public event Action<string, string> OnPlayerNames; // (localName, opponentName)
        public event Action<int, int> OnScores;            // (localScore, opponentScore)
        public event Action<bool, int, int> OnGameOver;    // (localWin, local, opp)
        public event Action<int, int> OnReveal;            // (correctIndex, localSelectedIndex or -1)

        //Sound
        public SoundID quizTimerSound;

        private bool isMaster = true;
        private bool localAnsweredCurrent;
        private bool opponentAnsweredCurrent;
        private bool started = false; // ensure StartQuestion only runs once
        private int localSelectedIndex = -1;
        private bool WinCountcheck = false;

        private CancellationTokenSource timerCts; // NEW: for canceling timer loops

        // Called from BootstrapGame before connecting (or right after)
        public void SetLocalPlayerId(string id)
        {
            localPlayerId = string.IsNullOrWhiteSpace(id)
                ? ("player-" + UnityEngine.Random.Range(1000, 9999))
                : id;
        }

        public async Task BootAsync(INetworkService network, QuizTopic topic, int totalQuestions = 10, int joinTimeoutMs = 10000)
        {
            Network = network ?? new OfflineNetworkService();
            // Ensure single subscription
            Network.OnEventReceived -= HandleNetEvent;
            Network.OnEventReceived += HandleNetEvent;

            isMaster = Network.IsHost;

            if (Network.IsHost)
            {
                // Initialize names on the host immediately; opponent will be updated when their info arrives
                OnPlayerNames?.Invoke(localPlayerId, opponentPlayerId);

                var rnd = new System.Random();
                var shuffled = topic.questions.OrderBy(_ => rnd.Next()).Take(totalQuestions).ToList();
                CurrentQuestions = shuffled;

                var payload = new StartPayload
                {
                    questions = CurrentQuestions.ToArray(),
                    hostPlayerId = localPlayerId
                };
                string json = JsonConvert.SerializeObject(payload);
                Network.SendEvent(NetEventCodes.StartGame, json);

                // Don't start immediately - wait for the StartGame event to come back
            }
        }

        public void ResetForLobby()
        {
            // Cancel any running timer when resetting
            timerCts?.Cancel();

            // Reset gameplay state so a new match can start cleanly
            CurrentQuestions.Clear();
            CurrentIndex = 0;
            LocalScore = 0;
            OpponentScore = 0;
            RemainingTime = 0f;
            localAnsweredCurrent = false;
            opponentAnsweredCurrent = false;
            localSelectedIndex = -1;
            started = false;
            WinCountcheck = false;
            opponentPlayerId = "Opponent";

            if (Network != null)
            {
                Network.OnEventReceived -= HandleNetEvent;
            }
        }

        [Serializable]
        public class StartPayload
        {
            public QuestionData[] questions;
            public string hostPlayerId;
        }

        [Serializable]
        public class AnswerPayload
        {
            public int qIndex;
            public int choiceIndex;
            public string playerId;
        }

        [Serializable]
        public class PlayerInfoPayload
        {
            public string playerId;
        }

        [Serializable]
        public class RevealPayload
        {
            public int qIndex;
            public int correctIndex;
        }

        private void HandleNetEvent(byte code, object content)
        {
            switch (code)
            {
                case NetEventCodes.StartGame:
                    {
                        string json = content as string;
                        var payload = JsonConvert.DeserializeObject<StartPayload>(json);

                        if (!Network.IsHost)
                        {
                            CurrentQuestions = payload.questions.ToList();

                            if (!string.IsNullOrEmpty(payload.hostPlayerId) && payload.hostPlayerId != localPlayerId)
                            {
                                opponentPlayerId = payload.hostPlayerId;
                                OnPlayerNames?.Invoke(localPlayerId, opponentPlayerId);
                            }

                            var info = new PlayerInfoPayload { playerId = localPlayerId };
                            string infoJson = JsonConvert.SerializeObject(info);
                            Network.SendEvent(NetEventCodes.PlayerInfo, infoJson);
                        }

                        if (!started)
                        {
                            started = true;
                            StartQuestion(0);
                        }
                        break;
                    }

                case NetEventCodes.SubmitAnswer:
                    {
                        string json = content as string;
                        var p = JsonConvert.DeserializeObject<AnswerPayload>(json);

                        if (!string.IsNullOrEmpty(localPlayerId) && p.playerId == localPlayerId)
                            break;

                        if (string.IsNullOrEmpty(opponentPlayerId) || opponentPlayerId == "Opponent")
                        {
                            opponentPlayerId = p.playerId;
                            OnPlayerNames?.Invoke(localPlayerId, opponentPlayerId);
                        }

                        if (opponentAnsweredCurrent)
                            break;

                        opponentAnsweredCurrent = true;
                        if (p.choiceIndex == CurrentQuestions[p.qIndex].correctIndex)
                            OpponentScore++;

                        break;
                    }

                case NetEventCodes.NextQuestion:
                    {
                        int next = Convert.ToInt32(content);
                        StartQuestion(next);
                        break;
                    }

                case NetEventCodes.GameOver:
                    {
                        FinishGame();
                        break;
                    }

                case NetEventCodes.PlayerInfo:
                    {
                        string json = content as string;
                        var pinfo = JsonConvert.DeserializeObject<PlayerInfoPayload>(json);

                        if (!string.IsNullOrEmpty(localPlayerId) && pinfo.playerId == localPlayerId)
                            break;

                        if (string.IsNullOrEmpty(opponentPlayerId) || opponentPlayerId == "Opponent")
                        {
                            opponentPlayerId = pinfo.playerId;
                            OnPlayerNames?.Invoke(localPlayerId, opponentPlayerId);
                        }
                        break;
                    }

                case NetEventCodes.SyncTimer:
                    {
                        if (!Network.IsHost)
                        {
                            float t = Convert.ToSingle(content);
                            RemainingTime = t;
                            OnTimer?.Invoke(Mathf.Max(0f, RemainingTime));
                        }
                        break;
                    }

                case NetEventCodes.Reveal:
                    {
                        string json = content as string;
                        var payload = JsonConvert.DeserializeObject<RevealPayload>(json);

                        if (payload.qIndex == CurrentIndex)
                        {
                            OnReveal?.Invoke(payload.correctIndex, localSelectedIndex);
                            OnScores?.Invoke(LocalScore, OpponentScore);
                        }
                        break;
                    }
            }
        }

        public void SubmitLocalAnswer(int index)
        {
            if (localAnsweredCurrent) return;

            localAnsweredCurrent = true;
            localSelectedIndex = index;
            if (index == CurrentQuestions[CurrentIndex].correctIndex)
                LocalScore++;

            var payload = new AnswerPayload
            {
                qIndex = CurrentIndex,
                choiceIndex = index,
                playerId = localPlayerId
            };

            string json = JsonConvert.SerializeObject(payload);
            Network.SendEvent(NetEventCodes.SubmitAnswer, json);
        }

        private async void StartQuestion(int index)
        {
            // Cancel any running timer before starting new one
            timerCts?.Cancel();
            timerCts = new CancellationTokenSource();

            //Sound
            quizTimerSound.Play();

            CurrentIndex = index;
            RemainingTime = QuestionTimeSeconds;
            localAnsweredCurrent = false;
            opponentAnsweredCurrent = false;
            localSelectedIndex = -1;
            OnQuestion?.Invoke(CurrentQuestions[index], index);

            if (Network.IsHost)
            {
                try
                {
                    while (RemainingTime > 0f)
                    {
                        await Task.Delay(100, timerCts.Token);
                        RemainingTime -= 0.1f;
                        OnTimer?.Invoke(Mathf.Max(0f, RemainingTime));
                        Network.SendEvent(NetEventCodes.SyncTimer, RemainingTime);
                    }
                }
                catch (TaskCanceledException) { return; }
            }

            // Reveal phase
            if (Network.IsHost)
            {
                int correct = CurrentQuestions[index].correctIndex;
                var payload = new RevealPayload { qIndex = index, correctIndex = correct };
                string revealJson = JsonConvert.SerializeObject(payload);
                Network.SendEvent(NetEventCodes.Reveal, revealJson);

                int revealMs = Mathf.RoundToInt(Mathf.Max(0f, RevealDelaySeconds) * 1000f);
                if (revealMs > 0)
                    await Task.Delay(revealMs);
            }

            int next = index + 1;
            if (next < CurrentQuestions.Count)
            {
                if (Network.IsHost)
                {
                    Network.SendEvent(NetEventCodes.NextQuestion, next);
                    StartQuestion(next);
                }
            }
            else
            {
                if (Network.IsHost)
                {
                    Network.SendEvent(NetEventCodes.GameOver, null);
                    FinishGame();
                }
            }
        }

        private void FinishGame()
        {
            if (WinCountcheck) return;
            WinCountcheck = true;
            bool localWin = LocalScore >= OpponentScore;

            if (localWin)
                BootstrapGame.instance.OnQuizCompleted();
            else
                BootstrapGame.instance.OnQuizFailed();

            OnGameOver?.Invoke(localWin, LocalScore, OpponentScore);
        }
    }
}
