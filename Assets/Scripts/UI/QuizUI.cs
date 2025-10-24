using UnityEngine;
using QuizGame.Game;
using QuizGame.Models;
using TMPro;
using UnityEngine.UI;

namespace QuizGame.UI
{
    public class QuizUI : MonoBehaviour
    {
        [Header("Question Elements")]
        public TextMeshProUGUI questionText;
        public Button[] optionButtons;
        public TextMeshProUGUI timerText;

        [Header("Scoreboard Elements")]
        public TextMeshProUGUI localNameText;
        public TextMeshProUGUI localScoreText;
        public TextMeshProUGUI opponentNameText;
        public TextMeshProUGUI opponentScoreText;
        public Color32 correctAnsColor;
        public Color32 wrongAnsColor;

        public GameObject panel;
        private QuizGameManager gm;
        private Color[] originalButtonColors;

        public void Bind(QuizGameManager manager)
        {
            gm = manager;

            // Unsubscribe to avoid duplicate bindings
            gm.OnQuestion -= HandleQuestion;
            gm.OnTimer -= HandleTimer;
            gm.OnScores -= HandleScores;
            gm.OnPlayerNames -= HandleNames;
            gm.OnReveal -= HandleReveal;

            // Subscribe
            gm.OnQuestion += HandleQuestion;
            gm.OnTimer += HandleTimer;
            gm.OnScores += HandleScores;
            gm.OnPlayerNames += HandleNames;
            gm.OnReveal += HandleReveal;

            if (originalButtonColors == null || originalButtonColors.Length != optionButtons.Length)
            {
                originalButtonColors = new Color[optionButtons.Length];
                for (int i = 0; i < optionButtons.Length; i++)
                {
                    var img = optionButtons[i].GetComponent<Image>();
                    originalButtonColors[i] = img != null ? img.color : Color.white;
                }
            }
        }

        public void ResetUI()
        {
            questionText.text = "";
            timerText.text = "";
            localNameText.text = "";
            localScoreText.text = "0";
            opponentNameText.text = "";
            opponentScoreText.text = "0";
            ResetOptionStyles();
            for (int i = 0; i < optionButtons.Length; i++)
            {
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].interactable = true;
            }
        }

        private void HandleQuestion(QuestionData q, int index)
        {
            questionText.text = q.prompt;
            ResetOptionStyles();
            for (int i = 0; i < optionButtons.Length; i++)
            {
                var label = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                label.text = q.options[i];
                int idx = i;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() =>
                {
                    for (int b = 0; b < optionButtons.Length; b++) optionButtons[b].interactable = false;
                    gm.SubmitLocalAnswer(idx);
                });
                optionButtons[i].interactable = true;
            }
        }

        private void HandleTimer(float t)
        {
            timerText.text = Mathf.CeilToInt(t).ToString();
        }

        private void HandleNames(string localName, string opponentName)
        {
            localNameText.text = localName;
            opponentNameText.text = opponentName;
        }

        private void HandleScores(int localScore, int opponentScore)
        {
            localScoreText.text = localScore.ToString();
            opponentScoreText.text = opponentScore.ToString();
        }

        private void HandleReveal(int correctIndex, int localSelectedIndex)
        {
            // Disable input during reveal
            for (int i = 0; i < optionButtons.Length; i++)
            {
                optionButtons[i].interactable = false;
            }

            // Color the correct option green
            if (correctIndex >= 0 && correctIndex < optionButtons.Length)
            {
                var img = optionButtons[correctIndex].GetComponent<Image>();
                if (img != null) img.color = correctAnsColor; // green
            }

            // If local chose incorrectly, color their choice red
            if (localSelectedIndex >= 0 && localSelectedIndex < optionButtons.Length && localSelectedIndex != correctIndex)
            {
                var img = optionButtons[localSelectedIndex].GetComponent<Image>();
                if (img != null) img.color = wrongAnsColor; // red
            }
        }

        private void ResetOptionStyles()
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                var img = optionButtons[i].GetComponent<Image>();
                if (img != null)
                {
                    var baseColor = (originalButtonColors != null && i < originalButtonColors.Length) ? originalButtonColors[i] : Color.white;
                    img.color = baseColor;
                }
            }
        }
    }
}
