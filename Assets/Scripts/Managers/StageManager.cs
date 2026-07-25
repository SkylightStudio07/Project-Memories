using System.Collections;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 스테이지 시작/전환 매니저.
    /// 로스터에서 <see cref="startStageIndex"/>(인스펙터, 테스트용)의 스테이지를 골라
    /// 배경/바닥 스프라이트를 바꿔끼우고 <see cref="RoundManager"/>에 스테이지를 넘긴다.
    /// RoundManager.Awake보다 먼저 실행된다(DefaultExecutionOrder).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class StageManager : MonoBehaviour
    {
        private static int pendingRetryStageIndex = -1;

        [Header("로스터")]
        [SerializeField] private StageRosterSO roster;
        [Tooltip("시작 스테이지 인덱스(0부터). 적/에셋 테스트용으로 인스펙터에서 변경")]
        [SerializeField, Min(0)] private int startStageIndex = 0;

        [Header("참조")]
        [SerializeField] private RoundManager round;
        [Tooltip("씬의 '백그라운드' SpriteRenderer")]
        [SerializeField] private SpriteRenderer background;
        [Tooltip("씬의 '바닥' SpriteRenderer")]
        [SerializeField] private SpriteRenderer floor;
        [Tooltip("씬의 EnemyActor SpriteRenderer — 시작 시 적 스프라이트를 미리 세팅(카운트인 동안 이전 적 안 남게)")]
        [SerializeField] private SpriteRenderer enemyActor;

        [Header("전환 연출 (인스펙터 조정)")]
        [Tooltip("전체화면 검은 오버레이 CanvasGroup. 비우면 암전 없이 즉시 전환")]
        [SerializeField] private CanvasGroup blackout;
        [Tooltip("암전 페이드 시간(초)")]
        [SerializeField, Min(0f)] private float fadeDuration = 0.5f;
        [Tooltip("완전 암전 상태에서 적/배경을 교체하고 대기하는 시간(초)")]
        [SerializeField, Min(0f)] private float holdBlackSeconds = 0.6f;
        [Tooltip("모든 스테이지 클리어 시 켤 게임 클리어 UI(옵션)")]
        [SerializeField] private GameObject gameClearScreen;
        [Tooltip("Act 1~4 클리어 배너 연출(비우면 배너 없이 전환)")]
        [SerializeField] private StageClearBanner stageClearBanner;

        public event System.Action OnGameCleared;

        public int CurrentIndex { get; private set; } = -1;
        public StageSO CurrentStage => roster != null ? roster.Get(CurrentIndex) : null;
        private bool transitioning;

        private void Awake()
        {
            int initialIndex = pendingRetryStageIndex >= 0
                ? pendingRetryStageIndex
                : startStageIndex;
            pendingRetryStageIndex = -1;
            ApplyStage(initialIndex);
        }

        private void OnEnable()
        {
            if (round != null) round.OnStageCleared += HandleStageCleared;
        }

        private void OnDisable()
        {
            if (round != null) round.OnStageCleared -= HandleStageCleared;
        }

        /// <summary>지정 인덱스 스테이지 적용: 배경/바닥 스왑 + RoundManager에 전달.</summary>
        public void ApplyStage(int index)
        {
            StageSO s = roster != null ? roster.Get(index) : null;
            if (s == null)
            {
                Debug.LogWarning($"[Stage] 로스터에 인덱스 {index} 스테이지가 없음 — 기존 설정 유지");
                return;
            }
            CurrentIndex = index;

            if (background != null && s.backgroundSprite != null) background.sprite = s.backgroundSprite;
            if (floor != null && s.floorSprite != null) floor.sprite = s.floorSprite;
            if (enemyActor != null)
            {
                Sprite es = s.enemySprite != null ? s.enemySprite : FirstPoolSprite(s);
                if (es != null) enemyActor.sprite = es; // 카운트인 전에 즉시 반영
            }
            if (round != null) round.SetStage(s);

            Debug.Log($"[Stage] 적용: idx {index} → 스테이지 {s.stageNumber} '{s.displayName}'");
        }

        private void HandleStageCleared()
        {
            if (!transitioning)
                StartCoroutine(StageTransition());
        }

        private IEnumerator StageTransition()
        {
            transitioning = true;
            round?.PauseForStageTransition();

            int nextIndex = CurrentIndex + 1;
            bool hasNext = roster != null && roster.Get(nextIndex) != null;

            if (hasNext)
            {
                // Act(1~4) 클리어 배너를 현재 스테이지 위에 연출 → 암전 → 다음 스테이지
                if (stageClearBanner != null && CurrentStage != null)
                    yield return stageClearBanner.PlayActClear(CurrentStage.stageNumber);

                yield return Fade(1f);
                ApplyStage(nextIndex);
                if (holdBlackSeconds > 0f)
                    yield return new WaitForSecondsRealtime(holdBlackSeconds);
                yield return Fade(0f);
                round?.StartRound();
            }
            else
            {
                // 최종(Act5): 게임클리어 배너 + 리트라이/타이틀 — GameOverView가 OnFinalStageCleared로 재사용
                round?.StopAtStageClear();
                if (gameClearScreen != null) gameClearScreen.SetActive(true);
                OnGameCleared?.Invoke();
                Debug.Log($"[Stage] 마지막 스테이지 완료: idx {CurrentIndex}");
            }

            transitioning = false;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            const float width = 180f;
            const float height = 44f;
            const float margin = 16f;
            Rect buttonRect = new Rect(Screen.width - width - margin, margin, width, height);
            bool previousEnabled = GUI.enabled;
            GUI.enabled = roster != null && roster.Get(CurrentIndex + 1) != null;

            if (GUI.Button(buttonRect, "DEV  다음 스테이지"))
            {
                HandleStageCleared();
            }

            GUI.enabled = previousEnabled;
        }
#endif

        public void RememberCurrentStageForRetry()
        {
            if (CurrentIndex >= 0) pendingRetryStageIndex = CurrentIndex;
        }

        public static void ClearPendingRetry()
        {
            pendingRetryStageIndex = -1;
        }

        private IEnumerator Fade(float targetAlpha)
        {
            if (blackout == null) yield break;
            blackout.blocksRaycasts = targetAlpha > 0.5f;
            if (fadeDuration <= 0f)
            {
                blackout.alpha = targetAlpha;
                yield break;
            }

            float start = blackout.alpha;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                blackout.alpha = Mathf.Lerp(start, targetAlpha, elapsed / fadeDuration);
                yield return null;
            }
            blackout.alpha = targetAlpha;
        }

        private static Sprite FirstPoolSprite(StageSO s)
        {
            if (s.enemyPool == null) return null;
            for (int i = 0; i < s.enemyPool.Count; i++)
                if (s.enemyPool[i] != null && s.enemyPool[i].Sprite != null) return s.enemyPool[i].Sprite;
            return null;
        }
    }
}
