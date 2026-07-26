using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace BeatMemories
{
    /// <summary>
    /// P0 HUD/뷰. RoundManager/Conductor/PlayerData 이벤트를 구독해
    /// 캐릭터(월드 <see cref="SpriteRenderer"/>) 색·스프라이트, HUD(하트·8박 인디케이터·페이즈·피드백·충전)를 갱신한다.
    /// 캐릭터는 월드 스프라이트(2.5D 대비), HUD는 Canvas Overlay. 아트 없으면 색 폴백(placeholder 스프라이트 틴트).
    /// 모든 색·연출값은 인스펙터 노출.
    /// </summary>
    public class HudView : MonoBehaviour
    {
        private const int QueueSlotCount = Conductor.BeatsPerMeasure;

        [Header("참조")]
        [SerializeField] private RoundManager round;
        [SerializeField] private Conductor conductor;
        [SerializeField] private PlayerData player;
        [SerializeField] private StageManager stageManager;
        [SerializeField] private CameraSway cameraSway;

        [Header("Player Action Audio")]
        [SerializeField] private PlayerActionAudioSettings playerActionAudioSettings;

        [Header("캐릭터 (월드 SpriteRenderer)")]
        [SerializeField] private SpriteRenderer enemySlot;
        [SerializeField] private SpriteRenderer playerSlot;
        [Tooltip("플레이어 idle 키프레임 애니메이터(옵션). 있으면 idle을 이게 담당")]
        [SerializeField] private KeyframeAnimator playerIdleAnim;
        [Tooltip("스프라이트를 이 월드 높이로 맞춤(원본 비율 유지). 0이면 스케일 고정")]
        [SerializeField] private float enemyWorldHeight = 2.4f;
        [SerializeField] private float playerWorldHeight = 2.0f;

        [Header("HUD (Canvas)")]
        [SerializeField] private Image[] beatDots = new Image[8];
        [Tooltip("HP Fill 이미지(Filled Horizontal). fillAmount = 현재/최대")]
        [SerializeField] private Image hpFill;
        [Tooltip("플레이어 HP 칸(세그먼트). i<현재HP 면 켜짐")]
        [SerializeField] private Image[] hearts;
        [Tooltip("적 HP 칸(세그먼트)")]
        [SerializeField] private Image[] enemyCells;
        [Tooltip("RoundManager가 없을 때만 사용하는 적 HP 표시 기본값")]
        [SerializeField, Min(1)] private int enemyMaxHp = 7;
        [SerializeField] private Text phaseLabel;
        [SerializeField] private Text feedbackLabel;
        [SerializeField] private Text gameOverLabel;
        [SerializeField] private Text countdownLabel;
        [SerializeField] private Text chargeLabel;
        [SerializeField] private Text scoreLabel;

        [Header("스프라이트 점수")]
        [Tooltip("0부터 9 순서의 숫자 Sprite.")]
        [SerializeField] private Sprite[] scoreDigitSprites = new Sprite[10];
        [SerializeField] private Vector2 scoreDigitSize = new Vector2(52f, 52f);
        [SerializeField] private float scoreDigitSpacing = -5f;

        [Header("통합 Queue (기존 Enemy Queue 슬롯 재사용)")]
        [SerializeField] private Image[] enemyQueueSlots = new Image[QueueSlotCount];
        [Tooltip("기존 Player Queue. 통합 후 비활성화만 하며 Scene 오브젝트는 보존")]
        [SerializeField] private Image[] playerQueueSlots = new Image[QueueSlotCount];
        [SerializeField] private Sprite emptyQueueSprite;
        [SerializeField] private Color queueEmptyColor = new Color(0.12f, 0.15f, 0.20f, 0.45f);
        [Tooltip("적이 예고를 차폐했을 때 표시할 노이즈 오버레이. 비우면 빈 슬롯과 동일하게 표시")]
        [SerializeField] private Sprite hiddenQueueSprite;
        [Tooltip("차폐 슬롯 색. 노이즈 오버레이를 원본 색으로 보이려면 흰색")]
        [SerializeField] private Color hiddenQueueColor = Color.white;
        [Tooltip("차폐 노이즈만 슬롯보다 크게 표시할 배율")]
        [SerializeField, Min(0.1f)] private float hiddenQueueScale = 1.2f;
        [SerializeField] private Color queueDamageColor = new Color(0.95f, 0.18f, 0.18f);
        [SerializeField] private Color queueSuccessColor = new Color(0.25f, 0.95f, 0.38f);

        [Header("인디케이터·하트 색")]
        [SerializeField] private Color presentDotColor = new Color(0.95f, 0.80f, 0.32f);
        [SerializeField] private Color responseDotColor = new Color(0.32f, 0.90f, 0.90f);
        [SerializeField] private Color dotOffColor = new Color(0.24f, 0.24f, 0.30f);

        [Header("판정 Floating Text")]
        [SerializeField] private Color timingSuccessColor = new Color(0.3f, 1f, 0.55f);
        [SerializeField] private Color timingEarlyColor = new Color(1f, 0.72f, 0.2f);
        [SerializeField] private Color timingLateColor = new Color(0.65f, 0.72f, 0.8f);
        [SerializeField, Min(0.1f)] private float timingFeedbackDuration = 0.55f;
        [Tooltip("삐끗 텍스트를 플레이어 머리 위로 띄울 추가 월드 높이.")]
        [SerializeField, Min(0f)] private float timingFeedbackHeadOffset = 0.3f;
        [SerializeField, Min(1)] private int timingFeedbackFontSize = 52;
        [SerializeField, Min(0.1f)] private float timingFeedbackStartScale = 0.8f;
        [SerializeField, Min(0.1f)] private float timingFeedbackPopScale = 1.15f;
        [SerializeField, Min(0f)] private float timingFeedbackRiseDistance = 48f;

        [Header("하트 색")]
        [SerializeField] private Color heartFull = new Color(0.90f, 0.26f, 0.36f);
        [SerializeField] private Color heartEmpty = new Color(0.24f, 0.20f, 0.22f);

        [Header("연출 (인스펙터 조정)")]
        [Tooltip("행동/자세 스프라이트 표시 후 idle 복귀까지 시간(초)")]
        [SerializeField, Min(0f)] private float actionSpriteHold = 0.3f;
        [Tooltip("스프라이트 교체 시 종이를 넘기듯 가로로 접혔다 펴지는 시간(초)")]
        [SerializeField, Min(0f)] private float spriteFlipDuration = 0.12f;
        [Tooltip("피격 플래시 지속 시간(초)")]
        [SerializeField, Min(0f)] private float damageFlashDuration = 0.16f;
        [SerializeField, Min(1f)] private float damageFlashDurationMultiplier = 1.25f;
        [Tooltip("피격 플래시 점멸 속도")]
        [SerializeField, Min(0f)] private float damageFlashSpeed = 45f;
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.08f, 0.08f);
        [Tooltip("피격 흔들림 지속 시간(초)")]
        [SerializeField, Min(0f)] private float hitShakeDuration = 0.18f;
        [Tooltip("피격 흔들림 최대 거리(로컬 좌표)")]
        [SerializeField, Min(0f)] private float hitShakeDistance = 0.12f;

        [Header("공격 성공 Effect")]
        [SerializeField, Min(0f)] private float actionEffectDuration = 0.18f;
        [SerializeField, Min(0f)] private float attackLungeDistance = 0.35f;
        [SerializeField, Min(0f)] private float enemyHitShakeStrength = 0.35f;
        [SerializeField, Min(1)] private int enemyHitShakeVibrato = 18;

        [Header("Laser Effect")]
        [Tooltip("PlayerActor 자식 발사 위치의 LineRenderer")]
        [SerializeField] private LineRenderer playerLaser;
        [SerializeField] private LineRenderer playerLaserOuter;
        [Tooltip("EnemyActor 자식 발사 위치의 LineRenderer")]
        [SerializeField] private LineRenderer enemyLaser;
        [SerializeField] private LineRenderer enemyLaserOuter;
        [SerializeField] private Light2D playerLaserMuzzleGlow;
        [SerializeField] private Light2D enemyLaserMuzzleGlow;
        [SerializeField] private Light2D playerLaserHitFlash;
        [SerializeField] private Light2D enemyLaserHitFlash;
        [SerializeField, Min(0.01f)] private float laserWidth = 0.1f;
        [SerializeField, Min(1f)] private float laserOuterWidthMultiplier = 1.9f;
        [SerializeField, Range(0.01f, 0.5f)] private float laserStartWidthRatio = 0.08f;
        [SerializeField, Min(1f)] private float laserFlashWidthMultiplier = 1.8f;
        [SerializeField, Range(0.08f, 0.12f)] private float laserPrepareDuration = 0.1f;
        [SerializeField, Min(0f)] private float laserGrowDuration = 0.08f;
        [SerializeField, Min(0f)] private float laserFadeDuration = 0.1f;
        [SerializeField, Min(0f)] private float laserGlowIntensity = 1.4f;
        [SerializeField, Min(0f)] private float laserHitStopDuration = 0.05f;
        [SerializeField] private Color playerLaserColor = new Color(0.2f, 0.95f, 1f, 1f);
        [SerializeField] private Color enemyLaserColor = new Color(1f, 0.18f, 0.18f, 1f);

        [Header("Charge Effect")]
        [SerializeField] private ChargeAuraEffect chargeAura;
        [SerializeField] private ChargeAuraEffect enemyChargeAura;

        [Header("Guard Shield Effect")]
        [SerializeField] private Sprite guardShieldSprite;
        [SerializeField, Min(0.1f)] private float guardShieldWorldHeight = 2.2f;
        [SerializeField, Min(0.01f)] private float guardShieldDuration = 0.28f;
        [SerializeField] private Color guardShieldColor = Color.white;

        [Header("Player Damage Vignette")]
        [SerializeField] private Volume damageVignetteVolume;
        [SerializeField] private Color damageVignetteColor = new Color(0.7f, 0f, 0f, 1f);
        [SerializeField, Range(0f, 1f)] private float damageVignetteBaseIntensity = 0.22f;
        [SerializeField, Range(0f, 1f)] private float damageVignetteStackIntensity = 0.1f;
        [SerializeField, Range(0f, 1f)] private float damageVignetteMaxIntensity = 0.65f;
        [SerializeField, Min(0.01f)] private float damageVignetteFlashDuration = 0.08f;
        [SerializeField, Min(0f)] private float damageVignetteHoldDuration = 0.08f;
        [SerializeField, Min(0.01f)] private float damageVignetteRestoreDuration = 0.8f;

        [Header("BPM Scale Bounce")]
        [Tooltip("메트로놈 DSP 위상을 직접 샘플링해 프레임 드롭 뒤에도 현재 박자에 재합류합니다.")]
        [SerializeField] private bool useDspSyncedIdleBounce;
        [Tooltip("화면 출력 지연 보정(ms). 양수면 모션을 더 일찍 표시합니다.")]
        [SerializeField, Range(-100f, 100f)] private float visualBeatOffsetMilliseconds;
        [Tooltip("정박 순간 적용할 Y Scale 배율.")]
        [SerializeField, Range(0.8f, 1f)] private float idleBeatSquash = 0.98f;
        [Tooltip("한 박 길이 중 원래 Y Scale로 복원하는 데 사용할 비율.")]
        [SerializeField, Range(0.05f, 0.8f)] private float idleBeatRestoreRatio = 0.22f;
        [SerializeField] private Ease idleBeatRestoreEase = Ease.OutBack;

        [Header("Death Presentation")]
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private CanvasGroup combatUiGroup;
        [SerializeField, Min(0f)] private float deathHitStopDuration = 0.08f;
        [SerializeField, Range(0.35f, 1f)] private float deathCameraZoomRatio = 0.62f;
        [SerializeField, Min(0.01f)] private float deathCameraDuration = 0.22f;
        [SerializeField, Min(0.01f)] private float deathUiFadeDuration = 0.16f;
        [SerializeField, Min(0.01f)] private float deathShakeDuration = 0.24f;
        [SerializeField, Min(0f)] private float deathShakeStrength = 0.18f;
        [SerializeField, Min(0f)] private float explosionInterval = 0.11f;
        [SerializeField] private Vector2[] explosionOffsets =
        {
            new Vector2(-0.18f, 0.12f),
            new Vector2(0.2f, 0.2f),
            new Vector2(0.04f, -0.16f),
        };
        [SerializeField] private float[] explosionScales = { 0.82f, 1.12f, 0.96f };
        [Tooltip("폭발 생성 후 캐릭터가 날아가기 시작할 때까지의 시간.")]
        [SerializeField, Min(0f)] private float explosionFlyDelay = 0.18f;
        [Tooltip("생성된 Explosion Particle을 정리하기까지의 시간. Particle Lifetime과 무관하다.")]
        [SerializeField, Min(0.1f)] private float explosionCleanupDelay = 2f;
        [SerializeField, Min(0.1f)] private float deathFlyDuration = 0.72f;
        [SerializeField, Min(1f)] private float deathFlyDistance = 12f;
        [SerializeField, Min(0f)] private float deathFlyHeight = 4f;
        [SerializeField, Min(0f)] private float deathSpinDegrees = 720f;

        [Header("Floating Score")]
        [SerializeField, Min(0.1f)] private float floatingScoreDuration = 0.65f;
        [SerializeField, Min(1f)] private float floatingScoreMinScale = 1f;
        [SerializeField, Min(1f)] private float floatingScoreMaxScale = 1.8f;
        [SerializeField, Min(1)] private int floatingScoreMaxValue = 500;
        [Tooltip("적 머리 위 기준 Floating 점수 생성 위치.")]
        [SerializeField] private Vector2 floatingScoreBaseOffset = new Vector2(0f, 0.35f);
        [Tooltip("매번 다른 위치에서 나오게 할 X/Y 랜덤 범위.")]
        [SerializeField] private Vector2 floatingScoreRandomOffset = new Vector2(0.3f, 0.18f);
        [SerializeField] private Color hitScoreColor = new Color(0.3f, 1f, 0.55f);
        [SerializeField] private Color clearBonusColor = new Color(1f, 0.82f, 0.25f);

        private int _enemyHp;
        private float enemySpriteTimer;
        private float enemyDamageFlashTimer;
        private Vector3 enemyShakeOffset;
        private Vector3 enemyBaseScale;
        private Coroutine enemyFlipRoutine;
        private float playerSpriteTimer;
        private float playerDamageFlashTimer;
        private float playerShakeTimer;
        private Vector3 playerShakeOffset;
        private Vector3 playerBaseScale;
        private Coroutine playerFlipRoutine;
        private bool presentationInitialized;
        private readonly Enemy[] revealedEnemies = new Enemy[QueueSlotCount];
        private float actionEffectTimer;
        private Vector3 playerActionOffset;
        private Tween playerIdleBounce;
        private Tween enemyIdleBounce;
        private bool playerIdleBounceEnabled;
        private bool enemyIdleBounceEnabled;
        private Coroutine playerDeathRoutine;
        private Coroutine enemyDeathRoutine;
        private bool deathPresentationActive;
        private int floatingScoreOrder;
        private SpriteNumberVisual scoreNumberVisual;
        private SpriteRenderer playerGuardShield;
        private SpriteRenderer enemyGuardShield;
        private Vignette damageVignette;
        private VolumeProfile runtimeVignetteProfile;
        private int previousPlayerHp = -1;
        private int previousPlayerMaxHp = -1;
        private AudioSource playerActionAudio;

        private sealed class SpriteNumberVisual
        {
            public RectTransform Root;
            public CanvasGroup Group;
            public readonly List<Image> Digits = new List<Image>();
        }

        private void Awake()
        {
            if (playerActionAudioSettings == null)
                playerActionAudioSettings = PlayerActionAudioSettings.Load();
            playerActionAudio = gameObject.AddComponent<AudioSource>();
            playerActionAudio.playOnAwake = false;
            playerActionAudio.loop = false;
            playerActionAudio.spatialBlend = 0f;
        }

        private void OnEnable()
        {
            if (round != null)
            {
                round.OnEnemyPreviewed += OnPreview;
                round.OnJudged += OnJudged;
                round.OnTimingJudged += OnTimingJudged;
                round.OnPhaseChanged += OnPhase;
                round.OnCycleStarted += OnCycleStarted;
                round.OnScoreAwarded += OnScoreAwarded;
                round.OnScoreChanged += OnScoreChanged;
                round.OnEnemyHealthChanged += OnEnemyHealthChanged;
                round.OnGameOver += OnGameOver;
                round.OnStageApplied += OnStageApplied;
                round.OnFinalStageCleared += OnFinalStageCleared;
                round.OnEnemyChargedChanged += OnEnemyCharged;
            }
            if (conductor != null) conductor.OnBeat += OnBeat;
            if (player != null)
            {
                player.OnHealthChanged += OnHealth;
                player.OnActionPresented += OnPlayerActionPresented;
                player.OnChargedChanged += OnCharged;
            }
        }

        private void OnDisable()
        {
            if (round != null)
            {
                round.OnEnemyPreviewed -= OnPreview;
                round.OnJudged -= OnJudged;
                round.OnTimingJudged -= OnTimingJudged;
                round.OnPhaseChanged -= OnPhase;
                round.OnCycleStarted -= OnCycleStarted;
                round.OnScoreAwarded -= OnScoreAwarded;
                round.OnScoreChanged -= OnScoreChanged;
                round.OnEnemyHealthChanged -= OnEnemyHealthChanged;
                round.OnGameOver -= OnGameOver;
                round.OnStageApplied -= OnStageApplied;
                round.OnFinalStageCleared -= OnFinalStageCleared;
                round.OnEnemyChargedChanged -= OnEnemyCharged;
            }
            if (conductor != null) conductor.OnBeat -= OnBeat;
            if (player != null)
            {
                player.OnHealthChanged -= OnHealth;
                player.OnActionPresented -= OnPlayerActionPresented;
                player.OnChargedChanged -= OnCharged;
            }

            if (presentationInitialized)
            {
                if (enemyFlipRoutine != null) StopCoroutine(enemyFlipRoutine);
                if (playerFlipRoutine != null) StopCoroutine(playerFlipRoutine);
                enemyFlipRoutine = null;
                playerFlipRoutine = null;
                RestorePresentation(enemySlot, enemyBaseScale, ref enemyShakeOffset);
                RestorePresentation(playerSlot, playerBaseScale, ref playerShakeOffset);
                RestoreOffset(playerSlot, ref playerActionOffset);
            }
            StopLaser(playerLaser, playerLaserOuter);
            StopLaser(enemyLaser, enemyLaserOuter);
            StopPlayerIdleBounce();
            StopEnemyIdleBounce();
            if (playerDeathRoutine != null) StopCoroutine(playerDeathRoutine);
            if (enemyDeathRoutine != null) StopCoroutine(enemyDeathRoutine);
            playerDeathRoutine = null;
            enemyDeathRoutine = null;
            if (combatUiGroup != null)
            {
                combatUiGroup.DOKill();
                combatUiGroup.alpha = 1f;
            }
            cameraSway?.RestoreFocus(0.01f);
            StopEffectLight(playerLaserMuzzleGlow);
            StopEffectLight(enemyLaserMuzzleGlow);
            StopEffectLight(playerLaserHitFlash);
            StopEffectLight(enemyLaserHitFlash);
            StopChargeEffect();
            StopGuardShield(playerGuardShield);
            StopGuardShield(enemyGuardShield);
            StopDamageVignette();
        }

        private void OnDestroy()
        {
            if (playerGuardShield != null) Destroy(playerGuardShield.gameObject);
            if (enemyGuardShield != null) Destroy(enemyGuardShield.gameObject);
            if (runtimeVignetteProfile != null) Destroy(runtimeVignetteProfile);
        }

        private void Start()
        {
            RefreshCharacterViews();
            if (enemySlot != null) enemyBaseScale = enemySlot.transform.localScale;
            if (playerSlot != null) playerBaseScale = playerSlot.transform.localScale;
            presentationInitialized = true;
            if (gameOverLabel != null) gameOverLabel.enabled = false;
            if (countdownLabel != null) countdownLabel.enabled = false;
            if (chargeLabel != null) chargeLabel.enabled = false;
            if (phaseLabel != null) phaseLabel.text = "";
            if (feedbackLabel != null) feedbackLabel.text = "";
            SyncStageHud();
            SetPlayerIdle();
            InitializeScoreDisplay(round != null ? round.Score : 0);
            if (player != null) OnHealth(player.CurrentHp, player.MaxHp);
            OnEnemyHealthChanged(
                round != null ? round.CurrentEnemyHp : enemyMaxHp,
                round != null ? round.EnemyMaxHp : enemyMaxHp);
            SetDots(-1);
            InitializeQueues();
            InitializeLaser(playerLaser, playerLaserOuter, playerLaserMuzzleGlow, playerLaserHitFlash);
            InitializeLaser(enemyLaser, enemyLaserOuter, enemyLaserMuzzleGlow, enemyLaserHitFlash);
            InitializeChargeEffects();
            InitializeGuardShields();
            InitializeDamageVignette();
        }

        private void Update()
        {
            if (countdownLabel != null && conductor != null)
            {
                double t = conductor.TimeUntilStart;
                bool counting = t > 0.001;
                countdownLabel.enabled = counting;
                if (counting) countdownLabel.text = "준비  " + System.Math.Ceiling(t).ToString("0");
            }

            if (enemySlot != null)
            {
                if (enemySpriteTimer > 0f)
                {
                    enemySpriteTimer -= Time.deltaTime;
                    if (enemySpriteTimer <= 0f) SetEnemyIdle();
                }
                enemySlot.color = enemyDamageFlashTimer > 0f
                    ? DamageFlash(Color.white, ref enemyDamageFlashTimer)
                    : Color.white;
            }
            if (playerSlot != null)
            {
                if (playerSpriteTimer > 0f)
                {
                    playerSpriteTimer -= Time.deltaTime;
                    if (playerSpriteTimer <= 0f) SetPlayerIdle();
                }
                playerSlot.color = playerDamageFlashTimer > 0f
                    ? DamageFlash(Color.white, ref playerDamageFlashTimer)
                    : Color.white;
                UpdateShake(playerSlot, ref playerShakeTimer, ref playerShakeOffset);
                UpdateActionMotion();
            }
        }

        private void LateUpdate()
        {
            if (!useDspSyncedIdleBounce) return;

            UpdateDspIdleBounce(
                playerSlot,
                playerBaseScale,
                playerIdleBounceEnabled);
            UpdateDspIdleBounce(
                enemySlot,
                enemyBaseScale,
                enemyIdleBounceEnabled);
        }

        private void OnPreview(EnemyPreviewCue cue)
        {
            int slot = cue.Slot;
            Enemy enemy = cue.VisibleEnemy;
            if (slot >= 0 && slot < revealedEnemies.Length) revealedEnemies[slot] = enemy;

            // 차폐(적 기믹)와 '아직 예고 전인 빈 슬롯'을 구분해서 보여준다.
            // hiddenQueueSprite가 비어 있으면 기존처럼 빈 슬롯과 동일하게 처리.
            bool useNoise = cue.IsHidden && hiddenQueueSprite != null;
            Sprite slotSprite = cue.IsHidden
                ? (useNoise ? hiddenQueueSprite : emptyQueueSprite)
                : QueueSprite(enemy);
            Color slotColor = cue.IsHidden
                ? (useNoise ? hiddenQueueColor : queueEmptyColor)
                : Color.white;
            SetQueueSlot(enemyQueueSlots, slot, slotSprite, slotColor, useNoise ? hiddenQueueScale : 1f);
            if (feedbackLabel != null)
                feedbackLabel.text = cue.IsHidden || enemy == null ? "" : enemy.DisplayName;
        }

        private void OnJudged(int slot, Enemy e, JudgeResult r)
        {
            StopEnemyIdleBounce();
            if (e != null && e.Action == PlayerAction.Attack) StopPlayerIdleBounce();
            if (enemySlot != null) enemySlot.flipX = false;

            if (stageManager?.EnemyCharacter == null && enemyLaser != null && e != null)
                enemyLaser.transform.localPosition = e.LaserOriginOffset;
            if (stageManager?.EnemyCharacter == null && enemyLaserOuter != null && e != null)
                enemyLaserOuter.transform.localPosition = e.LaserOriginOffset;

            if (e != null && e.Sprite != null)
            {
                SetEnemySprite(e.Sprite, true);
                enemySpriteTimer = actionSpriteHold;
            }

            bool playerBeamFired =
                r.Input == PlayerAction.Attack && enemySlot != null && playerLaser != null;
            bool enemyBeamFired =
                e != null && e.Action == PlayerAction.Attack
                && playerSlot != null && enemyLaser != null;
            if (playerBeamFired)
                PlayLaser(
                    playerLaser,
                    playerLaserOuter,
                    playerLaserMuzzleGlow,
                    playerLaserHitFlash,
                    enemySlot,
                    ResolveLaserOrigin(true, playerLaser),
                    ResolveHitPosition(false, enemySlot),
                    playerLaserColor,
                    r.Cleared,
                    round != null && round.CurrentJudgePlayerChargedAttack
                        ? round.ChargedLaserWidthMultiplier
                        : 1f);
            if (enemyBeamFired)
                PlayLaser(
                    enemyLaser,
                    enemyLaserOuter,
                    enemyLaserMuzzleGlow,
                    enemyLaserHitFlash,
                    playerSlot,
                    ResolveLaserOrigin(false, enemyLaser),
                    ResolveHitPosition(true, playerSlot),
                    enemyLaserColor,
                    r.PlayerDamage > 0,
                    round != null && round.CurrentJudgeEnemyChargedAttack
                        ? round.ChargedLaserWidthMultiplier
                        : 1f);
            if (playerBeamFired || enemyBeamFired)
                PlayPlayerVoice(playerActionAudioSettings != null
                    ? playerActionAudioSettings.BeamEffect
                    : null,
                    playerActionAudioSettings != null
                        ? playerActionAudioSettings.BeamVolume
                        : 1f);

            if (r.Input == PlayerAction.Guard)
                PlayGuardShield(playerGuardShield, playerSlot, enemySlot);
            if (e != null && e.Action == PlayerAction.Guard)
                PlayGuardShield(enemyGuardShield, enemySlot, playerSlot);

            PlayResolvedActionEffect(e, r);
            PlayPlayerJudgementVoice(r);

            if (r.Cleared && r.Input == PlayerAction.Attack)
                PlayAttackMotion();

            ResolveQueueSlot(slot, r);
            if (feedbackLabel != null)
                feedbackLabel.text = string.IsNullOrEmpty(r.Feedback) ? $"{r.Input} → {r.Type}" : r.Feedback;
        }

        private void PlayPlayerJudgementVoice(JudgeResult result)
        {
            if (playerActionAudioSettings == null || result.Input == PlayerAction.None)
                return;

            if (result.Input == PlayerAction.Attack)
            {
                if (!result.Cleared)
                {
                    PlayPlayerVoice(playerActionAudioSettings.MistakeVoices);
                    return;
                }

                AudioClip[] attackVoices =
                    round != null && round.CurrentJudgePlayerChargedAttack
                        ? playerActionAudioSettings.ChargedAttackVoices
                        : playerActionAudioSettings.AttackVoices;
                PlayPlayerVoice(attackVoices);
                return;
            }

            bool chargeFailed = result.Input == PlayerAction.Charge
                && result.Type == OutcomeType.Punished;
            bool guardFailed = result.Input == PlayerAction.Guard
                && result.PlayerDamage > 0;
            if (chargeFailed || guardFailed)
                PlayPlayerVoice(playerActionAudioSettings.MistakeVoices);
        }

        private void PlayResolvedActionEffect(Enemy enemy, JudgeResult result)
        {
            if (playerActionAudioSettings == null) return;

            bool playerParried = result.Input == PlayerAction.Guard
                && enemy != null
                && enemy.Action == PlayerAction.Attack
                && enemy.AttackDamage > 0
                && result.PlayerDamage <= 0
                && (round == null || round.CurrentEnemyHp > 0);
            bool enemyParried = result.Input == PlayerAction.Attack
                && enemy != null
                && enemy.Action == PlayerAction.Guard
                && !result.Cleared;
            if (playerParried || enemyParried)
            {
                PlayPlayerVoice(playerActionAudioSettings.ParryEffect);
                return;
            }

            bool playerCharged = result.Input == PlayerAction.Charge
                && result.Type != OutcomeType.Punished;
            bool enemyCharged = enemy != null
                && enemy.Action == PlayerAction.Charge
                && !result.Cleared;
            if (playerCharged || enemyCharged)
                PlayPlayerVoice(playerActionAudioSettings.ChargeEffect);
        }

        private void OnTimingJudged(int slot, RhythmTimingResult result)
        {
            if (result == RhythmTimingResult.Success) return;
            PlayPlayerVoice(
                playerActionAudioSettings != null
                    ? playerActionAudioSettings.MistakeVoices
                    : null);
            if (player != null && player.TimingMistakeSprite != null)
            {
                StopPlayerIdleBounce();
                SetPlayerSprite(player.TimingMistakeSprite, true);
                playerSpriteTimer = actionSpriteHold;
            }
            else
            {
                SetPlayerIdle();
            }

            bool tooEarly = result == RhythmTimingResult.TooEarly;
            ShowTimingFeedback(
                tooEarly ? "빨랐다!" : "느렸다!",
                tooEarly ? timingEarlyColor : timingLateColor);
        }

        private void ShowTimingFeedback(string message, Color color)
        {
            Text template = feedbackLabel != null ? feedbackLabel : scoreLabel;
            if (template == null) return;

            Text floating = Instantiate(template, template.transform.parent);
            floating.name = "TimingFeedback";
            floating.enabled = true;
            floating.raycastTarget = false;
            floating.text = message;
            floating.color = color;
            floating.fontSize = timingFeedbackFontSize;

            RectTransform rect = floating.rectTransform;
            rect.SetAsLastSibling();
            RectTransform parentRect = rect.parent as RectTransform;
            bool positionedAbovePlayer = false;
            Camera worldCamera = Camera.main;
            if (playerSlot != null && parentRect != null && worldCamera != null)
            {
                Canvas canvas = floating.canvas;
                Camera uiCamera = canvas != null
                    && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? canvas.worldCamera
                    : null;
                Vector3 worldPosition =
                    playerSlot.bounds.max + Vector3.up * timingFeedbackHeadOffset;
                Vector2 screenPosition =
                    RectTransformUtility.WorldToScreenPoint(worldCamera, worldPosition);
                positionedAbovePlayer = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    parentRect,
                    screenPosition,
                    uiCamera,
                    out Vector3 canvasWorldPosition);
                if (positionedAbovePlayer) rect.position = canvasWorldPosition;
            }
            if (!positionedAbovePlayer)
                rect.anchoredPosition =
                    template.rectTransform.anchoredPosition + Vector2.down * 48f;
            rect.localScale = Vector3.one * timingFeedbackStartScale;

            Sequence sequence = DOTween.Sequence().SetTarget(floating);
            sequence.Append(rect.DOScale(timingFeedbackPopScale, 0.1f).SetEase(Ease.OutBack));
            sequence.Append(rect.DOAnchorPosY(
                    rect.anchoredPosition.y + timingFeedbackRiseDistance,
                    timingFeedbackDuration)
                .SetEase(Ease.OutCubic));
            sequence.Join(floating.DOFade(0f, timingFeedbackDuration).SetEase(Ease.InQuad));
            sequence.OnComplete(() => Destroy(floating.gameObject));
        }

        private void OnCycleStarted(int cycle)
        {
            ResetQueues();
            SetEnemyIdle();
        }

        private void RefreshEnemyCells()
        {
            if (enemyCells == null) return;
            for (int i = 0; i < enemyCells.Length; i++)
                if (enemyCells[i] != null) enemyCells[i].enabled = i < _enemyHp;
        }

        private void OnEnemyHealthChanged(int currentHp, int maxHp)
        {
            _enemyHp = Mathf.Clamp(currentHp, 0, Mathf.Max(1, maxHp));
            RefreshEnemyCells();
            int currentPage = round != null ? round.CurrentEnemyPage : 1;
            int pageCount = round != null ? round.EnemyPageCount : 1;
            if (ShouldPlayEnemyDeath(_enemyHp, currentPage, pageCount)
                && enemyDeathRoutine == null)
            {
                enemyDeathRoutine = StartCoroutine(BeginEnemyDeathAfterLaser());
            }
        }

        internal static bool ShouldPlayEnemyDeath(
            int currentHp,
            int currentPage,
            int pageCount)
            => currentHp <= 0
               && Mathf.Max(1, currentPage) >= Mathf.Max(1, pageCount);

        private void OnStageApplied(StageSO appliedStage)
        {
            RefreshCharacterViews();
            SyncStageHud(appliedStage);
        }

        private void SyncStageHud(StageSO appliedStage = null)
        {
            StageSO currentStage = appliedStage != null
                ? appliedStage
                : stageManager != null ? stageManager.CurrentStage : null;
            if (currentStage != null)
            {
                enemyIdleSprite = currentStage.enemySprite;
                if (enemyIdleSprite == null && currentStage.enemyPool != null)
                {
                    for (int i = 0; i < currentStage.enemyPool.Count; i++)
                    {
                        Enemy enemy = currentStage.enemyPool[i];
                        if (enemy == null || enemy.Sprite == null) continue;
                        enemyIdleSprite = enemy.Sprite;
                        break;
                    }
                }
            }

            OnEnemyHealthChanged(
                round != null ? round.CurrentEnemyHp : enemyMaxHp,
                round != null ? round.EnemyMaxHp : enemyMaxHp);
            ResetQueues();
            SetEnemyIdle();
        }

        private void OnPhase(int cycle, PhaseSO p)
        {
            if (phaseLabel != null) phaseLabel.text = p != null ? p.PhaseName : "(균등)";
        }

        private void OnGameOver()
        {
            if (gameOverLabel != null) { gameOverLabel.enabled = true; gameOverLabel.text = "GAME OVER"; }
            if (playerDeathRoutine == null)
                playerDeathRoutine = StartCoroutine(BeginPlayerDeath());
        }

        private void OnFinalStageCleared()
        {
            if (gameOverLabel != null)
            {
                gameOverLabel.enabled = true;
                gameOverLabel.text = "STAGE CLEAR";
            }
        }

        private void OnBeat(int beatInCycle)
        {
            SetDots(beatInCycle);
            if (useDspSyncedIdleBounce) return;

            PlayIdleBeatBounce(
                playerSlot,
                playerBaseScale,
                playerIdleBounceEnabled,
                ref playerIdleBounce);
            PlayIdleBeatBounce(
                enemySlot,
                enemyBaseScale,
                enemyIdleBounceEnabled,
                ref enemyIdleBounce);
        }

        private void UpdateDspIdleBounce(
            SpriteRenderer slot,
            Vector3 baseScale,
            bool enabled)
        {
            if (slot == null) return;
            if (!enabled || conductor == null || !conductor.IsRunning)
            {
                RestoreIdleScaleY(slot, baseScale);
                return;
            }

            double beatPosition = conductor.ClockBeatPosition;
            if (conductor.SecondsPerBeat > 0f)
            {
                beatPosition +=
                    visualBeatOffsetMilliseconds
                    / 1000.0
                    / conductor.SecondsPerBeat;
            }

            if (beatPosition < 0.0)
            {
                RestoreIdleScaleY(slot, baseScale);
                return;
            }

            double phase = beatPosition - System.Math.Floor(beatPosition);
            float yRatio = EvaluateIdleBeatScaleRatio(
                (float)phase,
                idleBeatSquash,
                idleBeatRestoreRatio,
                idleBeatRestoreEase);
            Vector3 scale = slot.transform.localScale;
            scale.y = baseScale.y * yRatio;
            slot.transform.localScale = scale;
        }

        private static float EvaluateIdleBeatScaleRatio(
            float beatPhase,
            float squash,
            float restoreRatio,
            Ease ease)
        {
            float clampedRestore = Mathf.Max(0.0001f, restoreRatio);
            if (beatPhase >= clampedRestore) return 1f;

            float progress = Mathf.Clamp01(beatPhase / clampedRestore);
            return DOVirtual.EasedValue(squash, 1f, progress, ease);
        }

        private void OnHealth(int current, int max)
        {
            if (previousPlayerHp >= 0
                && max == previousPlayerMaxHp
                && current < previousPlayerHp)
            {
                PlayDamageVignette(previousPlayerHp - current);
                PlayPlayerVoice(
                    playerActionAudioSettings != null
                        ? playerActionAudioSettings.DamageVoices
                        : null);
            }
            previousPlayerHp = current;
            previousPlayerMaxHp = max;
            if (hpFill != null) hpFill.fillAmount = max > 0 ? (float)current / max : 0f;
            // HP 칸(세그먼트): 현재 HP만큼만 켠다(나머지는 꺼서 빈 칸이 보이게)
            if (hearts != null)
                for (int i = 0; i < hearts.Length; i++)
                    if (hearts[i] != null) hearts[i].enabled = i < current;
        }

        private void OnPlayerActionPresented(PlayerAction action, Sprite sprite)
        {
            StopPlayerIdleBounce();
            if (playerIdleAnim != null) playerIdleAnim.Pause(); // 행동 동안 idle 스텝 정지
            SetPlayerSprite(sprite != null ? sprite : playerSlot != null ? playerSlot.sprite : null, true);
            playerSpriteTimer = actionSpriteHold;
        }

        private void OnCharged(bool charged)
        {
            if (presentationInitialized)
                chargeAura?.SetReady(charged);

            if (chargeLabel != null)
            {
                chargeLabel.enabled = charged;
                if (charged) chargeLabel.text = "⚡ 충전";
            }
        }

        private void PlayPlayerVoice(AudioClip[] clips)
        {
            if (playerActionAudio == null
                || playerActionAudioSettings == null
                || clips == null
                || clips.Length == 0)
                return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip != null)
                playerActionAudio.PlayOneShot(
                    clip,
                    playerActionAudioSettings.Volume);
        }

        private void PlayPlayerVoice(AudioClip clip)
        {
            if (playerActionAudio == null
                || playerActionAudioSettings == null
                || clip == null)
                return;
            playerActionAudio.PlayOneShot(clip, playerActionAudioSettings.Volume);
        }

        private void PlayPlayerVoice(AudioClip clip, float volume)
        {
            if (playerActionAudio == null || clip == null) return;
            playerActionAudio.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        private void OnEnemyCharged(bool charged)
        {
            if (presentationInitialized)
                enemyChargeAura?.SetReady(charged);
        }

        private void SetEnemyIdle()
        {
            StageSO currentStage = stageManager != null ? stageManager.CurrentStage : null;
            if (enemySlot != null) enemySlot.flipX = currentStage != null && currentStage.flipEnemyIdleX;
            SetEnemySprite(enemyIdleSprite); // null이면 현재 placeholder 유지
            StartEnemyIdleBounce();
        }

        private void SetPlayerIdle()
        {
            // 키프레임 idle이 있으면 그쪽에 맡긴다(림버스식 스텝). 없으면 단일 idle 스프라이트 폴백.
            if (playerIdleAnim != null && playerIdleAnim.HasFrames) playerIdleAnim.Resume();
            else SetPlayerSprite(player != null ? player.IdleSprite : null);
            StartPlayerIdleBounce();
        }

        [Header("idle 스프라이트")]
        [Tooltip("적 슬롯 idle(기본) 스프라이트 — 없으면 현재 placeholder 유지")]
        [SerializeField] private Sprite enemyIdleSprite;

        private void SetEnemySprite(Sprite sprite, bool forceFlip = false)
        {
            if (enemySlot == null || sprite == null || (!forceFlip && enemySlot.sprite == sprite)) return;
            if (enemyFlipRoutine != null) StopCoroutine(enemyFlipRoutine);
            enemySlot.transform.localScale = enemyBaseScale;
            enemyFlipRoutine = StartCoroutine(FlipSprite(enemySlot, sprite, enemyBaseScale, false));
        }

        private void SetPlayerSprite(Sprite sprite, bool forceFlip = false)
        {
            if (playerSlot == null || sprite == null || (!forceFlip && playerSlot.sprite == sprite)) return;
            if (playerFlipRoutine != null) StopCoroutine(playerFlipRoutine);
            playerSlot.transform.localScale = playerBaseScale;
            playerFlipRoutine = StartCoroutine(FlipSprite(playerSlot, sprite, playerBaseScale, true));
        }

        private IEnumerator FlipSprite(SpriteRenderer slot, Sprite next, Vector3 baseScale, bool isPlayer)
        {
            float quarterDuration = spriteFlipDuration * 0.25f;
            if (quarterDuration <= 0f)
            {
                slot.sprite = next;
            }
            else
            {
                // 1회차: 정방향 → 접힘 → 좌우 반전. 전체 시간의 절반을 사용한다.
                yield return ScaleSpriteWidth(slot, baseScale, 1f, 0f, quarterDuration);
                slot.sprite = next;
                yield return ScaleSpriteWidth(slot, baseScale, 0f, -1f, quarterDuration);

                // 2회차: 좌우 반전 → 접힘 → 정방향. 총 재생 시간은 기존과 동일하다.
                yield return ScaleSpriteWidth(slot, baseScale, -1f, 0f, quarterDuration);
                yield return ScaleSpriteWidth(slot, baseScale, 0f, 1f, quarterDuration);
            }

            slot.transform.localScale = baseScale;
            if (isPlayer) playerFlipRoutine = null;
            else enemyFlipRoutine = null;
        }

        private static IEnumerator ScaleSpriteWidth(
            SpriteRenderer slot,
            Vector3 baseScale,
            float from,
            float to,
            float duration)
        {
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                float width = Mathf.Lerp(from, to, elapsed / duration);
                slot.transform.localScale = new Vector3(baseScale.x * width, baseScale.y, baseScale.z);
                yield return null;
            }
            slot.transform.localScale = new Vector3(baseScale.x * to, baseScale.y, baseScale.z);
        }

        private void PlayHit(ref float flashTimer, ref float shakeTimer)
        {
            flashTimer = damageFlashDuration * damageFlashDurationMultiplier;
            shakeTimer = hitShakeDuration;
        }

        private Color DamageFlash(Color baseColor, ref float timer)
        {
            if (timer <= 0f || damageFlashDuration <= 0f) return baseColor;

            float totalDuration = damageFlashDuration * damageFlashDurationMultiplier;
            timer = Mathf.Max(0f, timer - Time.deltaTime);
            float elapsed = totalDuration - timer;
            float pulse = Mathf.Abs(Mathf.Cos(elapsed * damageFlashSpeed));
            float strength = pulse * (timer / totalDuration);
            return Color.Lerp(baseColor, damageFlashColor, strength);
        }

        private void InitializeQueues()
        {
            for (int i = 0; i < QueueSlotCount; i++)
            {
                Image enemyQueueSlot = QueueSlot(enemyQueueSlots, i);
                if (enemyQueueSlot != null)
                {
                    enemyQueueSlot.enabled = true;
                    enemyQueueSlot.preserveAspect = true;
                    enemyQueueSlot.raycastTarget = false;
                }

                // 기존 Player Queue 오브젝트는 삭제하지 않고 표시만 끈다.
                Image legacyPlayerSlot = QueueSlot(playerQueueSlots, i);
                if (legacyPlayerSlot != null) legacyPlayerSlot.gameObject.SetActive(false);
            }
            ResetQueues();
        }

        private void ResetQueues()
        {
            for (int i = 0; i < QueueSlotCount; i++)
            {
                revealedEnemies[i] = null;
                SetQueueSlot(enemyQueueSlots, i, emptyQueueSprite, queueEmptyColor);
            }
        }

        private Sprite QueueSprite(Enemy enemy)
        {
            if (enemy == null) return emptyQueueSprite;
            return enemy.Sprite != null ? enemy.Sprite : emptyQueueSprite;
        }

        private static Image QueueSlot(Image[] slots, int index)
            => slots != null && index >= 0 && index < slots.Length ? slots[index] : null;

        // scale은 차폐 노이즈처럼 슬롯보다 크게 보여야 하는 경우에만 1이 아닌 값을 넘긴다.
        // 기본값이 1이라 다른 호출부는 자동으로 원래 크기로 되돌아간다.
        private void SetQueueSlot(Image[] slots, int index, Sprite sprite, Color color, float scale = 1f)
        {
            Image slot = QueueSlot(slots, index);
            if (slot == null) return;
            slot.sprite = sprite != null ? sprite : emptyQueueSprite;
            slot.color = color;
            // 판정 연출(DOPunchScale)은 시작 시점 스케일로 되돌리므로, 살아 있는 트윈을 먼저 끊지 않으면
            // 노이즈(1.2배) → 일반 슬롯(1배) 전환이 다시 1.2배로 덮일 수 있다.
            slot.rectTransform.DOKill();
            slot.rectTransform.localScale = Vector3.one * scale;
        }

        private void ResolveQueueSlot(int slot, JudgeResult result)
        {
            if (slot < 0 || slot >= QueueSlotCount) return;

            bool playerDamaged = result.PlayerDamage > 0;
            bool enemyDamaged = result.Cleared;
            Color resultColor = playerDamaged == enemyDamaged
                ? Color.white
                : (playerDamaged ? queueDamageColor : queueSuccessColor);

            Image queueSlot = QueueSlot(enemyQueueSlots, slot);
            if (queueSlot != null)
            {
                // 차폐(노이즈)됐던 슬롯도 판정이 끝나면 실제 자세를 공개한다.
                // 플레이어가 "무엇이 왔는지"를 사후에 확인하고 학습할 수 있어야 하기 때문.
                Enemy revealed = slot < revealedEnemies.Length ? revealedEnemies[slot] : null;
                if (revealed != null) queueSlot.sprite = QueueSprite(revealed);

                queueSlot.color = resultColor;
                queueSlot.rectTransform.DOKill();
                // 노이즈 확대(hiddenQueueScale)를 원래 크기로 되돌린 뒤 펀치를 시작한다.
                // DOPunchScale은 시작 시점 스케일로 복원하므로 순서가 중요하다.
                queueSlot.rectTransform.localScale = Vector3.one;
                queueSlot.rectTransform.DOPunchScale(Vector3.one * 0.16f, 0.18f, 6, 0.5f);
            }
        }

        private void StartPlayerIdleBounce()
        {
            playerIdleBounceEnabled = !deathPresentationActive && playerSlot != null;
        }

        private void StartEnemyIdleBounce()
        {
            enemyIdleBounceEnabled = !deathPresentationActive && enemySlot != null;
        }

        private void PlayIdleBeatBounce(
            SpriteRenderer slot,
            Vector3 baseScale,
            bool enabled,
            ref Tween activeTween)
        {
            if (!enabled || slot == null || conductor == null) return;

            activeTween?.Kill();
            Transform target = slot.transform;
            Vector3 scale = target.localScale;
            scale.y = baseScale.y * idleBeatSquash;
            target.localScale = scale;

            float restoreDuration =
                Mathf.Max(0.03f, conductor.SecondsPerBeat * idleBeatRestoreRatio);
            activeTween = DOTween.To(
                    () => target.localScale.y,
                    value =>
                    {
                        Vector3 current = target.localScale;
                        current.y = value;
                        target.localScale = current;
                    },
                    baseScale.y,
                    restoreDuration)
                .SetEase(idleBeatRestoreEase)
                .SetTarget(slot);
        }

        private void StopPlayerIdleBounce()
        {
            playerIdleBounce?.Kill();
            playerIdleBounce = null;
            playerIdleBounceEnabled = false;
            if (presentationInitialized) RestoreIdleScaleY(playerSlot, playerBaseScale);
        }

        private void StopEnemyIdleBounce()
        {
            enemyIdleBounce?.Kill();
            enemyIdleBounce = null;
            enemyIdleBounceEnabled = false;
            if (presentationInitialized) RestoreIdleScaleY(enemySlot, enemyBaseScale);
        }

        private static void RestoreIdleScaleY(SpriteRenderer slot, Vector3 baseScale)
        {
            if (slot == null) return;
            Vector3 scale = slot.transform.localScale;
            scale.y = baseScale.y;
            slot.transform.localScale = scale;
        }

        private void PlayAttackMotion()
        {
            actionEffectTimer = actionEffectDuration;
        }

        private void PlayEnemyHit()
        {
            enemyDamageFlashTimer = damageFlashDuration * damageFlashDurationMultiplier;
            if (enemySlot == null) return;
            enemySlot.transform.DOKill();
            enemySlot.transform.DOShakePosition(
                hitShakeDuration,
                enemyHitShakeStrength,
                enemyHitShakeVibrato,
                90f,
                false,
                true,
                ShakeRandomnessMode.Harmonic);
        }

        private void InitializeLaser(
            LineRenderer laser,
            LineRenderer outerLaser,
            Light2D muzzleGlow,
            Light2D hitFlash)
        {
            if (laser == null) return;
            InitializeLaserLine(laser, laserWidth);
            if (outerLaser != null)
            {
                InitializeLaserLine(outerLaser, laserWidth * laserOuterWidthMultiplier);
                outerLaser.sortingOrder = laser.sortingOrder - 1;
            }
            InitializeEffectLight(muzzleGlow);
            InitializeEffectLight(hitFlash);
        }

        private void InitializeLaserLine(LineRenderer laser, float width)
        {
            laser.useWorldSpace = true;
            laser.positionCount = 2;
            laser.widthMultiplier = width;
            laser.widthCurve = new AnimationCurve(
                new Keyframe(0f, laserStartWidthRatio),
                new Keyframe(0.18f, 0.72f),
                new Keyframe(1f, 1f));
            laser.enabled = false;
        }

        private void RefreshCharacterViews()
        {
            if (stageManager == null)
                stageManager = FindFirstObjectByType<StageManager>();

            SpriteRenderer nextPlayer =
                stageManager != null ? stageManager.PlayerActor : null;
            SpriteRenderer nextEnemy =
                stageManager != null ? stageManager.EnemyActor : null;

            if (nextPlayer != null && nextPlayer != playerSlot)
            {
                StopPlayerIdleBounce();
                playerSlot = nextPlayer;
                playerBaseScale = playerSlot.transform.localScale;
                CharacterView view = stageManager.PlayerCharacter;
                KeyframeAnimator prefabAnimator =
                    view != null ? view.GetComponent<KeyframeAnimator>() : null;
                if (prefabAnimator != null) playerIdleAnim = prefabAnimator;
                if (presentationInitialized)
                {
                    chargeAura?.Initialize(playerSlot, playerLaserColor);
                    chargeAura?.SetReady(player != null && player.IsCharged);
                }
            }

            if (nextEnemy != null && nextEnemy != enemySlot)
            {
                StopEnemyIdleBounce();
                enemySlot = nextEnemy;
                enemyBaseScale = enemySlot.transform.localScale;
                if (presentationInitialized)
                {
                    enemyChargeAura?.Initialize(enemySlot, enemyLaserColor);
                    enemyChargeAura?.SetReady(round != null && round.IsEnemyCharged);
                }
            }
        }

        private Vector3 ResolveLaserOrigin(bool playerSource, LineRenderer fallback)
        {
            CharacterView view = stageManager != null
                ? playerSource
                    ? stageManager.PlayerCharacter
                    : stageManager.EnemyCharacter
                : null;
            if (view != null)
                return view.GetAnchorPosition(CharacterAnchorType.LaserMuzzle);
            return fallback != null ? fallback.transform.position : Vector3.zero;
        }

        private Vector3 ResolveCurrentLaserOrigin(LineRenderer laser, Vector3 fallback)
        {
            if (laser == playerLaser) return ResolveLaserOrigin(true, laser);
            if (laser == enemyLaser) return ResolveLaserOrigin(false, laser);
            return fallback;
        }

        private Vector3 ResolveHitPosition(bool playerTarget, SpriteRenderer fallback)
        {
            CharacterView view = stageManager != null
                ? playerTarget
                    ? stageManager.PlayerCharacter
                    : stageManager.EnemyCharacter
                : null;
            if (view != null)
                return view.GetAnchorPosition(CharacterAnchorType.Hit);
            return fallback != null ? fallback.bounds.center : Vector3.zero;
        }

        private void PlayLaser(
            LineRenderer laser,
            LineRenderer outerLaser,
            Light2D muzzleGlow,
            Light2D hitFlash,
            SpriteRenderer targetActor,
            Vector3 origin,
            Vector3 target,
            Color color,
            bool hit,
            float widthMultiplier)
        {
            if (laser == null || targetActor == null) return;

            float activeWidth = laserWidth * Mathf.Max(1f, widthMultiplier);
            StopLaser(laser, outerLaser);
            StopEffectLight(muzzleGlow);
            laser.transform.position = origin;
            if (outerLaser != null) outerLaser.transform.position = origin;
            if (muzzleGlow != null) muzzleGlow.transform.position = origin;
            float progress = 0f;
            float alpha = 1f;
            laser.widthMultiplier = activeWidth * laserFlashWidthMultiplier;
            SetLaserColor(laser, color, alpha);
            if (outerLaser != null)
            {
                outerLaser.widthMultiplier =
                    activeWidth * laserOuterWidthMultiplier * laserFlashWidthMultiplier;
                SetLaserColor(outerLaser, Color.white, alpha);
            }
            SetLaserPositions(laser, outerLaser, origin, origin);

            Sequence sequence = DOTween.Sequence().SetTarget(laser);
            if (muzzleGlow != null)
            {
                muzzleGlow.color = color;
                muzzleGlow.enabled = true;
                muzzleGlow.intensity = 0f;
                muzzleGlow.transform.localScale = Vector3.one * 0.75f;
                sequence.Append(DOTween.To(
                    () => muzzleGlow.intensity,
                    value => muzzleGlow.intensity = value,
                    laserGlowIntensity,
                    laserPrepareDuration).SetEase(Ease.OutQuad));
                sequence.Join(muzzleGlow.transform.DOScale(1.15f, laserPrepareDuration)
                    .SetEase(Ease.OutBack));
            }
            else
            {
                sequence.AppendInterval(laserPrepareDuration);
            }

            sequence.AppendCallback(() =>
            {
                if (outerLaser != null) outerLaser.enabled = true;
                laser.enabled = true;
            });
            sequence.Append(DOTween.To(
                () => progress,
                value =>
                {
                    progress = value;
                    Vector3 currentOrigin = ResolveCurrentLaserOrigin(laser, origin);
                    SetLaserPositions(
                        laser,
                        outerLaser,
                        currentOrigin,
                        Vector3.Lerp(origin, target, value));
                },
                1f,
                laserGrowDuration).SetEase(Ease.OutCubic));
            sequence.Join(DOTween.To(
                () => laser.widthMultiplier,
                value => laser.widthMultiplier = value,
                activeWidth,
                laserGrowDuration).SetEase(Ease.OutQuad));
            if (outerLaser != null)
                sequence.Join(DOTween.To(
                    () => outerLaser.widthMultiplier,
                    value => outerLaser.widthMultiplier = value,
                    activeWidth * laserOuterWidthMultiplier,
                    laserGrowDuration).SetEase(Ease.OutQuad));
            if (muzzleGlow != null)
                sequence.Join(muzzleGlow.transform.DOScale(1f, laserGrowDuration)
                    .SetEase(Ease.OutQuad));
            sequence.AppendCallback(() =>
            {
                PlayLaserImpact(hitFlash, targetActor, target, color, hit);
            });
            sequence.Append(DOTween.To(
                () => alpha,
                value =>
                {
                    alpha = value;
                    SetLaserColor(laser, color, value);
                    if (outerLaser != null) SetLaserColor(outerLaser, Color.white, value);
                },
                0f,
                laserFadeDuration).SetEase(Ease.InQuad));
            if (outerLaser != null)
                sequence.Join(DOTween.To(
                    () => outerLaser.widthMultiplier,
                    value => outerLaser.widthMultiplier = value,
                    0f,
                    laserFadeDuration).SetEase(Ease.InQuad));
            sequence.Join(DOTween.To(
                () => laser.widthMultiplier,
                value => laser.widthMultiplier = value,
                0f,
                laserFadeDuration).SetEase(Ease.InQuad));
            if (muzzleGlow != null)
                sequence.Join(DOTween.To(
                    () => muzzleGlow.intensity,
                    value => muzzleGlow.intensity = value,
                    0f,
                    laserFadeDuration));
            sequence.OnComplete(() =>
            {
                laser.enabled = false;
                laser.widthMultiplier = laserWidth;
                if (outerLaser != null)
                {
                    outerLaser.enabled = false;
                    outerLaser.widthMultiplier = laserWidth * laserOuterWidthMultiplier;
                }
                if (muzzleGlow != null)
                {
                    muzzleGlow.enabled = false;
                    muzzleGlow.transform.localScale = Vector3.one;
                }
            });
        }

        private void PlayLaserImpact(
            Light2D hitFlash,
            SpriteRenderer targetActor,
            Vector3 target,
            Color color,
            bool hit)
        {
            if (hit)
            {
                if (targetActor == enemySlot) PlayEnemyHit();
                else if (targetActor == playerSlot)
                    PlayHit(ref playerDamageFlashTimer, ref playerShakeTimer);
            }

            if (hitFlash == null) return;

            StopEffectLight(hitFlash);
            hitFlash.transform.position = target;
            hitFlash.transform.localScale = Vector3.one * 0.35f;
            hitFlash.color = color;
            hitFlash.intensity = laserGlowIntensity * 1.25f;
            hitFlash.enabled = true;

            Sequence spark = DOTween.Sequence().SetTarget(hitFlash);
            spark.Append(hitFlash.transform.DOScale(1.35f, laserFadeDuration)
                .SetEase(Ease.OutCubic));
            spark.Join(DOTween.To(
                () => hitFlash.intensity,
                value => hitFlash.intensity = value,
                0f,
                laserFadeDuration));
            spark.OnComplete(() => hitFlash.enabled = false);
        }

        private static void SetLaserColor(LineRenderer laser, Color color, float alpha)
        {
            color.a = alpha;
            laser.startColor = color;
            laser.endColor = color;
        }

        private static void SetLaserPositions(
            LineRenderer laser,
            LineRenderer outerLaser,
            Vector3 origin,
            Vector3 target)
        {
            laser.SetPosition(0, origin);
            laser.SetPosition(1, target);
            if (outerLaser == null) return;
            outerLaser.SetPosition(0, origin);
            outerLaser.SetPosition(1, target);
        }

        private static void InitializeEffectLight(Light2D light)
        {
            if (light == null) return;
            light.intensity = 0f;
            light.enabled = false;
        }

        private static void StopEffectLight(Light2D light)
        {
            if (light == null) return;
            light.DOKill();
            light.transform.DOKill();
            light.intensity = 0f;
            light.enabled = false;
        }

        private void StopLaser(LineRenderer laser, LineRenderer outerLaser = null)
        {
            if (laser == null) return;
            laser.DOKill();
            laser.enabled = false;
            laser.widthMultiplier = laserWidth;
            if (outerLaser == null) return;
            outerLaser.DOKill();
            outerLaser.enabled = false;
            outerLaser.widthMultiplier = laserWidth * laserOuterWidthMultiplier;
        }

        private void StopChargeEffect()
        {
            chargeAura?.StopImmediate();
            enemyChargeAura?.StopImmediate();
        }

        private void InitializeChargeEffects()
        {
            if (chargeAura != null)
            {
                chargeAura.Initialize(playerSlot, playerLaserColor);
                chargeAura.SetReady(player != null && player.IsCharged);
            }

            if (enemyChargeAura == null || enemyChargeAura == chargeAura)
            {
                if (chargeAura != null)
                    enemyChargeAura = Instantiate(
                        chargeAura,
                        chargeAura.transform.parent);
                else
                {
                    GameObject auraObject = new GameObject("EnemyChargeAura");
                    auraObject.transform.SetParent(transform, false);
                    enemyChargeAura = auraObject.AddComponent<ChargeAuraEffect>();
                }
                enemyChargeAura.name = "EnemyChargeAura";
            }

            enemyChargeAura.Initialize(enemySlot, enemyLaserColor);
            enemyChargeAura.SetReady(round != null && round.IsEnemyCharged);
        }

        private void InitializeGuardShields()
        {
            playerGuardShield = CreateGuardShield(
                playerGuardShield,
                "PlayerGuardShield");
            enemyGuardShield = CreateGuardShield(
                enemyGuardShield,
                "EnemyGuardShield");
        }

        private SpriteRenderer CreateGuardShield(
            SpriteRenderer current,
            string objectName)
        {
            if (current != null || guardShieldSprite == null) return current;

            GameObject shieldObject = new GameObject(objectName);
            SpriteRenderer shield = shieldObject.AddComponent<SpriteRenderer>();
            shield.sprite = guardShieldSprite;
            shield.color = new Color(
                guardShieldColor.r,
                guardShieldColor.g,
                guardShieldColor.b,
                0f);
            shield.enabled = false;
            return shield;
        }

        private void PlayGuardShield(
            SpriteRenderer shield,
            SpriteRenderer guardedActor,
            SpriteRenderer opposingActor)
        {
            if (shield == null || guardedActor == null || shield.sprite == null) return;

            Transform shieldTransform = shield.transform;
            shield.DOKill();
            shieldTransform.DOKill();

            Vector3 guardedCenter = guardedActor.bounds.center;
            float direction = opposingActor != null
                ? Mathf.Sign(opposingActor.bounds.center.x - guardedCenter.x)
                : guardedActor == playerSlot ? 1f : -1f;
            if (Mathf.Approximately(direction, 0f)) direction = 1f;
            float offset = guardedActor.bounds.extents.x * 0.72f;
            shieldTransform.position = guardedCenter + Vector3.right * direction * offset;
            float spriteHeight = Mathf.Max(0.01f, shield.sprite.bounds.size.y);
            float scale = guardShieldWorldHeight / spriteHeight;
            shieldTransform.localScale = Vector3.one * scale * 0.72f;
            shield.flipX = direction < 0f;
            shield.sortingLayerID = guardedActor.sortingLayerID;
            shield.sortingOrder = guardedActor.sortingOrder + 2;
            shield.color = new Color(
                guardShieldColor.r,
                guardShieldColor.g,
                guardShieldColor.b,
                0f);
            shield.enabled = true;

            float alpha = 0f;
            Sequence sequence = DOTween.Sequence().SetTarget(shield);
            sequence.Append(DOTween.To(
                () => alpha,
                value =>
                {
                    alpha = value;
                    Color color = guardShieldColor;
                    color.a *= value;
                    shield.color = color;
                },
                1f,
                guardShieldDuration * 0.3f));
            sequence.Join(shieldTransform.DOScale(
                    Vector3.one * scale,
                    guardShieldDuration * 0.45f)
                .SetEase(Ease.OutBack));
            sequence.AppendInterval(guardShieldDuration * 0.2f);
            sequence.Append(DOTween.To(
                () => alpha,
                value =>
                {
                    alpha = value;
                    Color color = guardShieldColor;
                    color.a *= value;
                    shield.color = color;
                },
                0f,
                guardShieldDuration * 0.35f));
            sequence.OnComplete(() => shield.enabled = false);
        }

        private static void StopGuardShield(SpriteRenderer shield)
        {
            if (shield == null) return;
            shield.DOKill();
            shield.transform.DOKill();
            shield.enabled = false;
        }

        private void InitializeDamageVignette()
        {
            if (damageVignetteVolume == null)
            {
                damageVignetteVolume = gameObject.AddComponent<Volume>();
                damageVignetteVolume.isGlobal = true;
                damageVignetteVolume.priority = 100f;
                runtimeVignetteProfile = ScriptableObject.CreateInstance<VolumeProfile>();
                damageVignetteVolume.profile = runtimeVignetteProfile;
            }

            VolumeProfile profile = damageVignetteVolume.profile;
            if (profile == null)
            {
                runtimeVignetteProfile = ScriptableObject.CreateInstance<VolumeProfile>();
                damageVignetteVolume.profile = runtimeVignetteProfile;
                profile = runtimeVignetteProfile;
            }
            if (!profile.TryGet(out damageVignette))
                damageVignette = profile.Add<Vignette>(true);

            damageVignette.active = true;
            damageVignette.color.Override(damageVignetteColor);
            damageVignette.smoothness.Override(0.45f);
            damageVignette.intensity.Override(0f);
        }

        private void PlayDamageVignette(int damage)
        {
            if (damageVignette == null) InitializeDamageVignette();
            if (damageVignette == null) return;

            DOTween.Kill(damageVignette);
            float current = damageVignette.intensity.value;
            float target = Mathf.Clamp(
                current > 0.01f
                    ? current + damageVignetteStackIntensity * Mathf.Max(1, damage)
                    : damageVignetteBaseIntensity
                        + damageVignetteStackIntensity * Mathf.Max(0, damage - 1),
                0f,
                damageVignetteMaxIntensity);

            Sequence sequence = DOTween.Sequence().SetTarget(damageVignette);
            sequence.Append(DOTween.To(
                () => damageVignette.intensity.value,
                value => damageVignette.intensity.Override(value),
                target,
                damageVignetteFlashDuration).SetEase(Ease.OutQuad));
            if (damageVignetteHoldDuration > 0f)
                sequence.AppendInterval(damageVignetteHoldDuration);
            sequence.Append(DOTween.To(
                () => damageVignette.intensity.value,
                value => damageVignette.intensity.Override(value),
                0f,
                damageVignetteRestoreDuration).SetEase(Ease.OutSine));
        }

        private void StopDamageVignette()
        {
            if (damageVignette == null) return;
            DOTween.Kill(damageVignette);
            damageVignette.intensity.Override(0f);
        }

        private IEnumerator BeginEnemyDeathAfterLaser()
        {
            float laserLead = laserPrepareDuration + laserGrowDuration + laserHitStopDuration;
            if (laserLead > 0f) yield return new WaitForSecondsRealtime(laserLead);
            if (!deathPresentationActive)
                yield return PlayDeathSequence(enemySlot, true);
            enemyDeathRoutine = null;
        }

        private IEnumerator BeginPlayerDeath()
        {
            yield return PlayDeathSequence(playerSlot, false);
            playerDeathRoutine = null;
        }

        private IEnumerator PlayDeathSequence(SpriteRenderer actor, bool restoreActor)
        {
            if (actor == null || deathPresentationActive) yield break;
            deathPresentationActive = true;

            // 연출 도중 스테이지가 전환되면 액터(CharacterView 인스턴스)가 Destroy될 수 있다.
            // 그 경우 파괴된 Transform 접근으로 코루틴이 예외로 죽어 전투 UI가 숨겨진 채 남고
            // 카메라 줌도 풀리지 않으므로, 매 단계에서 생존을 확인하고 정리는 finally로 보장한다.
            try
            {
            HideCombatUi();
            if (actor == playerSlot) StopPlayerIdleBounce();
            else StopEnemyIdleBounce();

            Transform actorTransform = actor.transform;
            actorTransform.DOKill();
            Vector3 originalPosition = actorTransform.position;
            Quaternion originalRotation = actorTransform.rotation;
            Vector3 originalScale = actorTransform.localScale;
            bool originalEnabled = actor.enabled;

            if (deathHitStopDuration > 0f)
                yield return new WaitForSecondsRealtime(deathHitStopDuration);
            if (actor == null) yield break;

            CharacterView actorView = CharacterViewFor(actor);
            Vector3 focusPosition = actorView != null
                ? actorView.GetAnchorPosition(CharacterAnchorType.Hit)
                : actor.bounds.center;
            cameraSway?.FocusOn(focusPosition, deathCameraZoomRatio, deathCameraDuration);
            yield return new WaitForSecondsRealtime(deathCameraDuration);
            if (actor == null) yield break;

            actorTransform.DOShakePosition(
                    deathShakeDuration,
                    deathShakeStrength,
                    24,
                    90f,
                    false,
                    true,
                    ShakeRandomnessMode.Harmonic)
                .SetUpdate(true);
            yield return new WaitForSecondsRealtime(deathShakeDuration);
            if (actor == null) yield break;

            Vector3 explosionCenter = actorView != null
                ? actorView.GetAnchorPosition(CharacterAnchorType.Effect)
                : actor.bounds.center;
            for (int i = 0; i < 3; i++)
            {
                SpawnExplosion(explosionCenter, i);
                if (i < 2 && explosionInterval > 0f)
                    yield return new WaitForSecondsRealtime(explosionInterval);
            }

            if (explosionFlyDelay > 0f)
                yield return new WaitForSecondsRealtime(explosionFlyDelay);
            if (actor == null) yield break;

            float direction = actor == playerSlot ? -1f : 1f;
            Vector3 flyTarget = actorTransform.position
                + new Vector3(direction * deathFlyDistance, deathFlyHeight, 0f);
            actorTransform.DOMove(flyTarget, deathFlyDuration)
                .SetEase(Ease.InCubic)
                .SetUpdate(true);
            actorTransform.DORotate(
                    new Vector3(0f, 0f, direction * deathSpinDegrees),
                    deathFlyDuration,
                    RotateMode.FastBeyond360)
                .SetEase(Ease.InQuad)
                .SetUpdate(true);
            yield return new WaitForSecondsRealtime(deathFlyDuration);
            if (actor == null) yield break;

            if (restoreActor)
            {
                actorTransform.DOKill();
                actorTransform.position = originalPosition;
                actorTransform.rotation = originalRotation;
                actorTransform.localScale = originalScale;
                actor.enabled = originalEnabled;
                SetEnemyIdle();
            }
            }
            finally
            {
                // 중간에 액터가 파괴돼 조기 종료되더라도 전투 UI·카메라·플래그는 반드시 되돌린다.
                // (특히 카메라 줌을 여기서 풀지 않으면 다음 스테이지까지 확대된 채 고착된다.)
                deathPresentationActive = false;
                FadeCombatUi(1f);
                cameraSway?.RestoreFocus(deathCameraDuration);
            }
        }

        private CharacterView CharacterViewFor(SpriteRenderer actor)
        {
            if (stageManager == null || actor == null) return null;
            if (actor == playerSlot) return stageManager.PlayerCharacter;
            if (actor == enemySlot) return stageManager.EnemyCharacter;
            return null;
        }

        private void FadeCombatUi(float alpha)
        {
            if (combatUiGroup == null) return;
            combatUiGroup.DOKill();
            combatUiGroup.DOFade(alpha, deathUiFadeDuration)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }

        private void HideCombatUi()
        {
            if (combatUiGroup == null) return;
            combatUiGroup.DOKill();
            combatUiGroup.alpha = 0f;
        }

        private void SpawnExplosion(Vector3 center, int index)
        {
            if (explosionPrefab == null) return;

            Vector2 offset = explosionOffsets != null && explosionOffsets.Length > 0
                ? explosionOffsets[Mathf.Clamp(index, 0, explosionOffsets.Length - 1)]
                : Vector2.zero;
            float scale = explosionScales != null && explosionScales.Length > 0
                ? explosionScales[Mathf.Clamp(index, 0, explosionScales.Length - 1)]
                : 1f;
            GameObject explosion = Instantiate(
                explosionPrefab,
                center + (Vector3)offset,
                explosionPrefab.transform.rotation);
            explosion.transform.localScale =
                explosionPrefab.transform.localScale
                * scale;
            StartCoroutine(DestroyAfterRealtime(explosion, explosionCleanupDelay));
        }

        private static IEnumerator DestroyAfterRealtime(GameObject target, float delay)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            if (target != null) Destroy(target);
        }

        private void UpdateActionMotion()
        {
            Transform target = playerSlot.transform;
            target.localPosition -= playerActionOffset;
            playerActionOffset = Vector3.zero;

            if (actionEffectTimer <= 0f || actionEffectDuration <= 0f) return;

            actionEffectTimer = Mathf.Max(0f, actionEffectTimer - Time.deltaTime);

            float normalized = 1f - actionEffectTimer / actionEffectDuration;
            float distance = Mathf.Sin(normalized * Mathf.PI) * attackLungeDistance;
            playerActionOffset = new Vector3(distance, 0f, 0f);
            target.localPosition += playerActionOffset;
        }

        private void OnScoreAwarded(int points, bool isClearBonus)
        {
            if (round == null) return;
            if (scoreNumberVisual != null && enemySlot != null)
            {
                ShowFloatingSpriteScore(points, isClearBonus);
                return;
            }
            if (scoreLabel == null || enemySlot == null)
            {
                round.CommitScore(points);
                return;
            }

            Text floating = Instantiate(scoreLabel, scoreLabel.transform.parent);
            floating.name = "FloatingScore";
            floating.enabled = true;
            floating.raycastTarget = false;
            floating.text = $"+{points:N0}";
            floating.color = Color.white;

            RectTransform floatingRect = floating.rectTransform;
            RectTransform parent = floatingRect.parent as RectTransform;
            Canvas canvas = scoreLabel.canvas;
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            Vector2 randomOffset = RandomFloatingScoreOffset();
            Vector3 worldOrigin = enemySlot.bounds.max
                + new Vector3(
                    floatingScoreBaseOffset.x + randomOffset.x,
                    floatingScoreBaseOffset.y + randomOffset.y,
                    0f);
            Vector2 screenOrigin = RectTransformUtility.WorldToScreenPoint(Camera.main, worldOrigin);
            if (parent != null
                && RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenOrigin, uiCamera, out Vector2 localOrigin))
                floatingRect.anchoredPosition = localOrigin;

            float valueRatio = Mathf.InverseLerp(0f, floatingScoreMaxValue, points);
            float scale = Mathf.Lerp(floatingScoreMinScale, floatingScoreMaxScale, valueRatio);
            floatingRect.localScale = Vector3.one * scale;
            floatingScoreOrder = (floatingScoreOrder + 1) % QueueSlotCount;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(floatingRect.DOPunchScale(Vector3.one * 0.3f, 0.16f, 6, 0.5f));
            sequence.Append(floatingRect.DOMove(scoreLabel.rectTransform.position, floatingScoreDuration)
                .SetEase(Ease.InCubic));
            sequence.Join(floating.DOFade(0.25f, floatingScoreDuration)
                .SetEase(Ease.InQuad));
            sequence.OnComplete(() =>
            {
                round.CommitScore(points);
                scoreLabel.rectTransform.DOKill();
                scoreLabel.rectTransform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 6, 0.5f);
                Destroy(floating.gameObject);
            });
        }

        private void OnScoreChanged(int score)
        {
            if (scoreNumberVisual != null)
                SetSpriteNumber(scoreNumberVisual, score, Color.white);
            else if (scoreLabel != null)
                scoreLabel.text = $"SCORE  {score:N0}";
        }

        private void InitializeScoreDisplay(int score)
        {
            if (!HasScoreDigitSprites() || scoreLabel == null)
            {
                if (scoreLabel != null)
                {
                    scoreLabel.enabled = true;
                    scoreLabel.text = $"SCORE  {score:N0}";
                }
                return;
            }

            RectTransform labelRect = scoreLabel.rectTransform;
            scoreNumberVisual = CreateSpriteNumberVisual(
                "ScoreDigits",
                labelRect.parent,
                labelRect.anchoredPosition,
                labelRect.anchorMin,
                labelRect.anchorMax,
                labelRect.pivot);
            scoreLabel.enabled = false;
            SetSpriteNumber(scoreNumberVisual, score, Color.white);
        }

        private void ShowFloatingSpriteScore(int points, bool isClearBonus)
        {
            RectTransform parent = scoreNumberVisual.Root.parent as RectTransform;
            if (parent == null || enemySlot == null)
            {
                round.CommitScore(points);
                return;
            }

            Canvas canvas = parent.GetComponentInParent<Canvas>();
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            Camera worldCamera = Camera.main;
            Vector2 localOrigin = scoreNumberVisual.Root.anchoredPosition;
            if (worldCamera != null)
            {
                Vector2 randomOffset = RandomFloatingScoreOffset();
                Vector3 worldOrigin = enemySlot.bounds.max
                    + new Vector3(
                        floatingScoreBaseOffset.x + randomOffset.x,
                        floatingScoreBaseOffset.y + randomOffset.y,
                        0f);
                Vector2 screenOrigin =
                    RectTransformUtility.WorldToScreenPoint(worldCamera, worldOrigin);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent,
                    screenOrigin,
                    uiCamera,
                    out localOrigin);
            }

            SpriteNumberVisual floating = CreateSpriteNumberVisual(
                "FloatingScore",
                parent,
                localOrigin,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f));
            SetSpriteNumber(
                floating,
                points,
                Color.white);

            float valueRatio = Mathf.InverseLerp(0f, floatingScoreMaxValue, points);
            float scale = Mathf.Lerp(
                floatingScoreMinScale,
                floatingScoreMaxScale,
                valueRatio);
            floating.Root.localScale = Vector3.one * scale;
            floatingScoreOrder = (floatingScoreOrder + 1) % QueueSlotCount;

            Sequence sequence = DOTween.Sequence().SetTarget(floating.Root);
            sequence.Append(floating.Root.DOPunchScale(
                Vector3.one * 0.3f,
                0.16f,
                6,
                0.5f));
            sequence.Append(floating.Root.DOMove(
                    scoreNumberVisual.Root.position,
                    floatingScoreDuration)
                .SetEase(Ease.InCubic));
            sequence.Join(floating.Group.DOFade(0.25f, floatingScoreDuration)
                .SetEase(Ease.InQuad));
            sequence.OnComplete(() =>
            {
                round.CommitScore(points);
                scoreNumberVisual.Root.DOKill();
                scoreNumberVisual.Root.DOPunchScale(
                    Vector3.one * 0.2f,
                    0.2f,
                    6,
                    0.5f);
                Destroy(floating.Root.gameObject);
            });
        }

        private Vector2 RandomFloatingScoreOffset()
        {
            float side = floatingScoreOrder % 2 == 0 ? -1f : 1f;
            float xMagnitude = floatingScoreRandomOffset.x > 0f
                ? Random.Range(floatingScoreRandomOffset.x * 0.35f, floatingScoreRandomOffset.x)
                : 0f;
            return new Vector2(
                side * xMagnitude,
                Random.Range(-floatingScoreRandomOffset.y, floatingScoreRandomOffset.y));
        }

        private SpriteNumberVisual CreateSpriteNumberVisual(
            string objectName,
            Transform parent,
            Vector2 anchoredPosition,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot)
        {
            GameObject rootObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasGroup));
            RectTransform root = (RectTransform)rootObject.transform;
            root.SetParent(parent, false);
            root.anchorMin = anchorMin;
            root.anchorMax = anchorMax;
            root.pivot = pivot;
            root.anchoredPosition = anchoredPosition;
            root.sizeDelta = scoreDigitSize;
            return new SpriteNumberVisual
            {
                Root = root,
                Group = rootObject.GetComponent<CanvasGroup>(),
            };
        }

        private void SetSpriteNumber(
            SpriteNumberVisual visual,
            int value,
            Color color)
        {
            if (visual == null || visual.Root == null) return;
            string digits = Mathf.Max(0, value).ToString();
            EnsureDigitImages(visual, digits.Length);

            float step = scoreDigitSize.x + scoreDigitSpacing;
            float width = scoreDigitSize.x + step * (digits.Length - 1);
            visual.Root.sizeDelta = new Vector2(width, scoreDigitSize.y);
            float firstX = -width * 0.5f + scoreDigitSize.x * 0.5f;
            float y = visual.Root.pivot.y >= 0.99f
                ? -scoreDigitSize.y * 0.5f
                : 0f;

            for (int i = 0; i < visual.Digits.Count; i++)
            {
                Image image = visual.Digits[i];
                bool active = i < digits.Length;
                image.gameObject.SetActive(active);
                if (!active) continue;
                int digit = digits[i] - '0';
                image.sprite = scoreDigitSprites[digit];
                image.color = color;
                image.rectTransform.anchoredPosition =
                    new Vector2(firstX + step * i, y);
            }
        }

        private void EnsureDigitImages(SpriteNumberVisual visual, int count)
        {
            while (visual.Digits.Count < count)
            {
                GameObject digitObject = new GameObject(
                    $"Digit{visual.Digits.Count}",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                RectTransform rect = (RectTransform)digitObject.transform;
                rect.SetParent(visual.Root, false);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = scoreDigitSize;
                Image image = digitObject.GetComponent<Image>();
                image.preserveAspect = true;
                image.raycastTarget = false;
                visual.Digits.Add(image);
            }
        }

        private bool HasScoreDigitSprites()
        {
            if (scoreDigitSprites == null || scoreDigitSprites.Length < 10)
                return false;
            for (int i = 0; i < 10; i++)
                if (scoreDigitSprites[i] == null) return false;
            return true;
        }

        private void UpdateShake(SpriteRenderer slot, ref float timer, ref Vector3 offset)
        {
            Transform target = slot.transform;
            target.localPosition -= offset;
            offset = Vector3.zero;

            if (timer <= 0f || hitShakeDuration <= 0f) return;

            timer = Mathf.Max(0f, timer - Time.deltaTime);
            float strength = hitShakeDistance * (timer / hitShakeDuration);
            Vector2 randomOffset = Random.insideUnitCircle * strength;
            offset = new Vector3(randomOffset.x, randomOffset.y, 0f);
            target.localPosition += offset;
        }

        private static void RestoreOffset(SpriteRenderer slot, ref Vector3 offset)
        {
            if (slot == null) return;
            slot.transform.localPosition -= offset;
            offset = Vector3.zero;
        }

        private static void RestorePresentation(SpriteRenderer slot, Vector3 baseScale, ref Vector3 shakeOffset)
        {
            if (slot == null) return;
            slot.transform.localPosition -= shakeOffset;
            slot.transform.localScale = baseScale;
            shakeOffset = Vector3.zero;
        }

        private void SetDots(int active)
        {
            if (beatDots == null) return;
            for (int i = 0; i < beatDots.Length; i++)
            {
                if (beatDots[i] == null) continue;
                Color onColor = i < Conductor.ResponseStartBeat ? presentDotColor : responseDotColor;
                beatDots[i].color = i == active ? onColor : dotOffColor;
            }
        }

    }
}
