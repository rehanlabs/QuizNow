using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using QuizGame.Game;
using Ami.BroAudio;

namespace QuizGame.UI
{
    public class ResultsUI : MonoBehaviour
    {
        public GameObject panel;
        public TextMeshProUGUI resultText;
        public TextMeshProUGUI LocalPlayerScoreText, OpponentPlayerScoreText;
        public TextMeshProUGUI LocalPlayerNameText, OpponentPlayerNameText;
        public Button backToLobbyButton;

        private Action onBackToLobby;

        // Sound
        public SoundID winSound;
        public SoundID loseSound;

        public void BindBackToLobby(Action handler)
        {
            onBackToLobby = handler;
            if (backToLobbyButton != null)
            {
                backToLobbyButton.onClick.RemoveAllListeners();
                backToLobbyButton.onClick.AddListener(() => onBackToLobby?.Invoke());
            }
        }

        public void Show(bool youWin, int local, int opp, string localName, string opponentName)
        {
            // Set result text
            if (local == opp)
            {
                resultText.text = "Draw";
                winSound.Play();
            }
            else if (youWin)
            {
                resultText.text = "You Win!";
                winSound.Play(); // play win sound
            }
            else
            {
                resultText.text = "You Lose";
                loseSound.Play(); // play lose sound
            }

            // Update UI
            LocalPlayerScoreText.text = local.ToString();
            OpponentPlayerScoreText.text = opp.ToString();
            LocalPlayerNameText.text = localName;
            OpponentPlayerNameText.text = opponentName;
            panel.SetActive(true);
        }

        public void Hide() => panel.SetActive(false);

        public void ResetUI()
        {
            resultText.text = string.Empty;
            LocalPlayerScoreText.text = string.Empty;
            OpponentPlayerScoreText.text = string.Empty;
            LocalPlayerNameText.text = string.Empty;
            OpponentPlayerNameText.text = string.Empty;
            panel.SetActive(false);
        }
    }
}
