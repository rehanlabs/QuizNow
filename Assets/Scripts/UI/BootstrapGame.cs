using UnityEngine;
using UnityEngine.UI;
using QuizGame.Services;
using QuizGame.Models;
using TMPro;
using QuizGame.UI;
using QuizGame.Game;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public class BootstrapGame : MonoBehaviour
{
    public static BootstrapGame instance;
    [Header("Panels")]
    public SignupUI signupPanel;
    public LoginUI loginPanel;
    public GuestUI guestPanel;
    public TopicUI topicPanel;
    public QuizUI quizPanel;
    public MatchmakingUI matchmakingPanel;
    public ResultsUI resultsPanel;
    [SerializeField] private LoadingUI loadingUI;

    public Dictionary<string, int> winCounts = new Dictionary<string, int>();

    [Header("UI Elements")]
    public TextMeshProUGUI statusText;

    [SerializeField]
    private PunNetworkService network;
    [SerializeField]
    private PlayFabAuthService auth;
    // We are no longer using LocalQuizService, but the reference here is crucial for the editor.
    // Ensure you drag your PlayFabQuizService script from a GameObject in the scene here.
    [SerializeField]
    public PlayFabQuizService quiz;

    [SerializeField]
    private QuizGameManager gameManager;
    [SerializeField]
    bool isConnectedToPhoton;
    public GameObject LeaderBoardPanel;
    public GameObject ListingPrefab;
    public Transform ListingContainer;

    public GameObject StartPanal;

    public bool IsGuest;

    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (ButtonEventCenter.Instance != null)
        {
            ButtonEventCenter.Instance.OnAnyButtonClicked += OnBackToLobby;
        }
        else
        {
            Debug.LogWarning("ButtonEventCenter not found in scene!");
        }

        // Initialize and load topics from PlayFab
        if (quiz != null)
        {
            quiz.OnTopicsLoaded -= OnTopicsLoaded; // Prevent double subscription
            quiz.OnTopicsLoaded += OnTopicsLoaded;
        }
    }

    // This method is now responsible for populating the topics UI
    private void OnTopicsLoaded(QuizTopic[] topics)
    {
        topicPanel.SetTopics(topics, () => OnTopicSelected());

        if (loadingUI != null)
        {
            loadingUI.SetProgress(0.85f); // topics loaded -> ~85%
            loadingUI.Complete(); // now finish the loading visually
        }
    }

    // -------------------- UI EVENT HANDLERS --------------------

    public async void OnRegisterClicked()
    {
        string email = signupPanel.email.text;
        string username = signupPanel.username.text;
        string pass = signupPanel.password.text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(username))
        {
            signupPanel.status.gameObject.SetActive(true);
            signupPanel.status.color = Color.red;
            signupPanel.status.text = "Enter valid credentials";
            return;
        }

        signupPanel.status.gameObject.SetActive(true);
        signupPanel.status.color = Color.white;
        signupPanel.status.text = "Registering user...";
        bool isLoggedIn = await auth.SignupAsync(email, pass, username);

        if (isLoggedIn)
        {
            signupPanel.status.gameObject.SetActive(true);
            signupPanel.status.text = "Registered! Select Topic";
            signupPanel.status.color = Color.green;
            topicPanel.displayName.text = auth.DisplayName;
            ConnectToPhoton();
        }
        else
        {
            signupPanel.status.gameObject.SetActive(true);
            signupPanel.status.color = Color.red;
            signupPanel.status.text = "Signup failed";
        }
    }

    public async void OnLoginClicked()
    {
        string email = loginPanel.email.text;
        string pass = loginPanel.password.text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            loginPanel.status.gameObject.SetActive(true);
            loginPanel.status.color = Color.red;
            loginPanel.status.text = "Enter valid credentials";
            return;
        }

        loginPanel.status.gameObject.SetActive(true);
        loginPanel.status.color = Color.white;
        loginPanel.status.text = "Logging in...";
        bool isLoggedIn = await auth.LoginAsync(email, pass);

        if (isLoggedIn)
        {
            loginPanel.status.gameObject.SetActive(true);
            loginPanel.status.text = "Logged in! Select Topic";
            loginPanel.status.color = Color.green;
            topicPanel.displayName.text = auth.DisplayName;
            ConnectToPhoton();
        }
        else
        {
            loginPanel.status.gameObject.SetActive(true);
            loginPanel.status.color = Color.red;
            loginPanel.status.text = "Login failed!";
        }
    }

    public async void OnGuestClicked()
    {
        string username = guestPanel.username.text;
        if (string.IsNullOrEmpty(username))
        {
            guestPanel.status.gameObject.SetActive(true);
            guestPanel.status.color = Color.red;
            guestPanel.status.text = "Username Required to join as Guest";
            return;
        }
        guestPanel.status.gameObject.SetActive(true);
        guestPanel.status.color = Color.white;
        guestPanel.status.text = "Logging in...";
        bool isLoggedIn = await auth.LoginWithCustomIdAsync(username);

        if (isLoggedIn)
        {
            guestPanel.status.gameObject.SetActive(true);
            guestPanel.status.text = "Logged in! Select Topic";
            guestPanel.status.color = Color.green;
            topicPanel.displayName.text = auth.DisplayName;
            ConnectToPhoton();
        }
        else
        {
            guestPanel.status.gameObject.SetActive(true);
            guestPanel.status.color = Color.red;
            guestPanel.status.text = "Username Taken!------Login failed!";
        }
    }

    public async void ConnectToPhoton()
    {
        string chosenName = auth.DisplayName;
        Photon.Pun.PhotonNetwork.NickName = chosenName;

        if (loadingUI != null) loadingUI.Show("Connecting to server...");

        isConnectedToPhoton = await network.ConnectAsync(chosenName);

        if (isConnectedToPhoton)
        {
            if (loadingUI != null)
            {
                loadingUI.SetProgress(0.45f);
                loadingUI.UpdateStatus("Loading topics...");
            }

            // This now triggers the async loading process from PlayFab.
            // The rest of the logic moves to OnTopicsLoaded.
            quiz.LoadAllTopics();

            topicPanel.displayName.text = chosenName;
            ShowPanel(topicPanel.panel);

            if (gameManager != null)
            {
                gameManager.SetLocalPlayerId(chosenName);
            }
        }
        else
        {
            if (loadingUI != null) loadingUI.HideImmediate();
            statusText.text = "Failed to connect to Photon.";
        }
    }

    // The PopulateTopicUI method is simplified to just initiate the load process.
    public void PopulateTopicUI()
    {
        quiz.LoadAllTopics();
    }

    public void OnTopicSelected()
    {
        OnTopicSelectedAsync();
    }

    private async void OnTopicSelectedAsync()
    {
        if (topicPanel.selectedTopic == null)
        {
            topicPanel.status.text = "Please select a topic first.";
            return;
        }

        ShowPanel(matchmakingPanel.panel);

        var selected = topicPanel.selectedTopic;
        statusText.text = "Connecting...";

        bool roomJoined = await network.FindOrCreateRoomAsync(selected.topicId, 2, 5000);
        if (roomJoined)
        {
            matchmakingPanel.status.text = "Waiting for opponent...";

            network.OnAllPlayersReady -= OnBothPlayersReady;
            network.OnAllPlayersReady += OnBothPlayersReady;

            async void OnBothPlayersReady()
            {
                network.OnAllPlayersReady -= OnBothPlayersReady;

                statusText.text = "Opponent found! Starting quiz...";

                HookQuizEvents();
                await gameManager.BootAsync(network, selected);
                ShowPanel(quizPanel.panel);
            }
        }
        else
        {
            statusText.text = "Failed to join a match.";
        }
    }

    private void HookQuizEvents()
    {
        quizPanel.Bind(gameManager);

        gameManager.OnGameOver -= HandleGameOver;
        gameManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        if (ButtonEventCenter.Instance != null)
            ButtonEventCenter.Instance.OnAnyButtonClicked -= OnBackToLobby;
    }

    public void OnBackToLobby()
    {
        network.LeaveRoom();

        if (gameManager != null) gameManager.ResetForLobby();
        if (quizPanel != null) quizPanel.ResetUI();
        if (resultsPanel != null) resultsPanel.ResetUI();
        if (topicPanel != null) topicPanel.ResetUI();

        ShowPanel(topicPanel.panel);
    }

    public void OpenLogInPanel()
    {
        StartPanal.SetActive(false);
        ShowPanel(loginPanel.panel);
    }

    public void OpenSignUpPanel()
    {
        StartPanal.SetActive(false);
        ShowPanel(signupPanel.panel);
    }

    public void OpenGuestPanel()
    {
        StartPanal.SetActive(false);
        ShowPanel(guestPanel.panel);
    }

    public async void OnLeaderBoardBTNClicked()
    {
        statusText.text = "Available Leaderboard";
        LeaderBoardPanel.SetActive(true);
        await auth.GetLeaderBoard(ListingPrefab, ListingContainer);
    }

    public void OnLeaderBoardCloseClicked()
    {
        LeaderBoardPanel.SetActive(false);
    }

    private void ShowPanel(GameObject panel)
    {
        signupPanel.panel.SetActive(false);
        loginPanel.panel.SetActive(false);
        topicPanel.panel.SetActive(false);
        quizPanel.panel.SetActive(false);
        guestPanel.panel.SetActive(false);
        matchmakingPanel.panel.SetActive(false);
        if (resultsPanel != null) resultsPanel.panel.SetActive(false);

        panel.SetActive(true);
    }

    private void HandleGameOver(bool localWin, int localScore, int opponentScore)
    {
        if (resultsPanel != null)
        {
            resultsPanel.Show(localWin,
                              localScore,
                              opponentScore,
                              quizPanel.localNameText.text,
                              quizPanel.opponentNameText.text);
            ShowPanel(resultsPanel.panel);
        }
        else
        {
            statusText.text = $"Game Over - {(localWin ? "Win" : (localScore == opponentScore ? "Draw" : "Lose"))} ({localScore}:{opponentScore})";
            ShowPanel(topicPanel.panel);
        }
    }

    public void OnQuizCompleted()
    {
        string topicKey = auth.GetCurrentTopicKey();
        if (!winCounts.ContainsKey(topicKey))
        {
            winCounts[topicKey] = 0;
        }
        winCounts[topicKey]++;
        auth.SaveAllData();
        if (!IsGuest)
        {
            auth.LeaderBoard(winCounts[topicKey], topicKey);
        }
    }

    public void OnQuizFailed()
    {
        string topicKey = auth.GetCurrentTopicKey();
        if (!winCounts.ContainsKey(topicKey))
        {
            winCounts[topicKey] = 0;
        }
        auth.SaveAllData();
        if (!IsGuest)
        {
            auth.LeaderBoard(winCounts[topicKey], topicKey);
        }
    }
}