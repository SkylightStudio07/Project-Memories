using System.Collections;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 스테이지 시작/전환 매니저.
    /// 로스터에서 <see cref="startStageIndex"/>(인스펙터, 테스트용)의 스테이지를 골라
    /// 배경/바닥/적 스프라이트를 바꿔끼우고 <see cref="RoundManager"/>에 스테이지를 넘긴다.
    /// 적 처치(<see cref="RoundManager.OnStageCleared"/>) 시 암전 연출과 함께 다음 스테이지로 넘긴다.
    /// RoundManager.Awake보다 먼저 실행된다(DefaultExecutionOrder).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class StageManager : MonoBehaviour
    {
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
        [Tooltip("씬의 EnemyActor SpriteRenderer — 시작/전환 시 적 스프라이트를 미리 세팅")]
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

        /// <summary>모든 스테이지 클리어(게임 클리어).</summary>
        public event System.Action OnGameCleared;

        public int CurrentIndex { get; private set; } = -1;
        public StageSO CurrentStage => roster != null ? roster.Get(CurrentIndex) : null;

        private bool _transitioning;

        private void Awake()
        {
            ApplyStage(startStageIndex);
        }

        private void OnEnable()
        {
            if (round != null) round.OnStageCleared += HandleStageCleared;
        }

        private void OnDisable()
        {
            if (round != null) round.OnStageCleared -= HandleStageCleared;
        }

        /// <summary>지정 인덱스 스테이지 적용(시작용): 배경/바닥/적 스왑 + RoundManager에 전달.</summary>
        public void ApplyStage(int index)
        {
            StageSO s = roster != null ? roster.Get(index) : null;
            if (s == null)
            {
                Debug.LogWarning($"[Stage] 로스터에 인덱스 {index} 스테이지가 없음 — 기존 설정 유지");
                return;
            }
            CurrentIndex = index;
            ApplyVisuals(s);
            if (round != null) round.SetStage(s); // Awake 경로: RoundManager.Awake가 적용
            Debug.Log($"[Stage] 적용: idx {index} → 스테이지 {s.stageNumber} '{s.displayName}'");
        }

        private void HandleStageCleared()
        {
            if (!_transitioning) StartCoroutine(StageTransition());
        }

        private IEnumerator StageTransition()
        {
            _transitioning = true;
            yield return Fade(1f); // 암전

            int next = CurrentIndex + 1;
            if (roster != null && next < roster.Count && roster.Get(next) != null)
            {
                AdvanceTo(next); // 암전 상태에서 적/배경 교체 + 라운드 재구성(클록 미시작)
                if (holdBlackSeconds > 0f) yield return new WaitForSeconds(holdBlackSeconds);
                yield return Fade(0f); // 새 스테이지 공개
                if (round != null) round.StartRound(); // 공개 후 카운트인 시작(처음부터 보이게)
            }
            else
            {
                Debug.Log("[Stage] ===== 모든 스테이지 클리어 — 게임 클리어 =====");
                if (gameClearScreen != null) gameClearScreen.SetActive(true);
                OnGameCleared?.Invoke();
                // 마지막은 암전(또는 클리어 화면)을 유지한다.
            }

            _transitioning = false;
        }

        /// <summary>런타임: 다음 스테이지 시각 교체 + RoundManager 재시작.</summary>
        private void AdvanceTo(int index)
        {
            StageSO s = roster != null ? roster.Get(index) : null;
            if (s == null) return;
            CurrentIndex = index;
            ApplyVisuals(s);
            if (round != null) round.RestartForStage(s, startClock: false); // 공개 후 StartRound에서 시작
            Debug.Log($"[Stage] 다음 스테이지: idx {index} → 스테이지 {s.stageNumber} '{s.displayName}'");
        }

        private void ApplyVisuals(StageSO s)
        {
            if (background != null && s.backgroundSprite != null) background.sprite = s.backgroundSprite;
            if (floor != null && s.floorSprite != null) floor.sprite = s.floorSprite;
            if (enemyActor != null)
            {
                Sprite es = s.enemySprite != null ? s.enemySprite : FirstPoolSprite(s);
                if (es != null) enemyActor.sprite = es; // 카운트인 전에 즉시 반영
            }
        }

        private IEnumerator Fade(float targetAlpha)
        {
            if (blackout == null) yield break;
            blackout.blocksRaycasts = targetAlpha > 0.5f;
            if (fadeDuration <= 0f) { blackout.alpha = targetAlpha; yield break; }

            float start = blackout.alpha;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                blackout.alpha = Mathf.Lerp(start, targetAlpha, t / fadeDuration);
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
