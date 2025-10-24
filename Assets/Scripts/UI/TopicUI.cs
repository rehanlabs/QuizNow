using UnityEngine;
using System.Linq;
using QuizGame.Models;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

namespace QuizGame.UI
{
    public class TopicUI : MonoBehaviour
    {
        public static TopicUI instance;
        public GameObject topicButtonPrefab; // Assign in Inspector
        public Transform contentParent;      // ScrollView Content object
        public TextMeshProUGUI status;
        public TextMeshProUGUI displayName;
        public GameObject panel;

        [Header("Quick Join Mini Panel")]
        public GameObject quickJoinPanel; // mini panel overlay inside topic UI
        public Button quickJoinButton;    // button to proceed with matchmaking
        public Vector2 quickJoinMargin = new Vector2(8f, 8f);
        public QuizTopic[] topics;
        public QuizTopic selectedTopic;
         void Awake()
        {
            instance = this;
        }

        private UnityAction onQuickJoin;

        public void SetTopics(QuizTopic[] available, UnityAction action)
        {
            topics = available;
            selectedTopic = null;
            onQuickJoin = action;
            if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();

            // Clear existing buttons
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            // Create a button for each topic
            foreach (var topic in available)
            {
                GameObject buttonGO = Instantiate(topicButtonPrefab, contentParent);
                TextMeshProUGUI label = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
                label.text = topic.topicName;

                Button btn = buttonGO.GetComponent<Button>();
                var btnRect = buttonGO.GetComponent<RectTransform>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnTopicButtonClicked(topic, btnRect));
            }

            // Ensure quick join panel is hidden initially
            //if (quickJoinPanel != null) quickJoinPanel.SetActive(false);
        }

        private void OnTopicButtonClicked(QuizTopic topic, RectTransform sourceButton)
        {
            selectedTopic = topic;
            status.text = $"Selected Topic: {topic.topicName}";
            Debug.Log("Selected topic: " + topic.topicId);

            // Show mini panel
            if (quickJoinPanel != null) quickJoinPanel.SetActive(true);

            // Wire quick join to provided action
            if (quickJoinButton != null && onQuickJoin != null)
            {
                quickJoinButton.onClick.RemoveAllListeners();
                quickJoinButton.onClick.AddListener(onQuickJoin);
                // Hide the quick join panel after quick join is triggered
                quickJoinButton.onClick.AddListener(() => { if (quickJoinPanel != null) quickJoinPanel.SetActive(false); });
            }

            // Do not change panel anchoring/positioning; keep its prefab/default position
        }

        public QuizTopic GetSelectedTopic()
        {
            return selectedTopic;
        }

        private Canvas rootCanvas;

        public void ResetUI()
        {
            selectedTopic = null;
            status.text = "";
            if (quickJoinPanel != null) quickJoinPanel.SetActive(false);
        }

        private void PositionMiniPanelAtButtonBottomCorner(RectTransform buttonRect)
        {
            var panelRT = quickJoinPanel.GetComponent<RectTransform>();
            if (panelRT == null) return;

            Camera uiCamera = null;
            if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = rootCanvas.worldCamera;
            }

            var corners = new Vector3[4];
            buttonRect.GetWorldCorners(corners); // 0 BL, 1 TL, 2 TR, 3 BR
            Vector2 screenBL = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
            Vector2 screenBR = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[3]);

            float scale = (rootCanvas != null) ? rootCanvas.scaleFactor : 1f;
            Vector2 panelSize = panelRT.rect.size * scale;

            Vector2 desiredScreen = new Vector2(screenBR.x + quickJoinMargin.x + panelSize.x * 0.5f, screenBL.y + quickJoinMargin.y + panelSize.y * 0.5f);

            if (desiredScreen.x + panelSize.x * 0.5f > Screen.width)
            {
                desiredScreen.x = screenBL.x - quickJoinMargin.x - panelSize.x * 0.5f;
            }

            float halfH = panelSize.y * 0.5f;
            desiredScreen.y = Mathf.Clamp(desiredScreen.y, halfH, Screen.height - halfH);

            var parentRT = panelRT.parent as RectTransform;
            if (parentRT != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, desiredScreen, uiCamera, out Vector2 localPoint))
            {
                panelRT.anchoredPosition = localPoint;
            }
        }
    }
}
