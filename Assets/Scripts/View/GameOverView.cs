using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BeatMemories
{
    /// <summary>
    /// 전투 종료 UI를 표시하고 재시작/타이틀 씬 이동을 담당한다.
    /// 이 컴포넌트는 항상 활성 상태인 오브젝트에 두고, gameOverRoot만 비활성 상태로 시작한다.
    /// </summary>
    public class GameOverView : MonoBehaviour
    {
        private const string TitleSceneName = "Title";

        [Header("참조")]
        [SerializeField] private RoundManager round;
        [SerializeField] private StageManager stageManager;
        [SerializeField] private GameObject gameOverRoot;
        [SerializeField] private GameObject leaderboardButton;
        [SerializeField] private GameObject leaderboardScreen;
        [SerializeField] private LeaderboardScreenController leaderboardController;
        [SerializeField] private GameObject endingSceneBackground;

        [Header("게임오버 시 숨길 전투 UI")]
        [SerializeField] private GameObject[] hideOnGameOver;
        [Tooltip("사망 연출이 끝난 뒤 게임오버 UI를 표시하기까지의 시간.")]
        [SerializeField, Min(0f)] private float showDelay = 3.4f;
        [Tooltip("게임오버 UI가 충격감 있게 등장하는 데 걸리는 시간.")]
        [SerializeField, Min(0.05f)] private float impactDuration = 0.28f;
        [SerializeField, Range(0.1f, 1f)] private float impactStartScale = 0.68f;
        [SerializeField, Min(1f)] private float impactOvershootScale = 1.12f;

        [Header("게임 클리어(승리) 재사용")]
        [Tooltip("최종 클리어 시 배너에 끼울 게임클리어 스프라이트")]
        [SerializeField] private Sprite gameClearBanner;
        [Tooltip("승리 시 스프라이트를 교체할 배너 Image(보통 GameOverBanner)")]
        [SerializeField] private Image bannerImage;
        [Tooltip("승리 시엔 숨길 죽음 연출(스캔라인/글리치/BrokenTimeline 등)")]
        [SerializeField] private GameObject[] hideOnVictory;

        private bool _shown;
        private bool _leaderboardUnlocked;
        private Coroutine _showRoutine;
        private Vector3 _gameOverBaseScale = Vector3.one;

        private void OnEnable()
        {
            if (round != null)
            {
                round.OnGameOver += Show;
                round.OnFinalStageCleared += ShowVictory;
            }
        }

        private void OnDisable()
        {
            if (round != null)
            {
                round.OnGameOver -= Show;
                round.OnFinalStageCleared -= ShowVictory;
            }
            if (_showRoutine != null) StopCoroutine(_showRoutine);
            _showRoutine = null;
            if (gameOverRoot != null) gameOverRoot.transform.DOKill();
        }

        private void Start()
        {
            _shown = false;
            _leaderboardUnlocked = false;
            if (gameOverRoot != null)
            {
                _gameOverBaseScale = gameOverRoot.transform.localScale;
                gameOverRoot.SetActive(false);
            }
            if (leaderboardButton != null) leaderboardButton.SetActive(false);
            if (leaderboardScreen != null) leaderboardScreen.SetActive(false);
            if (endingSceneBackground != null) endingSceneBackground.SetActive(false);
        }

        private void Show()
        {
            if (_shown) return;
            _shown = true;
            _leaderboardUnlocked = false;
            if (endingSceneBackground != null) endingSceneBackground.SetActive(false);
            if (leaderboardButton != null) leaderboardButton.SetActive(false);
            if (leaderboardScreen != null) leaderboardScreen.SetActive(false);
            _showRoutine = StartCoroutine(ShowAfterDeathPresentation());
        }

        private IEnumerator ShowAfterDeathPresentation()
        {
            if (showDelay > 0f) yield return new WaitForSecondsRealtime(showDelay);
            HideCombatUI();
            PresentRoot();
            _showRoutine = null;
        }

        /// <summary>최종 스테이지 클리어(승리) — 같은 패널·버튼 재사용: 배너 교체 + 죽음 연출 숨김, 즉시 등장.</summary>
        public void ShowVictory()
        {
            if (_shown) return;
            _shown = true;
            _leaderboardUnlocked = true;
            if (bannerImage != null && gameClearBanner != null) bannerImage.sprite = gameClearBanner;
            if (endingSceneBackground != null) endingSceneBackground.SetActive(true);
            if (leaderboardButton != null) leaderboardButton.SetActive(true);
            if (hideOnVictory != null)
                foreach (GameObject target in hideOnVictory)
                    if (target != null) target.SetActive(false);
            HideCombatUI();
            PresentRoot();
        }

        private void HideCombatUI()
        {
            if (hideOnGameOver == null) return;
            foreach (GameObject target in hideOnGameOver)
                if (target != null) target.SetActive(false);
        }

        private void PresentRoot()
        {
            if (gameOverRoot == null) return;
            Transform rootTransform = gameOverRoot.transform;
            rootTransform.DOKill();
            rootTransform.localScale = _gameOverBaseScale * impactStartScale;
            gameOverRoot.SetActive(true);
            rootTransform.SetAsLastSibling();

            float punchDuration = Mathf.Max(0.05f, impactDuration);
            Sequence impact = DOTween.Sequence().SetUpdate(true).SetTarget(rootTransform);
            impact.Append(rootTransform
                .DOScale(_gameOverBaseScale * impactOvershootScale, punchDuration * 0.6f)
                .SetEase(Ease.OutBack));
            impact.Append(rootTransform
                .DOScale(_gameOverBaseScale, punchDuration * 0.4f)
                .SetEase(Ease.InOutSine));
        }

        public void RetryCurrentBattle()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || string.IsNullOrEmpty(activeScene.name)) return;
            if (stageManager != null) stageManager.RememberCurrentStageForRetry();
            SceneManager.LoadScene(activeScene.name);
        }

        public void GoToTitle()
        {
            StageManager.ClearPendingRetry();
            SceneManager.LoadScene(TitleSceneName);
        }

        public void OpenLeaderboard()
        {
            if (!_leaderboardUnlocked || leaderboardScreen == null) return;
            if (gameOverRoot != null) gameOverRoot.SetActive(false);
            if (endingSceneBackground != null) endingSceneBackground.SetActive(true);
            leaderboardScreen.SetActive(true);
            leaderboardScreen.transform.SetAsLastSibling();
            leaderboardController?.Open(round != null ? round.Score : 0);
        }

        public void StartNewRun()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || string.IsNullOrEmpty(activeScene.name)) return;
            StageManager.PrepareNewRun();
            SceneManager.LoadScene(activeScene.name);
        }
    }
}
