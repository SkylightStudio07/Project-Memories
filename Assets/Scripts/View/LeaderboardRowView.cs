using TMPro;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>One reusable row used by both the world ranking and My Rank panels.</summary>
    public sealed class LeaderboardRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text rankLabel;
        [SerializeField] private TMP_Text playerNameLabel;
        [SerializeField] private TMP_Text scoreLabel;

        public void SetEntry(int zeroBasedRank, string playerName, double score)
        {
            if (rankLabel != null)
                rankLabel.text = $"#{zeroBasedRank + 1:N0}";
            if (playerNameLabel != null)
                playerNameLabel.text = string.IsNullOrWhiteSpace(playerName)
                    ? "ANONYMOUS"
                    : playerName;
            if (scoreLabel != null)
                scoreLabel.text = $"{score:N0}";
        }

        public void SetEmpty(string message)
        {
            if (rankLabel != null) rankLabel.text = "#--";
            if (playerNameLabel != null) playerNameLabel.text = message;
            if (scoreLabel != null) scoreLabel.text = "--";
        }
    }
}
