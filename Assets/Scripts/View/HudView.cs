using System.Collections;
using DG.Tweening;
using UnityEngine;
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
        [SerializeField] private CameraSway cameraSway;

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
        [Tooltip("적 HP 최대치(칸 수와 맞춤). ※구동은 임시: 처리(Clear)마다 1 감소")]
        [SerializeField, Min(1)] private int enemyMaxHp = 7;
        [SerializeField] private Text phaseLabel;
        [SerializeField] private Text feedbackLabel;
        [SerializeField] private Text gameOverLabel;
        [SerializeField] private Text countdownLabel;
        [SerializeField] private Text chargeLabel;
        [SerializeField] private Text scoreLabel;

        [Header("통합 Queue (기존 Enemy Queue 슬롯 재사용)")]
        [SerializeField] private Image[] enemyQueueSlots = new Image[QueueSlotCount];
        [Tooltip("기존 Player Queue. 통합 후 비활성화만 하며 Scene 오브젝트는 보존")]
        [SerializeField] private Image[] playerQueueSlots = new Image[QueueSlotCount];
        [SerializeField] private Sprite emptyQueueSprite;
        [SerializeField] private Color queueEmptyColor = new Color(0.12f, 0.15f, 0.20f, 0.45f);
        [SerializeField] private Color queueDamageColor = new Color(0.95f, 0.18f, 0.18f);
        [SerializeField] private Color queueSuccessColor = new Color(0.25f, 0.95f, 0.38f);

        [Header("인디케이터·하트 색")]
        [SerializeField] private Color presentDotColor = new Color(0.95f, 0.80f, 0.32f);
        [SerializeField] private Color responseDotColor = new Color(0.32f, 0.90f, 0.90f);
        [SerializeField] private Color dotOffColor = new Color(0.24f, 0.24f, 0.30f);
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
        [Tooltip("EnemyActor 자식 발사 위치의 LineRenderer")]
        [SerializeField] private LineRenderer enemyLaser;
        [SerializeField] private Light2D playerLaserMuzzleGlow;
        [SerializeField] private Light2D enemyLaserMuzzleGlow;
        [SerializeField] private Light2D playerLaserHitFlash;
        [SerializeField] private Light2D enemyLaserHitFlash;
        [SerializeField, Min(0.01f)] private float laserWidth = 0.1f;
        [SerializeField, Min(1f)] private float laserFlashWidthMultiplier = 1.8f;
        [SerializeField, Range(0.08f, 0.12f)] private float laserPrepareDuration = 0.1f;
        [SerializeField, Min(0f)] private float laserGrowDuration = 0.08f;
        [SerializeField, Min(0f)] private float laserFadeDuration = 0.1f;
        [SerializeField, Min(0f)] private float laserGlowIntensity = 1.4f;
        [SerializeField, Min(0f)] private float laserHitStopDuration = 0.05f;
        [SerializeField] private Color playerLaserColor = new Color(0.2f, 0.95f, 1f, 1f);
        [SerializeField] private Color enemyLaserColor = new Color(1f, 0.18f, 0.18f, 1f);

        [Header("Charge Effect")]
        [SerializeField] private Light2D chargeGlow;
        [SerializeField] private LineRenderer chargeRing;
        private ParticleSystem chargeAura;
        [SerializeField, Min(0f)] private float chargeFlashIntensity = 1.6f;
        [SerializeField, Min(0f)] private float chargeSustainIntensity = 0.3f;
        [SerializeField, Min(0.1f)] private float chargePulsePeriod = 0.5f;
        [SerializeField, Min(0f)] private float chargeRingDuration = 0.28f;
        [SerializeField, Min(0.1f)] private float chargeRingRadius = 1.5f;

        [Header("Score HUD")]
        [SerializeField, Min(0.1f)] private float floatingScoreDuration = 0.65f;
        [SerializeField, Min(1f)] private float floatingScoreMinScale = 1f;
        [SerializeField, Min(1f)] private float floatingScoreMaxScale = 1.8f;
        [SerializeField, Min(1)] private int floatingScoreMaxValue = 500;
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
        private int floatingScoreOrder;
        private Coroutine hitStopRoutine;
        private float timeScaleBeforeHitStop = 1f;
        private double hitStopStartedAt;

        private void OnEnable()
        {
            if (round != null)
            {
                round.OnEnemyRevealed += OnReveal;
                round.OnJudged += OnJudged;
                round.OnPhaseChanged += OnPhase;
                round.OnCycleStarted += OnCycleStarted;
                round.OnScoreAwarded += OnScoreAwarded;
                round.OnScoreChanged += OnScoreChanged;
                round.OnGameOver += OnGameOver;
                round.OnEnemyBarChanged += OnEnemyBar;
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
                round.OnEnemyRevealed -= OnReveal;
                round.OnJudged -= OnJudged;
                round.OnPhaseChanged -= OnPhase;
                round.OnCycleStarted -= OnCycleStarted;
                round.OnScoreAwarded -= OnScoreAwarded;
                round.OnScoreChanged -= OnScoreChanged;
                round.OnGameOver -= OnGameOver;
                round.OnEnemyBarChanged -= OnEnemyBar;
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
            StopLaser(playerLaser);
            StopLaser(enemyLaser);
            StopEffectLight(playerLaserMuzzleGlow);
            StopEffectLight(enemyLaserMuzzleGlow);
            StopEffectLight(playerLaserHitFlash);
            StopEffectLight(enemyLaserHitFlash);
            StopChargeEffect();
            RestoreHitStop();
        }

        private void Start()
        {
            if (enemySlot != null) enemyBaseScale = enemySlot.transform.localScale;
            if (playerSlot != null) playerBaseScale = playerSlot.transform.localScale;
            presentationInitialized = true;
            if (gameOverLabel != null) gameOverLabel.enabled = false;
            if (countdownLabel != null) countdownLabel.enabled = false;
            if (chargeLabel != null) chargeLabel.enabled = false;
            if (phaseLabel != null) phaseLabel.text = "";
            if (feedbackLabel != null) feedbackLabel.text = "";
            SetEnemyIdle();
            SetPlayerIdle();
            if (scoreLabel != null)
            {
                scoreLabel.enabled = true;
                scoreLabel.text = $"SCORE  {(round != null ? round.Score : 0):N0}";
            }
            if (player != null) OnHealth(player.CurrentHp, player.MaxHp);
            _enemyHp = round != null ? round.EnemyBarCurrent : enemyMaxHp;
            RefreshEnemyCells();
            SetDots(-1);
            InitializeQueues();
            InitializeLaser(playerLaser, playerLaserMuzzleGlow, playerLaserHitFlash);
            InitializeLaser(enemyLaser, enemyLaserMuzzleGlow, enemyLaserHitFlash);
            InitializeChargeEffect();
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
            if (player != null && player.IsCharged) PositionChargeEffect();
        }

        private void OnReveal(int slot, Enemy e)
        {
            if (slot >= 0 && slot < revealedEnemies.Length) revealedEnemies[slot] = e;
            SetQueueSlot(enemyQueueSlots, slot, QueueSprite(e), Color.white);
            if (feedbackLabel != null) feedbackLabel.text = e != null ? e.DisplayName : "";
        }

        private void OnJudged(int slot, Enemy e, JudgeResult r)
        {
            if (enemyLaser != null && e != null)
                enemyLaser.transform.localPosition = e.LaserOriginOffset;

            if (e != null && e.Sprite != null)
            {
                SetEnemySprite(e.Sprite, true);
                enemySpriteTimer = actionSpriteHold;
            }

            if (r.Input == PlayerAction.Attack && enemySlot != null)
                PlayLaser(
                    playerLaser,
                    playerLaserMuzzleGlow,
                    playerLaserHitFlash,
                    enemySlot,
                    playerLaserColor,
                    r.Cleared);
            if (e != null && e.Action == PlayerAction.Attack && playerSlot != null)
                PlayLaser(
                    enemyLaser,
                    enemyLaserMuzzleGlow,
                    enemyLaserHitFlash,
                    playerSlot,
                    enemyLaserColor,
                    r.PlayerDamage > 0);

            if (r.Cleared && r.Input == PlayerAction.Attack)
                PlayAttackMotion();
            ResolveQueueSlot(slot, r);
            if (feedbackLabel != null)
                feedbackLabel.text = string.IsNullOrEmpty(r.Feedback) ? $"{r.Input} → {r.Type}" : r.Feedback;
            // 적 HP는 이제 RoundManager가 소유 → OnEnemyBarChanged로 갱신된다.
        }

        private void OnCycleStarted(int cycle)
        {
            ResetQueues();
            SetEnemyIdle();
        }

        private void OnEnemyBar(int current, int max)
        {
            _enemyHp = current;
            RefreshEnemyCells();
        }

        private void RefreshEnemyCells()
        {
            if (enemyCells == null) return;
            for (int i = 0; i < enemyCells.Length; i++)
                if (enemyCells[i] != null) enemyCells[i].enabled = i < _enemyHp;
        }

        private void OnPhase(int cycle, PhaseSO p)
        {
            if (phaseLabel != null) phaseLabel.text = p != null ? p.PhaseName : "(균등)";
        }

        private void OnGameOver()
        {
            if (gameOverLabel != null) { gameOverLabel.enabled = true; gameOverLabel.text = "GAME OVER"; }
        }

        private void OnBeat(int beatInCycle)
        {
            SetDots(beatInCycle);
        }

        private void OnHealth(int current, int max)
        {
            if (hpFill != null) hpFill.fillAmount = max > 0 ? (float)current / max : 0f;
            // HP 칸(세그먼트): 현재 HP만큼만 켠다(나머지는 꺼서 빈 칸이 보이게)
            if (hearts != null)
                for (int i = 0; i < hearts.Length; i++)
                    if (hearts[i] != null) hearts[i].enabled = i < current;
        }

        private void OnPlayerActionPresented(PlayerAction action, Sprite sprite)
        {
            if (playerIdleAnim != null) playerIdleAnim.Pause(); // 행동 동안 idle 스텝 정지
            SetPlayerSprite(sprite != null ? sprite : playerSlot != null ? playerSlot.sprite : null, true);
            playerSpriteTimer = actionSpriteHold;
        }

        private void OnCharged(bool charged)
        {
            if (presentationInitialized)
            {
                if (charged) PlayChargeStart();
                else PlayChargeRelease();
            }

            if (chargeLabel != null)
            {
                chargeLabel.enabled = charged;
                if (charged) chargeLabel.text = "⚡ 충전";
            }
        }

        private void SetEnemyIdle()
        {
            SetEnemySprite(enemyIdleSprite); // null이면 현재 placeholder 유지
        }

        private void SetPlayerIdle()
        {
            // 키프레임 idle이 있으면 그쪽에 맡긴다(림버스식 스텝). 없으면 단일 idle 스프라이트 폴백.
            if (playerIdleAnim != null && playerIdleAnim.HasFrames) { playerIdleAnim.Resume(); return; }
            SetPlayerSprite(player != null ? player.IdleSprite : null);
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

        private void SetQueueSlot(Image[] slots, int index, Sprite sprite, Color color)
        {
            Image slot = QueueSlot(slots, index);
            if (slot == null) return;
            slot.sprite = sprite != null ? sprite : emptyQueueSprite;
            slot.color = color;
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
                queueSlot.color = resultColor;
                queueSlot.rectTransform.DOKill();
                queueSlot.rectTransform.DOPunchScale(Vector3.one * 0.16f, 0.18f, 6, 0.5f);
            }
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

        private void InitializeLaser(LineRenderer laser, Light2D muzzleGlow, Light2D hitFlash)
        {
            if (laser == null) return;
            laser.useWorldSpace = true;
            laser.positionCount = 2;
            laser.widthMultiplier = laserWidth;
            laser.enabled = false;
            InitializeEffectLight(muzzleGlow);
            InitializeEffectLight(hitFlash);
        }

        private void PlayLaser(
            LineRenderer laser,
            Light2D muzzleGlow,
            Light2D hitFlash,
            SpriteRenderer targetActor,
            Color color,
            bool hit)
        {
            if (laser == null || targetActor == null) return;

            StopLaser(laser);
            StopEffectLight(muzzleGlow);
            Vector3 origin = laser.transform.position;
            Vector3 target = targetActor.bounds.center;
            float progress = 0f;
            float alpha = 1f;
            laser.widthMultiplier = laserWidth * laserFlashWidthMultiplier;
            SetLaserColor(laser, color, alpha);
            laser.SetPosition(0, origin);
            laser.SetPosition(1, origin);

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

            sequence.AppendCallback(() => laser.enabled = true);
            sequence.Append(DOTween.To(
                () => progress,
                value =>
                {
                    progress = value;
                    laser.SetPosition(0, laser.transform.position);
                    laser.SetPosition(1, Vector3.Lerp(origin, target, value));
                },
                1f,
                laserGrowDuration).SetEase(Ease.OutCubic));
            sequence.Join(DOTween.To(
                () => laser.widthMultiplier,
                value => laser.widthMultiplier = value,
                laserWidth,
                laserGrowDuration).SetEase(Ease.OutQuad));
            if (muzzleGlow != null)
                sequence.Join(muzzleGlow.transform.DOScale(1f, laserGrowDuration)
                    .SetEase(Ease.OutQuad));
            sequence.AppendCallback(() =>
            {
                PlayLaserImpact(hitFlash, targetActor, target, color, hit);
                if (hit) BeginHitStop();
            });
            sequence.Append(DOTween.To(
                () => alpha,
                value =>
                {
                    alpha = value;
                    SetLaserColor(laser, color, value);
                },
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

        private void StopLaser(LineRenderer laser)
        {
            if (laser == null) return;
            laser.DOKill();
            laser.enabled = false;
            laser.widthMultiplier = laserWidth;
        }

        private void InitializeChargeEffect()
        {
            InitializeEffectLight(chargeGlow);
            EnsureChargeAura();
            if (chargeAura != null)
            {
                ParticleSystem.MainModule main = chargeAura.main;
                main.startColor = playerLaserColor;
                chargeAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (chargeRing == null) return;

            chargeRing.useWorldSpace = false;
            chargeRing.loop = true;
            chargeRing.positionCount = 32;
            chargeRing.widthMultiplier = 0.05f;
            for (int i = 0; i < chargeRing.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / chargeRing.positionCount;
                chargeRing.SetPosition(
                    i,
                    new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * chargeRingRadius);
            }
            SetLaserColor(chargeRing, playerLaserColor, 0f);
            chargeRing.enabled = false;
        }

        private void EnsureChargeAura()
        {
            if (chargeAura == null && chargeGlow != null)
            {
                chargeAura = chargeGlow.GetComponent<ParticleSystem>();
                if (chargeAura == null) chargeAura = chargeGlow.gameObject.AddComponent<ParticleSystem>();
            }
            if (chargeAura == null) return;
            chargeAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = chargeAura.main;
            main.duration = 1f;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.15f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = playerLaserColor;
            main.maxParticles = 48;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            ParticleSystem.EmissionModule emission = chargeAura.emission;
            emission.enabled = true;
            emission.rateOverTime = 16f;

            ParticleSystem.ShapeModule shape = chargeAura.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1.15f;
            shape.radiusThickness = 0.35f;

            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f),
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.65f, 0.2f),
                    new GradientAlphaKey(0f, 1f),
                });
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = chargeAura.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = gradient;

            ParticleSystem.NoiseModule noise = chargeAura.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.Low;
            noise.strength = 0.12f;
            noise.frequency = 0.6f;
            noise.scrollSpeed = 0.2f;

            ParticleSystemRenderer auraRenderer = chargeAura.GetComponent<ParticleSystemRenderer>();
            if (auraRenderer != null) auraRenderer.sortingOrder = 22;
        }

        private void PlayChargeStart()
        {
            StopChargeEffect();
            PositionChargeEffect();

            if (chargeAura != null)
            {
                ParticleSystem.MainModule main = chargeAura.main;
                main.startColor = playerLaserColor;
                chargeAura.Play(true);
            }

            if (chargeGlow != null)
            {
                chargeGlow.color = playerLaserColor;
                chargeGlow.intensity = 0f;
                chargeGlow.enabled = true;

                Sequence glowIntro = DOTween.Sequence().SetTarget(chargeGlow);
                glowIntro.Append(DOTween.To(
                    () => chargeGlow.intensity,
                    value => chargeGlow.intensity = value,
                    chargeFlashIntensity,
                    0.1f).SetEase(Ease.OutQuad));
                glowIntro.Append(DOTween.To(
                    () => chargeGlow.intensity,
                    value => chargeGlow.intensity = value,
                    chargeSustainIntensity,
                    0.12f).SetEase(Ease.InQuad));
                glowIntro.OnComplete(StartChargePulse);
            }

            if (chargeRing != null)
            {
                float alpha = 1f;
                chargeRing.enabled = true;
                chargeRing.transform.localScale = Vector3.one * 0.3f;
                SetLaserColor(chargeRing, playerLaserColor, alpha);

                Sequence ring = DOTween.Sequence().SetTarget(chargeRing);
                ring.Append(chargeRing.transform.DOScale(1.25f, chargeRingDuration)
                    .SetEase(Ease.OutCubic));
                ring.Join(DOTween.To(
                    () => alpha,
                    value =>
                    {
                        alpha = value;
                        SetLaserColor(chargeRing, playerLaserColor, value);
                    },
                    0f,
                    chargeRingDuration).SetEase(Ease.InQuad));
                ring.OnComplete(() => chargeRing.enabled = false);
            }
        }

        private void StartChargePulse()
        {
            if (chargeGlow == null || player == null || !player.IsCharged) return;

            chargeGlow.DOKill();
            float low = chargeSustainIntensity * 0.7f;
            float high = chargeSustainIntensity * 1.25f;
            chargeGlow.intensity = low;
            DOTween.To(
                    () => chargeGlow.intensity,
                    value => chargeGlow.intensity = value,
                    high,
                    chargePulsePeriod * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(chargeGlow);
        }

        private void PlayChargeRelease()
        {
            PositionChargeEffect();
            if (chargeAura != null)
                chargeAura.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (chargeGlow == null) return;

            chargeGlow.DOKill();
            chargeGlow.enabled = true;
            Sequence release = DOTween.Sequence().SetTarget(chargeGlow);
            release.Append(DOTween.To(
                () => chargeGlow.intensity,
                value => chargeGlow.intensity = value,
                chargeFlashIntensity * 1.35f,
                0.05f).SetEase(Ease.OutQuad));
            release.Append(DOTween.To(
                () => chargeGlow.intensity,
                value => chargeGlow.intensity = value,
                0f,
                0.12f).SetEase(Ease.InQuad));
            release.OnComplete(() => chargeGlow.enabled = false);
            if (chargeRing != null) chargeRing.enabled = false;
        }

        private void PositionChargeEffect()
        {
            if (playerSlot == null) return;
            Vector3 center = playerSlot.bounds.center;
            if (chargeGlow != null) chargeGlow.transform.position = center;
            if (chargeRing != null) chargeRing.transform.position = center;
        }

        private void StopChargeEffect()
        {
            StopEffectLight(chargeGlow);
            if (chargeAura != null)
                chargeAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (chargeRing == null) return;
            chargeRing.DOKill();
            chargeRing.transform.DOKill();
            chargeRing.enabled = false;
            chargeRing.transform.localScale = Vector3.one;
        }

        private void BeginHitStop()
        {
            if (laserHitStopDuration <= 0f || hitStopRoutine != null || Time.timeScale <= 0f)
                return;
            hitStopRoutine = StartCoroutine(HitStop());
        }

        private IEnumerator HitStop()
        {
            timeScaleBeforeHitStop = Time.timeScale;
            hitStopStartedAt = Time.realtimeSinceStartupAsDouble;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(laserHitStopDuration);
            CompleteHitStop();
        }

        private void RestoreHitStop()
        {
            if (hitStopRoutine == null) return;

            StopCoroutine(hitStopRoutine);
            CompleteHitStop();
        }

        private void CompleteHitStop()
        {
            double pausedDuration = Time.realtimeSinceStartupAsDouble - hitStopStartedAt;
            Time.timeScale = timeScaleBeforeHitStop;
            conductor?.DelayClock(pausedDuration);
            hitStopRoutine = null;
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
            floating.color = isClearBonus ? clearBonusColor : hitScoreColor;

            RectTransform floatingRect = floating.rectTransform;
            RectTransform parent = floatingRect.parent as RectTransform;
            Canvas canvas = scoreLabel.canvas;
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            Vector3 worldOrigin = enemySlot.transform.position + Vector3.up * (0.4f + 0.12f * floatingScoreOrder);
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
            if (scoreLabel != null) scoreLabel.text = $"SCORE  {score:N0}";
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
