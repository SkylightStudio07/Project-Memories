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
        [SerializeField] private HudView hud;
        [Tooltip("복제 씬에서만 사용하는 DSP BGM 로더. 비우면 기존 시작 흐름을 유지한다.")]
        [SerializeField] private RhythmAudioController rhythmAudio;
        [Tooltip("BGM 로딩 완료 후 Round의 DSP 시계를 시작한다. Conductor의 Play On Start를 함께 꺼야 한다.")]
        [SerializeField] private bool gateClockUntilAudioReady;
        [Tooltip("씬의 '백그라운드' SpriteRenderer")]
        [SerializeField] private SpriteRenderer background;
        [Tooltip("씬의 '바닥' SpriteRenderer")]
        [SerializeField] private SpriteRenderer floor;
        [Tooltip("씬의 EnemyActor SpriteRenderer — 시작 시 적 스프라이트를 미리 세팅(카운트인 동안 이전 적 안 남게)")]
        [SerializeField] private SpriteRenderer enemyActor;
        [Tooltip("씬의 PlayerActor SpriteRenderer. 비어 있으면 이름으로 한 번 탐색한다.")]
        [SerializeField] private SpriteRenderer playerActor;
        [SerializeField] private PlayerData playerData;

        private CharacterView playerCharacterInstance;
        private CharacterView enemyCharacterInstance;

        public CharacterView PlayerCharacter => playerCharacterInstance;
        public CharacterView EnemyCharacter => enemyCharacterInstance;
        public SpriteRenderer PlayerActor =>
            playerCharacterInstance != null && playerCharacterInstance.Renderer != null
                ? playerCharacterInstance.Renderer
                : playerActor;
        public SpriteRenderer EnemyActor =>
            enemyCharacterInstance != null && enemyCharacterInstance.Renderer != null
                ? enemyCharacterInstance.Renderer
                : enemyActor;

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
        [Tooltip("스테이지 시작 전(프롤로그)·전환 시 대사 재생. 비우면 대사 없이 바로 카운트인")]
        [SerializeField] private DialogueViewer dialogueViewer;

        public event System.Action OnGameCleared;

        public int CurrentIndex { get; private set; } = -1;
        public StageSO CurrentStage => roster != null ? roster.Get(CurrentIndex) : null;
        private bool transitioning;

        private void Awake()
        {
            if (hud == null) hud = FindFirstObjectByType<HudView>();
            ResolveLegacyActors();
            int initialIndex = pendingRetryStageIndex >= 0
                ? pendingRetryStageIndex
                : startStageIndex;
            pendingRetryStageIndex = -1;
            ApplyStage(initialIndex);
        }

        // 클록은 자동 시작하지 않는다(Conductor.playOnStart=false 전제) — 프롤로그 대사가
        // 끝난 뒤에야(없으면 즉시) 카운트인을 시작해 대사 중 카운팅되는 것을 막는다.
        private IEnumerator Start()
        {
            yield return PlayIntroDialogue();

            if (gateClockUntilAudioReady)
            {
                if (rhythmAudio == null)
                {
                    Debug.LogError(
                        "[Stage] DSP clock start aborted because the " +
                        "audio-ready gate has no RhythmAudioController.",
                        this);
                    yield break;
                }

                yield return rhythmAudio.PrepareCurrentClip();
                if (!rhythmAudio.IsCurrentClipReady)
                {
                    Debug.LogError(
                        "[Stage] DSP clock start aborted because the stage " +
                        "soundtrack did not finish loading.",
                        this);
                    yield break;
                }
            }

            round?.StartRound();
        }

        private IEnumerator PlayIntroDialogue()
        {
            StageSO s = CurrentStage;
            yield return PlayDialogue(s != null ? s.introDialogue : null);
        }

        /// <summary>대사 SO를 재생(비어 있으면 즉시 반환).</summary>
        private IEnumerator PlayDialogue(DialogueSO dialogue)
        {
            if (dialogueViewer != null && dialogue != null)
                yield return dialogueViewer.PlayRoutine(dialogue);
        }

        private void OnEnable()
        {
            if (round != null)
            {
                round.OnStageCleared += HandleStageCleared;
                round.OnEnemyPageDialogueRequested += HandlePageDialogue;
            }
        }

        private void OnDisable()
        {
            if (round != null)
            {
                round.OnStageCleared -= HandleStageCleared;
                round.OnEnemyPageDialogueRequested -= HandlePageDialogue;
            }
        }

        // 보스 페이지 돌입 대사 — RoundManager가 클록을 멈춘 상태로 호출한다.
        private void HandlePageDialogue(int page)
        {
            StartCoroutine(PageDialogueThenResume(page));
        }

        private IEnumerator PageDialogueThenResume(int page)
        {
            yield return PlayDialogue(round != null ? round.GetPageTransitionDialogue(page) : null);
            round?.StartRound(); // 대사 후 카운트인부터 재개
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
            ApplyCharacterPrefabs(s);
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

            if (hud != null)
                yield return hud.WaitForEnemyExit();

            // 적을 처치한 직후 대사(있으면). 다음 스테이지 진행/게임 클리어보다 먼저 나온다.
            StageSO clearedStage = CurrentStage;
            yield return PlayDialogue(clearedStage != null ? clearedStage.outroDialogue : null);

            if (hasNext)
            {
                // Act(1~4) 클리어 배너를 현재 스테이지 위에 연출 → 암전 → 다음 스테이지
                if (stageClearBanner != null && CurrentStage != null)
                    yield return stageClearBanner.PlayActClear(CurrentStage.stageNumber);

                yield return Fade(1f);
                hud?.RestoreEnemyAfterExit();
                ApplyStage(nextIndex);
                if (gateClockUntilAudioReady)
                {
                    if (rhythmAudio == null)
                    {
                        Debug.LogError(
                            "[Stage] Stage transition stopped because the " +
                            "audio-ready gate has no RhythmAudioController.",
                            this);
                        yield return Fade(0f);
                        transitioning = false;
                        yield break;
                    }

                    yield return rhythmAudio.PrepareCurrentClip();
                    if (!rhythmAudio.IsCurrentClipReady)
                    {
                        Debug.LogError(
                            "[Stage] Stage transition stopped because the " +
                            "next soundtrack did not finish loading.",
                            this);
                        yield return Fade(0f);
                        transitioning = false;
                        yield break;
                    }
                }
                if (holdBlackSeconds > 0f)
                    yield return new WaitForSecondsRealtime(holdBlackSeconds);
                yield return Fade(0f);
                // 다음 스테이지가 공개된 채로 대사가 끝난 뒤에야 카운트인을 시작한다.
                yield return PlayIntroDialogue();
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

#if UNITY_EDITOR
        private void OnGUI()
        {
            const float width = 220f;
            const float rowHeight = 34f;
            const float margin = 16f;
            int count = roster != null ? roster.Count : 0;
            if (count <= 0) return;

            Rect panel = new Rect(
                Screen.width - width - margin,
                margin,
                width,
                30f + rowHeight * count);
            GUI.Box(panel, "EDITOR STAGE DEBUG");
            for (int i = 0; i < count; i++)
            {
                StageSO debugStage = roster.Get(i);
                Rect button = new Rect(
                    panel.x + 8f,
                    panel.y + 26f + rowHeight * i,
                    panel.width - 16f,
                    rowHeight - 4f);
                bool previousEnabled = GUI.enabled;
                GUI.enabled = !transitioning && i != CurrentIndex;
                string label = debugStage != null
                    ? $"Stage {debugStage.stageNumber}  {debugStage.displayName}"
                    : $"Stage Index {i}";
                if (GUI.Button(button, label))
                    DebugSwitchStage(i);
                GUI.enabled = previousEnabled;
            }
        }

        private void DebugSwitchStage(int index)
        {
            if (transitioning || roster == null || roster.Get(index) == null) return;

            StopAllCoroutines();
            dialogueViewer?.StopAndHide();
            stageClearBanner?.Hide();
            if (blackout != null)
            {
                blackout.alpha = 0f;
                blackout.blocksRaycasts = false;
            }
            transitioning = true;
            round?.PauseForStageTransition();
            hud?.RestoreEnemyAfterExit();
            ApplyStage(index);
            StartCoroutine(DebugStartStage());
        }

        private IEnumerator DebugStartStage()
        {
            if (gateClockUntilAudioReady)
            {
                if (rhythmAudio == null)
                {
                    Debug.LogError(
                        "[Stage] Debug switch aborted because the audio-ready " +
                        "gate has no RhythmAudioController.",
                        this);
                    transitioning = false;
                    yield break;
                }

                yield return rhythmAudio.PrepareCurrentClip();
                if (!rhythmAudio.IsCurrentClipReady)
                {
                    Debug.LogError(
                        "[Stage] Debug switch aborted because the selected " +
                        "soundtrack did not finish loading.",
                        this);
                    transitioning = false;
                    yield break;
                }
            }

            round?.StartRound();
            transitioning = false;
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

        private void ResolveLegacyActors()
        {
            if (playerData == null)
                playerData = FindFirstObjectByType<PlayerData>();
            if (playerActor == null)
            {
                GameObject actor = GameObject.Find("PlayerActor");
                if (actor != null) playerActor = actor.GetComponent<SpriteRenderer>();
            }
            if (enemyActor == null)
            {
                GameObject actor = GameObject.Find("EnemyActor");
                if (actor != null) enemyActor = actor.GetComponent<SpriteRenderer>();
            }
        }

        private void ApplyCharacterPrefabs(StageSO s)
        {
            ReplaceCharacter(
                s != null ? s.playerPrefab : null,
                playerActor,
                ref playerCharacterInstance);
            ReplaceCharacter(
                s != null ? s.enemyPrefab : null,
                enemyActor,
                ref enemyCharacterInstance);
            if (playerData != null
                && playerCharacterInstance != null
                && playerCharacterInstance.Data is PlayerCharacterData definition)
            {
                playerData.SetCharacterData(definition);
            }
        }

        private static void ReplaceCharacter(
            CharacterView prefab,
            SpriteRenderer legacyActor,
            ref CharacterView instance)
        {
            if (instance != null)
            {
                instance.gameObject.SetActive(false);
                Destroy(instance.gameObject);
                instance = null;
            }

            if (legacyActor == null) return;
            if (prefab == null)
            {
                SetLegacyPresentationEnabled(legacyActor, true);
                return;
            }

            SetLegacyPresentationEnabled(legacyActor, false);
            instance = Instantiate(prefab, legacyActor.transform);
            Transform instanceTransform = instance.transform;
            instanceTransform.localPosition = Vector3.zero;
            instanceTransform.localRotation = Quaternion.identity;
            instanceTransform.localScale = Vector3.one;
        }

        private static void SetLegacyPresentationEnabled(
            SpriteRenderer legacyActor,
            bool enabled)
        {
            legacyActor.enabled = enabled;

            KeyframeAnimator keyframeAnimator =
                legacyActor.GetComponent<KeyframeAnimator>();
            if (keyframeAnimator != null) keyframeAnimator.enabled = enabled;

            Transform legacyShadow = legacyActor.transform.Find("Shadow");
            if (legacyShadow != null)
                legacyShadow.gameObject.SetActive(enabled);
        }
    }
}
