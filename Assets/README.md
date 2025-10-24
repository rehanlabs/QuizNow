# Unity Quiz Starter (PUN2 + PlayFab + PvAI)

This package gives you working scripts and sample data for a lightweight 1v1 quiz game that supports:
- 1v1 online multiplayer via **PUN 2**.
- Automatic fallback to **Player vs AI** if no opponent joins in time.
- MCQ gameplay with a countdown timer, scoring, and result screen.
- **5 quiz topics** to choose from.
- **PlayFab** login/signup (email).

You said you'll configure PUN2 and PlayFab yourself—great! This bundle keeps those as plug-in services so you can slot in your keys and prefabs quickly.

---

## Folder Map
```
Assets/
  Scripts/
    Services/
      IAuthService.cs
      PlayFabAuthService.cs
      INetworkService.cs
      Pun2NetworkService.cs
      OfflineNetworkService.cs
    Models/
      Question.cs
      QuestionBank.cs
    Game/
      QuizGameManager.cs
      Matchmaker.cs
      AIPlayer.cs
      PlayerController.cs
      NetEventCodes.cs
      CoroutineRunner.cs
    UI/
      LoginUI.cs
      LobbyUI.cs
      MatchmakingUI.cs
      QuizUI.cs
      ResultsUI.cs
Resources/
  QuestionBanks/
    topics.json
```

> You can place these anywhere under `Assets/`; the paths above are suggested.

---

## Define Symbols (so it compiles before SDKs are added)
These scripts compile even if PUN2 or PlayFab SDKs aren't imported:

- For **PUN2** code to compile, add scripting define symbol: `PHOTON_INSTALLED`
- For **PlayFab** code to compile, add: `PLAYFAB_INSTALLED`

If you don't add these symbols (and haven't imported SDKs), the services will gracefully use **Offline** stubs instead.

**Unity:** `Project Settings → Player → Other Settings → Scripting Define Symbols`

---

## Setup (Quick)
1. Import **Photon PUN 2** via Package Manager or Asset Store.
2. Import **PlayFab SDK**.
3. Add define symbols (above) or just rely on SDK presence + defines.
4. Create a scene with simple canvases:
   - Login Canvas (email+password, signup/login buttons) → hook `LoginUI`.
   - Lobby Canvas (topic dropdown + "Play 1v1") → hook `LobbyUI`.
   - Matchmaking Canvas (spinner + cancel) → hook `MatchmakingUI`.
   - Quiz Canvas (question text, 4 answer buttons, timer text) → hook `QuizUI`.
   - Results Canvas (scores + rematch/back) → hook `ResultsUI`.
5. Create a **QuestionBank** ScriptableObject per topic or use the provided `topics.json` with `QuestionBank` loader (drag into Resources or load at runtime).
6. In your bootstrap scene, add an empty `GameObject` with `CoroutineRunner`.
7. In `PunNetworkService`, set your Photon AppId (or set via PhotonServerSettings). In `PlayFabAuthService`, set your PlayFab TitleId.

---

## How Online 1v1 Works
- Lobby picks a topic, calls `Matchmaker.FindOrCreateMatch(topic)`.
- PUN2 tries to **join random room** filtered by topic; if none, it **creates** one.
- A **join timeout** (default 10s) waits for an opponent; on timeout, it switches to **PvAI**.
- The **Master** (online) or **Host** (offline) selects questions and broadcasts via network events.
- Players answer per round; answers and scores sync via events.
- After N questions, a results event ends the match.

---

## Notes
- All UI is kept basic & headless (no fancy prefabs). You wire your Canvas and assign fields in the Inspector.
- AI uses a simple answer-probability model by difficulty; tune in `AIPlayer`.
- Replace `// TODO` comments with your ids and polish.
- This is a solid starting point you can extend with cosmetics, power-ups, streaks, leaderboards, etc.

Happy building!
