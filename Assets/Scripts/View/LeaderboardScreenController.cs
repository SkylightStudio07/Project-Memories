using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

namespace BeatMemories
{
    /// <summary>
    /// Owns the online leaderboard screen. The battle UI only hands this class
    /// a final score; Unity Gaming Services handles identity and score storage.
    /// </summary>
    public sealed class LeaderboardScreenController : MonoBehaviour
    {
        private const string DefaultLeaderboardId = "global-score";

        [Header("Data")]
        [SerializeField] private string leaderboardId = DefaultLeaderboardId;
        [SerializeField, Range(10, 100)] private int worldEntryLimit = 100;

        [Header("Score UI")]
        [SerializeField] private TMP_Text finalScoreLabel;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private ScrollRect worldRankingScroll;
        [SerializeField] private RectTransform worldRankingContent;
        [SerializeField] private LeaderboardRowView worldRowTemplate;
        [SerializeField] private LeaderboardRowView myRankRow;

        [Header("Nickname UI")]
        [SerializeField] private GameObject nicknamePanel;
        [SerializeField] private TMP_InputField nicknameInput;
        [SerializeField] private TMP_Text nicknameErrorLabel;
        [SerializeField] private Button nicknameConfirmButton;

        [Header("Actions")]
        [SerializeField] private Button submitScoreButton;

        private readonly List<LeaderboardRowView> spawnedRows = new();
        private int finalScore;
        private bool busy;
        private bool submitAfterNickname;

        private void Awake()
        {
            if (submitScoreButton != null)
                submitScoreButton.onClick.AddListener(SubmitScore);
            if (nicknameConfirmButton != null)
                nicknameConfirmButton.onClick.AddListener(ConfirmNickname);
            if (nicknameInput != null)
                nicknameInput.onSubmit.AddListener(_ => ConfirmNickname());

            SetNicknamePanel(false);
            myRankRow?.SetEmpty("NOT SUBMITTED");
        }

        private void OnDestroy()
        {
            if (submitScoreButton != null)
                submitScoreButton.onClick.RemoveListener(SubmitScore);
            if (nicknameConfirmButton != null)
                nicknameConfirmButton.onClick.RemoveListener(ConfirmNickname);
            if (nicknameInput != null)
                nicknameInput.onSubmit.RemoveAllListeners();
        }

        /// <summary>Called by GameOverView after it activates this screen.</summary>
        public void Open(int score)
        {
            finalScore = Mathf.Max(0, score);
            if (finalScoreLabel != null)
                finalScoreLabel.text = finalScore.ToString("N0");

            ClearWorldRows();
            myRankRow?.SetEmpty("CONNECTING...");
            SetStatus("CONNECTING TO WORLD RANKING...", false);
            _ = InitializeAndRefreshAsync();
        }

        public async void SubmitScore()
        {
            if (busy) return;

            try
            {
                SetBusy(true);
                await EnsureSignedInAsync();

                if (!await HasPlayerNameAsync())
                {
                    submitAfterNickname = true;
                    SetNicknamePanel(true);
                    SetStatus("ENTER A CODENAME TO SUBMIT YOUR SCORE.", false);
                    return;
                }

                await SubmitAndRefreshAsync();
            }
            catch (Exception exception)
            {
                ReportFailure("SCORE SUBMISSION FAILED", exception);
            }
            finally
            {
                SetBusy(false);
            }
        }

        public async void ConfirmNickname()
        {
            if (busy) return;

            string nickname = nicknameInput != null
                ? nicknameInput.text.Trim()
                : string.Empty;
            string validationError = ValidateNickname(nickname);
            if (validationError != null)
            {
                SetNicknameError(validationError);
                return;
            }

            try
            {
                SetBusy(true);
                SetNicknameError(string.Empty);
                SetStatus("REGISTERING CODENAME...", false);
                await EnsureSignedInAsync();
                await AuthenticationService.Instance.UpdatePlayerNameAsync(nickname);
                SetNicknamePanel(false);

                if (submitAfterNickname)
                {
                    submitAfterNickname = false;
                    await SubmitAndRefreshAsync();
                }
                else
                {
                    await RefreshAsync();
                }
            }
            catch (Exception exception)
            {
                SetNicknameError("NAME REGISTRATION FAILED. TRY ANOTHER NAME.");
                ReportFailure("CODENAME REGISTRATION FAILED", exception);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task InitializeAndRefreshAsync()
        {
            if (busy) return;

            try
            {
                SetBusy(true);
                await EnsureSignedInAsync();
                bool hasPlayerName = await HasPlayerNameAsync();
                SetNicknamePanel(false);
                await RefreshAsync();
                if (!hasPlayerName)
                    SetStatus("PRESS SUBMIT SCORE TO SET A CODENAME.", false);
            }
            catch (Exception exception)
            {
                ReportFailure("WORLD RANKING UNAVAILABLE", exception);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private static async Task EnsureSignedInAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private static async Task<bool> HasPlayerNameAsync()
        {
            if (!string.IsNullOrWhiteSpace(AuthenticationService.Instance.PlayerName))
                return true;

            try
            {
                string playerName =
                    await AuthenticationService.Instance.GetPlayerNameAsync(false);
                return !string.IsNullOrWhiteSpace(playerName);
            }
            catch
            {
                return false;
            }
        }

        private async Task SubmitAndRefreshAsync()
        {
            SetStatus("UPLOADING SCORE...", false);
            await LeaderboardsService.Instance.AddPlayerScoreAsync(
                leaderboardId,
                finalScore);
            await RefreshAsync();
            SetStatus("SCORE REGISTERED.", false);
        }

        private async Task RefreshAsync()
        {
            SetStatus("LOADING WORLD RANKING...", false);

            LeaderboardScoresPage page =
                await LeaderboardsService.Instance.GetScoresAsync(
                    leaderboardId,
                    new GetScoresOptions
                    {
                        Offset = 0,
                        Limit = worldEntryLimit
                    });

            PopulateWorldRows(page.Results);
            await RefreshMyRankAsync();

            if (page.Results == null || page.Results.Count == 0)
                SetStatus("NO SCORES YET. BE THE FIRST.", false);
            else
                SetStatus($"SHOWING TOP {page.Results.Count:N0}", false);
        }

        private async Task RefreshMyRankAsync()
        {
            try
            {
                LeaderboardEntry entry =
                    await LeaderboardsService.Instance.GetPlayerScoreAsync(
                        leaderboardId);
                myRankRow?.SetEntry(entry.Rank, entry.PlayerName, entry.Score);
            }
            catch
            {
                myRankRow?.SetEmpty("NOT SUBMITTED");
            }
        }

        private void PopulateWorldRows(IReadOnlyList<LeaderboardEntry> entries)
        {
            ClearWorldRows();
            if (worldRowTemplate == null || worldRankingContent == null)
                return;

            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    LeaderboardEntry entry = entries[i];
                    LeaderboardRowView row = Instantiate(
                        worldRowTemplate,
                        worldRankingContent);
                    row.gameObject.SetActive(true);
                    row.SetEntry(entry.Rank, entry.PlayerName, entry.Score);
                    spawnedRows.Add(row);
                }
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(worldRankingContent);
            Canvas.ForceUpdateCanvases();
            if (worldRankingScroll != null)
            {
                worldRankingScroll.StopMovement();
                worldRankingScroll.verticalNormalizedPosition = 1f;
            }
        }

        private void ClearWorldRows()
        {
            for (int i = 0; i < spawnedRows.Count; i++)
                if (spawnedRows[i] != null)
                    Destroy(spawnedRows[i].gameObject);
            spawnedRows.Clear();
        }

        private void SetBusy(bool value)
        {
            busy = value;
            if (submitScoreButton != null)
                submitScoreButton.interactable = !value;
            if (nicknameConfirmButton != null)
                nicknameConfirmButton.interactable = !value;
        }

        private void SetNicknamePanel(bool visible)
        {
            if (nicknamePanel != null)
                nicknamePanel.SetActive(visible);
            if (visible && nicknameInput != null)
            {
                nicknameInput.ActivateInputField();
                nicknameInput.Select();
            }
        }

        private void SetNicknameError(string message)
        {
            if (nicknameErrorLabel != null)
                nicknameErrorLabel.text = message;
        }

        private void SetStatus(string message, bool isError)
        {
            if (statusLabel == null) return;
            statusLabel.text = message;
            statusLabel.color = isError
                ? new Color(1f, 0.22f, 0.25f)
                : new Color(0.25f, 0.95f, 1f);
        }

        private void ReportFailure(string heading, Exception exception)
        {
            SetStatus($"{heading}\n{FriendlyMessage(exception)}", true);
            Debug.LogException(exception, this);
        }

        private string FriendlyMessage(Exception exception)
        {
            string message = exception != null ? exception.Message : string.Empty;
            if (message.IndexOf("leaderboard", StringComparison.OrdinalIgnoreCase) >= 0
                && (message.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0
                    || message.IndexOf("404", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return $"CREATE '{leaderboardId}' IN UNITY DASHBOARD.";
            }

            return "CHECK YOUR NETWORK AND TRY AGAIN.";
        }

        private static string ValidateNickname(string nickname)
        {
            if (nickname.Length < 2)
                return "USE AT LEAST 2 CHARACTERS.";
            if (nickname.Length > 16)
                return "USE 16 CHARACTERS OR FEWER.";

            for (int i = 0; i < nickname.Length; i++)
                if (char.IsWhiteSpace(nickname[i]))
                    return "SPACES ARE NOT ALLOWED.";

            return null;
        }
    }
}
