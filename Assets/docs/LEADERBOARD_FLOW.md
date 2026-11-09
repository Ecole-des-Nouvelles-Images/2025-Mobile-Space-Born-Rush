# Leaderboard Flow — Main Menu to Leaderboard Display

## 1. Numbered Flow: Game Launch → First Leaderboard Display

1. **UIHandler.Start()** (`Assets/__Workspaces/Loan/Script/UIHandler.cs`, line 58–63)
   Coroutine: `yield return null;`, then `yield return StartCoroutine(CheckInternetConnection());`, then `InitializeUI()`.

2. **CheckInternetConnection()** (UIHandler.cs line 109–118)
   Sets `_hasInternetConnection = true` if `Application.internetReachability != NetworkReachability.NotReachable`, otherwise sets it to `false`.

3. **InitializeUI()** (UIHandler.cs line 122–145)
   - Sets `_menuEmpty.SetActive(true)` and `_gameEmpty.SetActive(false)`.
   - Computes `hasUsername = _leaderboardManager && _leaderboardManager.IsUser()` (line 127).
   - `IsUser()` (LeaderboardManager.cs line 110–114) calls `Talo.IdentityCheck()`, which checks `Talo.CurrentAlias != null` (Talo.cs line 191–194).
   - Three branches:
     a. `!_hasInternetConnection` → `OpenGamePanel()` + `return` (guest mode, offline).
     b. `_hasInternetConnection && hasUsername` → `OpenGamePanel()` (already identified, auto-login path).
     c. Otherwise → `OpenUsernamePanel()` + `SetupUsernamePanel()`.

4. **Talo.CurrentAlias becomes non-null**
   - Set only inside `PlayersAPI.HandleIdentifySuccess()` (PlayersAPI.cs line 57): `Talo.CurrentAlias = alias`.
   - Called from `PlayersAPI.Identify()` line 93 after a successful online identify.
   - Also called from `PlayerAuthAPI.DoStartSession()` line 49: `await Talo.Players.Identify("talo", _sessionManager.GetSessionIdentifier())` when a cached session exists.
   - Also from `PlayersAPI.IdentifyOffline()` line 228 when a cached offline alias matches.
   - No other code sets `Talo.CurrentAlias`.

5. **SubmitUsername** (LeaderboardManager.cs line 72–108)
   Called from `UIHandler.OnUsernameSubmitClicked()` line 210 via `_leaderboardManager.SubmitUsername(username)`:
   a. Validates username length ≥ 3 characters (line 74–75).
   b. Calls `Talo.IdentityCheck()` (line 79): if already identified, returns early with "Already identified" message (line 80–81).
   c. `await Talo.Players.Identify("username", username)` (line 86) — makes the HTTP identify call.
   d. On success: re-subscribes `EventBus.OnHighScore` (lines 88–89: `-=`, `+=`).
   e. **New (post-fix):** Calls `_ = LoadPersonalBest()` (line 89) so `_isPersonalBestLoaded` is set after mid-session identification.
   f. Returns `(true, $"Welcome, {Talo.CurrentAlias.identifier}!")` (line 90).
   g. On failure: catches `IdentifyException` (lines 92–102) and generic `Exception` (lines 103–107).

6. **LeaderboardManager.OnEnable / OnDisable** (LeaderboardManager.cs line 25–38)
   - `OnEnable()`: If `IsUser()` → subscribes `EventBus.OnHighScore += OnHighScore` (line 29) and calls `_ = LoadPersonalBest()` (line 30–31). The subscription is gated on `IsUser()` so that score submissions only trigger upload logic when a player is actually identified; a guest player would otherwise hit `IsUser()` inside `SubmitScoreIfHighScore` and just log a warning without ever calling `AddEntry`.
   - `OnDisable()`: Unsubscribes `EventBus.OnHighScore -= OnHighScore` (line 36) and cancels background pagination via `CancelBackgroundLoading()` (line 37).
   - `OnHighScore(int newHighscore)` (line 40–43): calls `_ = SubmitScoreIfHighScore(newHighscore)`.

7. **Full score path** (score → Talo upload)
   a. `ScoringManager.GameOver()` (ScoringManager.cs line 117–128) calls `EventBus.OnHighScore?.Invoke((int)_score)` (line 127).
   b. `LeaderboardManager.OnHighScore(int newHighscore)` (LeaderboardManager.cs line 40–43) calls `_ = SubmitScoreIfHighScore(newHighscore)`.
   c. `SubmitScoreIfHighScore(int score)` (line 144–164):
      - Checks `IsUser()` (line 146): if not identified, logs warning and returns.
      - Checks `IsHighScore(score)` (line 151): if score ≤ cached personal best, logs and returns.
      - Otherwise calls `UploadScore(score, callback)` (line 156).
   d. `UploadScore(int score, Action<bool> callback)` (line 282–309):
      - Checks `IsUser()` (line 284): if not identified, returns `callback(false)`.
      - Calls `Talo.Leaderboards.AddEntry(_leaderboardInternalName, score)` (line 293).
      - On success: updates `_cachedPersonalBest` if `result.Updated && score > _cachedPersonalBest` (lines 296–297).
      - Then refreshes UI: `_ = LoadLeaderboardInitial()` (line 300).
   e. `Talo.Leaderboards.AddEntry()` (LeaderboardsAPI.cs line 88–114):
      - Calls `Talo.IdentityCheck()` (line 90).
      - POSTs to `{baseUrl}/{internalName}/entries` with score and optional props.
      - On success: `_entriesManager.UpsertEntry(internalName, res.entry, true)` (line 102).
      - Returns `AddEntryResult(true, res.entry, res.updated)`.

8. **Full leaderboard display path** (panel → UI)
   a. `UIHandler.OpenScorePanel()` (UIHandler.cs line 422–439):
      - Calls `CheckAndShowUsernamePanelIfNeeded()` (line 425): if online and unidentified, shows username panel first and returns early.
      - Otherwise hides all other panels, shows `_scorePanel` and `_scoreboardPanel`, then `_ = _leaderboardManager.LoadLeaderboardInitial()` (line 438).
   b. `LoadLeaderboardInitial()` (LeaderboardManager.cs line 182–210):
      - Calls `CancelBackgroundLoading()` to cancel any previous background load (line 185).
      - Creates a new `CancellationTokenSource` (line 186).
      - Clears existing UI entries from `_scoreParent` (lines 189–190).
      - Calls `Talo.Leaderboards.GetEntries(_leaderboardInternalName, options)` with `page = 0` (line 195).
      - Calls `PopulateUI(result.entries)` for page 0 (line 198).
      - If `!result.isLastPage`, starts `_ = LoadRemainingPagesInBackground(1, _paginationCts.Token)` (line 203).
   c. `LoadRemainingPagesInBackground(int startPage, CancellationToken token)` (line 216–246):
      - Loop fetching pages sequentially from `startPage`.
      - Each iteration: `Talo.Leaderboards.GetEntries(..., { page = page })` (line 225).
      - `PopulateUI(result.entries)` appends entries to UI (line 231).
      - Breaks when `result.isLastPage` is true (line 233), token is cancelled (line 220/228), or exception occurs (line 238–244).
   d. `PopulateUI(LeaderboardEntry[] entries)` (line 251–262):
      - Instantiates prefab for each entry (line 255).
      - `texts[0].text = (entry.position + 1).ToString()` — rank is now 1-indexed (line 258).
      - `texts[1].text = entry.playerAlias.identifier` (line 259).
      - `texts[2].text = ((int)entry.score).ToString("N0")` (line 260).
   e. Cancellation path via `CancelBackgroundLoading()` (line 268–276): cancels and disposes `_paginationCts`. Called from `OnDisable()` (line 37), `LoadLeaderboardInitial()` (line 185), and `UIHandler.OpenGamePanel()` (line 393–394).

## 2. Current Behaviour vs. Desired Behaviour Table

| Behaviour | Status | Justification |
|-----------|--------|---------------|
| 1. "When the player opens the score panel, ALL scores eventually display — not just the first 50." | Works | `LoadLeaderboardInitial()` (LeaderboardManager.cs line 182) fetches page 0 synchronously and, if `!result.isLastPage`, starts `LoadRemainingPagesInBackground(1, token)` (line 203) which loops until `result.isLastPage` is true (line 233). The background load is fire-and-forget (`_ =` at line 203), so it can be interrupted by cancellation (token from `CancelBackgroundLoading()` at line 268–276, called from `OpenGamePanel()` at UIHandler.cs line 393–394) or by exceptions (line 238–244), but when the panel stays open and connectivity is stable, all pages are fetched. |
| 2. "When the player starts the game and has already claimed a username previously, they auto-login without seeing the username panel." | Partial | `TaloManager.Start()` (line 18–26) calls `Talo.PlayerAuth.StartSession()` when `autoStartSession` is true (default). `PlayerAuthAPI.DoStartSession()` (line 42–50) checks `_sessionManager.CheckForSession()` and if found calls `Talo.Players.Identify("talo", _sessionManager.GetSessionIdentifier())` (line 49). `SessionManager.cs` persists `TaloSessionIdentifier` and `TaloRefreshToken` via `PlayerPrefs` (lines 33, 39, 55, 82, 87). So auto-login works IF `autoStartSession` is enabled in `TaloSettings.cs` (line 33) AND the player previously identified with a cached session stored in PlayerPrefs AND `cachePlayerOnIdentify` is true (`TaloSettings.cs` line 36). However, `UIHandler.Start()` is a coroutine that yields before `InitializeUI()` runs (UIHandler.cs line 60–61), so there is no guaranteed await of `StartSession()` completion before the UI decision is made — if `StartSession()` hasn't resolved by the time `InitializeUI()` runs, `IsUser()` returns false and the username panel appears. |
| 3. "When there is no internet, the game skips the username panel and goes straight to the main menu." | Works | `InitializeUI()` (UIHandler.cs line 130–134): `if (!_hasInternetConnection) { OpenGamePanel(); return; }`. `_hasInternetConnection` is set in `CheckInternetConnection()` (UIHandler.cs line 109–118) which runs as a coroutine in `Start()` before `InitializeUI()` (UIHandler.cs line 61–62). `OpenGamePanel()` (line 390–408) shows the main game panel and hides all other panels including `_usernamePanel`. |

## 3. Where Auto-Login Would Need to Be Added

If Behaviour 2 is deemed not implemented enough for the desired UX (username panel still flashes on fresh launch even after a previous claim), the gap is:

- **Where username persistence already exists:** `SessionManager.cs` (Assets/Talo Game Services/Talo/Runtime/Utils/SessionManager.cs) writes `TaloSessionIdentifier` and `TaloRefreshToken` to `PlayerPrefs` (lines 33, 39, 55, 82, 87) on session creation/refresh.
- **Where the auto-Identify already runs:** `PlayerAuthAPI.DoStartSession()` (PlayerAuthAPI.cs line 42–59) is invoked from `TaloManager.Start()` when `autoStartSession` is true, and it calls `Talo.Players.Identify("talo", ...)` with the cached session identifier (line 49).
- **The gap to fix:** `UIHandler.Start()` (UIHandler.cs line 58–63) is a coroutine that yields before `InitializeUI()` runs, but does not await `Talo.PlayerAuth.StartSession()`. `InitializeUI()` reads `IsUser()` which depends on `Talo.CurrentAlias` being set. If `StartSession()` hasn't resolved yet, `CurrentAlias` is still null, so `IsUser()` returns false and the username panel appears.
- **Shape of fix (documentation only, not implemented):** Before calling `InitializeUI()`, await completion of `Talo.PlayerAuth.StartSession()` (or poll `Talo.HasIdentity()` until it resolves), so that `CurrentAlias` is populated from the cached PlayerPrefs session before the UI decision is made. Alternatively, make `InitializeUI()` async and wait for `Talo.PlayerAuth.StartSession()` to complete before checking `IsUser()`.
