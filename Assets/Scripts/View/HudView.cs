using System.Collections;
using UnityEngine;
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

        [Header("행동 Queue (기존 비트 슬롯 재사용)")]
        [SerializeField] private Image[] enemyQueueSlots = new Image[QueueSlotCount];
        [SerializeField] private Image[] playerQueueSlots = new Image[QueueSlotCount];
        [SerializeField] private Sprite emptyQueueSprite;
        [SerializeField] private Sprite guardActionIcon;
        [SerializeField] private Sprite attackActionIcon;
        [SerializeField] private Sprite chargeActionIcon;
        [SerializeField] private Color queueEmptyColor = new Color(0.12f, 0.15f, 0.20f, 0.45f);
        [SerializeField] private Color queueWaitingColor = Color.white;
        [SerializeField] private Color queueDamageColor = new Color(0.95f, 0.18f, 0.18f);
        [SerializeField] private Color queueSuccessColor = new Color(0.25f, 0.95f, 0.38f);
        [SerializeField, Min(0f)] private float queueBlinkDuration = 0.2f;
        [SerializeField, Min(0f)] private float queueBlinkSpeed = 35f;
        [SerializeField, Min(0f)] private float queuePulseScale = 0.15f;

        [Header("자세 색 (인스펙터 조정)")]
        [SerializeField] private Color aggressiveColor = new Color(0.90f, 0.32f, 0.32f); // 공세(정답 가드)
        [SerializeField] private Color defenselessColor = new Color(0.32f, 0.62f, 0.95f); // 무방비(정답 공격)

        [Header("판정 색")]
        [SerializeField] private Color clearedColor = new Color(0.32f, 0.90f, 0.44f);
        [SerializeField] private Color safeColor = new Color(0.92f, 0.85f, 0.32f);
        [SerializeField] private Color punishedColor = new Color(0.95f, 0.22f, 0.22f);
        [SerializeField] private Color idleColor = new Color(0.30f, 0.30f, 0.36f);

        [Header("인디케이터·하트 색")]
        [SerializeField] private Color presentDotColor = new Color(0.95f, 0.80f, 0.32f);
        [SerializeField] private Color responseDotColor = new Color(0.32f, 0.90f, 0.90f);
        [SerializeField] private Color dotOffColor = new Color(0.24f, 0.24f, 0.30f);
        [SerializeField] private Color heartFull = new Color(0.90f, 0.26f, 0.36f);
        [SerializeField] private Color heartEmpty = new Color(0.24f, 0.20f, 0.22f);

        [Header("연출 (인스펙터 조정)")]
        [SerializeField, Min(0.05f)] private float flashFade = 0.35f;
        [Tooltip("행동/자세 스프라이트 표시 후 idle 복귀까지 시간(초)")]
        [SerializeField, Min(0f)] private float actionSpriteHold = 0.3f;
        [Tooltip("스프라이트 교체 시 종이를 넘기듯 가로로 접혔다 펴지는 시간(초)")]
        [SerializeField, Min(0f)] private float spriteFlipDuration = 0.12f;
        [Tooltip("피격 플래시 지속 시간(초)")]
        [SerializeField, Min(0f)] private float damageFlashDuration = 0.16f;
        [Tooltip("피격 플래시 점멸 속도")]
        [SerializeField, Min(0f)] private float damageFlashSpeed = 45f;
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.08f, 0.08f);
        [Tooltip("피격 흔들림 지속 시간(초)")]
        [SerializeField, Min(0f)] private float hitShakeDuration = 0.18f;
        [Tooltip("피격 흔들림 최대 거리(로컬 좌표)")]
        [SerializeField, Min(0f)] private float hitShakeDistance = 0.12f;

        [Header("행동 Effect")]
        [SerializeField, Min(0f)] private float actionEffectDuration = 0.18f;
        [SerializeField, Min(0f)] private float attackLungeDistance = 0.35f;
        [SerializeField] private Color attackEffectColor = new Color(1f, 0.72f, 0.25f);
        [SerializeField] private Color defenseEffectColor = new Color(0.25f, 0.85f, 1f);

        private int _enemyHp;
        private Color enemyTarget;
        private float enemyFlash;
        private float enemySpriteTimer;
        private float enemyDamageFlashTimer;
        private float enemyShakeTimer;
        private Vector3 enemyShakeOffset;
        private Vector3 enemyBaseScale;
        private Coroutine enemyFlipRoutine;
        private Color playerTarget;
        private float playerFlash;
        private float playerSpriteTimer;
        private float playerDamageFlashTimer;
        private float playerShakeTimer;
        private Vector3 playerShakeOffset;
        private Vector3 playerBaseScale;
        private Coroutine playerFlipRoutine;
        private bool presentationInitialized;
        private readonly Enemy[] revealedEnemies = new Enemy[QueueSlotCount];
        private readonly bool[] playerQueueResolved = new bool[QueueSlotCount];
        private readonly float[] playerQueueBlinkTimers = new float[QueueSlotCount];
        private readonly Vector3[] playerQueueBaseScales = new Vector3[QueueSlotCount];
        private PlayerAction currentActionEffect;
        private float actionEffectTimer;
        private Vector3 playerActionOffset;

        private void OnEnable()
        {
            if (round != null)
            {
                round.OnEnemyRevealed += OnReveal;
                round.OnJudged += OnJudged;
                round.OnPhaseChanged += OnPhase;
                round.OnCycleStarted += OnCycleStarted;
                round.OnAttackLanded += OnAttackLanded;
                round.OnGameOver += OnGameOver;
            }
            if (conductor != null) conductor.OnBeat += OnBeat;
            if (player != null)
            {
                player.OnHealthChanged += OnHealth;
                player.OnSpriteChanged += OnPlayerSprite;
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
                round.OnAttackLanded -= OnAttackLanded;
                round.OnGameOver -= OnGameOver;
            }
            if (conductor != null) conductor.OnBeat -= OnBeat;
            if (player != null)
            {
                player.OnHealthChanged -= OnHealth;
                player.OnSpriteChanged -= OnPlayerSprite;
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
            enemyTarget = idleColor;
            playerTarget = idleColor;
            SetEnemyIdle();
            SetPlayerIdle();
            if (player != null) OnHealth(player.CurrentHp, player.MaxHp);
            _enemyHp = enemyMaxHp;
            RefreshEnemyCells();
            SetDots(-1);
            InitializeQueues();
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

            // 적: 자세 스프라이트 유지시간 후 idle 복귀. 색은 항상 placeholder/스프라이트를 틴트.
            if (enemySlot != null)
            {
                if (enemySpriteTimer > 0f)
                {
                    enemySpriteTimer -= Time.deltaTime;
                    if (enemySpriteTimer <= 0f) SetEnemyIdle();
                }
                enemyFlash = Mathf.MoveTowards(enemyFlash, 0f, Time.deltaTime / flashFade);
                Color color = Color.Lerp(Color.white, enemyTarget, enemyFlash);
                enemySlot.color = enemyDamageFlashTimer > 0f
                    ? DamageFlash(Color.white, ref enemyDamageFlashTimer)
                    : color;
                UpdateShake(enemySlot, ref enemyShakeTimer, ref enemyShakeOffset);
            }
            if (playerSlot != null)
            {
                if (playerSpriteTimer > 0f)
                {
                    playerSpriteTimer -= Time.deltaTime;
                    if (playerSpriteTimer <= 0f) SetPlayerIdle();
                }
                playerFlash = Mathf.MoveTowards(playerFlash, 0f, Time.deltaTime / flashFade);
                Color color = Color.Lerp(Color.white, playerTarget, playerFlash);
                color = ActionEffectColor(color);
                playerSlot.color = playerDamageFlashTimer > 0f
                    ? DamageFlash(Color.white, ref playerDamageFlashTimer)
                    : color;
                UpdateShake(playerSlot, ref playerShakeTimer, ref playerShakeOffset);
                UpdateActionMotion();
            }

            UpdateQueueAnimations();
        }

        private Color PoseColor(Enemy e)
        {
            if (e == null) return idleColor;
            return e.PrimaryAnswer() == PlayerAction.Guard ? aggressiveColor : defenselessColor;
        }

        private Color ResultColor(OutcomeType t)
            => t == OutcomeType.Cleared ? clearedColor : (t == OutcomeType.Safe ? safeColor : punishedColor);

        private void OnReveal(int slot, Enemy e)
        {
            if (slot >= 0 && slot < revealedEnemies.Length) revealedEnemies[slot] = e;
            SetQueueSlot(enemyQueueSlots, slot, QueueSprite(e), Color.white);
            if (feedbackLabel != null) feedbackLabel.text = e != null ? e.DisplayName : "";
        }

        private void OnJudged(int slot, Enemy e, JudgeResult r)
        {
            Color c = ResultColor(r.Type);
            enemyTarget = c; enemyFlash = 1f;
            playerTarget = c; playerFlash = 1f;
            if (r.PlayerDamage > 0)
                PlayHit(ref playerDamageFlashTimer, ref playerShakeTimer);
            if (r.Cleared && r.Input == PlayerAction.Attack)
                PlayHit(ref enemyDamageFlashTimer, ref enemyShakeTimer);
            ResolvePlayerQueueSlot(slot, r);
            PlayActionEffect(r.Input);
            if (feedbackLabel != null)
                feedbackLabel.text = string.IsNullOrEmpty(r.Feedback) ? $"{r.Input} → {r.Type}" : r.Feedback;

            // [임시] 적을 처리(Clear)하면 적 HP 1 감소 — 실제 구동 규칙은 추후
            if (r.Cleared)
            {
                _enemyHp = Mathf.Max(0, _enemyHp - 1);
                RefreshEnemyCells();
            }
        }

        private void OnCycleStarted(int cycle)
        {
            ResetQueues();
            SetEnemyIdle();
        }

        private void OnAttackLanded(bool strongAttack)
        {
            if (cameraSway != null) cameraSway.Shake(strongAttack);
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
            if (beatInCycle < Conductor.ResponseStartBeat) return;

            int slot = beatInCycle - Conductor.ResponseStartBeat;
            if (slot < 0 || slot >= QueueSlotCount) return;

            Enemy enemy = revealedEnemies[slot];
            if (enemy != null && enemy.Sprite != null)
            {
                enemyTarget = PoseColor(enemy);
                enemyFlash = 1f;
                SetEnemySprite(enemy.Sprite);
                enemySpriteTimer = actionSpriteHold;
            }
            BeginPlayerQueueSlot(slot);
        }

        private void OnHealth(int current, int max)
        {
            if (hpFill != null) hpFill.fillAmount = max > 0 ? (float)current / max : 0f;
            // HP 칸(세그먼트): 현재 HP만큼만 켠다(나머지는 꺼서 빈 칸이 보이게)
            if (hearts != null)
                for (int i = 0; i < hearts.Length; i++)
                    if (hearts[i] != null) hearts[i].enabled = i < current;
        }

        private void OnPlayerSprite(Sprite s)
        {
            if (s == null) return;
            if (playerIdleAnim != null) playerIdleAnim.Pause(); // 행동 동안 idle 스텝 정지
            SetPlayerSprite(s); // 행동 스프라이트(원본 비율 자동)
            playerSpriteTimer = actionSpriteHold;
        }

        private void OnCharged(bool charged)
        {
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

        private void SetEnemySprite(Sprite sprite)
        {
            if (enemySlot == null || sprite == null || enemySlot.sprite == sprite) return;
            if (enemyFlipRoutine != null) StopCoroutine(enemyFlipRoutine);
            enemySlot.transform.localScale = enemyBaseScale;
            enemyFlipRoutine = StartCoroutine(FlipSprite(enemySlot, sprite, enemyBaseScale, false));
        }

        private void SetPlayerSprite(Sprite sprite)
        {
            if (playerSlot == null || sprite == null || playerSlot.sprite == sprite) return;
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
            flashTimer = damageFlashDuration;
            shakeTimer = hitShakeDuration;
        }

        private Color DamageFlash(Color baseColor, ref float timer)
        {
            if (timer <= 0f || damageFlashDuration <= 0f) return baseColor;

            timer = Mathf.Max(0f, timer - Time.deltaTime);
            float elapsed = damageFlashDuration - timer;
            float pulse = Mathf.Abs(Mathf.Cos(elapsed * damageFlashSpeed));
            float strength = pulse * (timer / damageFlashDuration);
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

                Image playerQueueSlot = QueueSlot(playerQueueSlots, i);
                if (playerQueueSlot != null)
                {
                    playerQueueSlot.enabled = true;
                    playerQueueSlot.preserveAspect = true;
                    playerQueueSlot.raycastTarget = false;
                    playerQueueBaseScales[i] = playerQueueSlot.rectTransform.localScale;
                }
            }
            ResetQueues();
        }

        private void ResetQueues()
        {
            for (int i = 0; i < QueueSlotCount; i++)
            {
                revealedEnemies[i] = null;
                playerQueueResolved[i] = false;
                playerQueueBlinkTimers[i] = 0f;
                SetQueueSlot(enemyQueueSlots, i, emptyQueueSprite, queueEmptyColor);
                SetQueueSlot(playerQueueSlots, i, emptyQueueSprite, queueEmptyColor);

                Image playerQueueSlot = QueueSlot(playerQueueSlots, i);
                if (playerQueueSlot != null)
                    playerQueueSlot.rectTransform.localScale = playerQueueBaseScales[i];
            }
        }

        private Sprite QueueSprite(Enemy enemy)
        {
            if (enemy == null) return emptyQueueSprite;
            return enemy.Sprite != null ? enemy.Sprite : ActionIcon(enemy.PrimaryAnswer());
        }

        private Sprite ActionIcon(PlayerAction action)
        {
            switch (action)
            {
                case PlayerAction.Guard: return guardActionIcon;
                case PlayerAction.Attack: return attackActionIcon;
                case PlayerAction.Charge: return chargeActionIcon;
                default: return emptyQueueSprite;
            }
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

        private void BeginPlayerQueueSlot(int slot)
        {
            if (slot < 0 || slot >= QueueSlotCount) return;
            playerQueueResolved[slot] = false;
            playerQueueBlinkTimers[slot] = queueBlinkDuration;
            SetQueueSlot(playerQueueSlots, slot, emptyQueueSprite, queueWaitingColor);
        }

        private void ResolvePlayerQueueSlot(int slot, JudgeResult result)
        {
            if (slot < 0 || slot >= QueueSlotCount) return;

            bool playerDamaged = result.PlayerDamage > 0;
            bool enemyDamaged = result.Cleared;
            Color resultColor = playerDamaged == enemyDamaged
                ? Color.white
                : (playerDamaged ? queueDamageColor : queueSuccessColor);

            playerQueueResolved[slot] = true;
            playerQueueBlinkTimers[slot] = 0f;
            SetQueueSlot(playerQueueSlots, slot, ActionIcon(result.Input), resultColor);

            Image queueSlot = QueueSlot(playerQueueSlots, slot);
            if (queueSlot != null) queueSlot.rectTransform.localScale = playerQueueBaseScales[slot];
        }

        private void UpdateQueueAnimations()
        {
            for (int i = 0; i < QueueSlotCount; i++)
            {
                Image slot = QueueSlot(playerQueueSlots, i);
                if (slot == null || playerQueueResolved[i] || playerQueueBlinkTimers[i] <= 0f) continue;

                playerQueueBlinkTimers[i] = Mathf.Max(0f, playerQueueBlinkTimers[i] - Time.deltaTime);
                float elapsed = queueBlinkDuration - playerQueueBlinkTimers[i];
                float pulse = Mathf.Abs(Mathf.Cos(elapsed * queueBlinkSpeed));
                Color color = queueWaitingColor;
                color.a = Mathf.Lerp(queueEmptyColor.a, queueWaitingColor.a, pulse);
                slot.color = color;

                float scale = 1f + queuePulseScale * pulse;
                slot.rectTransform.localScale = playerQueueBaseScales[i] * scale;
                if (playerQueueBlinkTimers[i] <= 0f)
                    slot.rectTransform.localScale = playerQueueBaseScales[i];
            }
        }

        private void PlayActionEffect(PlayerAction action)
        {
            if (action != PlayerAction.Attack && action != PlayerAction.Guard) return;
            currentActionEffect = action;
            actionEffectTimer = actionEffectDuration;
        }

        private Color ActionEffectColor(Color baseColor)
        {
            if (actionEffectTimer <= 0f || actionEffectDuration <= 0f) return baseColor;

            float normalized = 1f - actionEffectTimer / actionEffectDuration;
            float strength = Mathf.Sin(normalized * Mathf.PI);
            Color effectColor = currentActionEffect == PlayerAction.Guard
                ? defenseEffectColor
                : attackEffectColor;
            return Color.Lerp(baseColor, effectColor, strength);
        }

        private void UpdateActionMotion()
        {
            Transform target = playerSlot.transform;
            target.localPosition -= playerActionOffset;
            playerActionOffset = Vector3.zero;

            if (actionEffectTimer <= 0f || actionEffectDuration <= 0f) return;

            actionEffectTimer = Mathf.Max(0f, actionEffectTimer - Time.deltaTime);
            if (currentActionEffect != PlayerAction.Attack) return;

            float normalized = 1f - actionEffectTimer / actionEffectDuration;
            float distance = Mathf.Sin(normalized * Mathf.PI) * attackLungeDistance;
            playerActionOffset = new Vector3(distance, 0f, 0f);
            target.localPosition += playerActionOffset;
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
