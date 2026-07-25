using System.Collections;
using DG.Tweening;
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
                round.OnScoreAwarded -= OnScoreAwarded;
                round.OnScoreChanged -= OnScoreChanged;
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
            SetEnemyIdle();
            SetPlayerIdle();
            if (scoreLabel != null)
            {
                scoreLabel.enabled = true;
                scoreLabel.text = $"SCORE  {(round != null ? round.Score : 0):N0}";
            }
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

        private void OnReveal(int slot, Enemy e)
        {
            if (slot >= 0 && slot < revealedEnemies.Length) revealedEnemies[slot] = e;
            SetQueueSlot(enemyQueueSlots, slot, QueueSprite(e), Color.white);
            if (feedbackLabel != null) feedbackLabel.text = e != null ? e.DisplayName : "";
        }

        private void OnJudged(int slot, Enemy e, JudgeResult r)
        {
            if (e != null && e.Sprite != null)
            {
                SetEnemySprite(e.Sprite);
                enemySpriteTimer = actionSpriteHold;
            }

            if (r.PlayerDamage > 0)
            {
                PlayHit(ref playerDamageFlashTimer, ref playerShakeTimer);
                if (cameraSway != null) cameraSway.Shake();
            }
            if (r.Cleared && r.Input == PlayerAction.Attack)
            {
                PlayEnemyHit();
                PlayAttackMotion();
            }
            ResolveQueueSlot(slot, r);
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
            enemyDamageFlashTimer = damageFlashDuration;
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
