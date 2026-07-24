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
        [Header("참조")]
        [SerializeField] private RoundManager round;
        [SerializeField] private Conductor conductor;
        [SerializeField] private PlayerData player;

        [Header("캐릭터 (월드 SpriteRenderer)")]
        [SerializeField] private SpriteRenderer enemySlot;
        [SerializeField] private SpriteRenderer playerSlot;
        [Tooltip("스프라이트를 이 월드 높이로 맞춤(원본 비율 유지). 0이면 스케일 고정")]
        [SerializeField] private float enemyWorldHeight = 2.4f;
        [SerializeField] private float playerWorldHeight = 2.0f;

        [Header("HUD (Canvas)")]
        [SerializeField] private Image[] beatDots = new Image[8];
        [SerializeField] private Image[] hearts;
        [SerializeField] private Text phaseLabel;
        [SerializeField] private Text feedbackLabel;
        [SerializeField] private Text gameOverLabel;
        [SerializeField] private Text countdownLabel;
        [SerializeField] private Text chargeLabel;

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

        private Color enemyTarget;
        private float enemyFlash;
        private float enemySpriteTimer;
        private Color playerTarget;
        private float playerFlash;
        private float playerSpriteTimer;

        private void OnEnable()
        {
            if (round != null)
            {
                round.OnEnemyRevealed += OnReveal;
                round.OnJudged += OnJudged;
                round.OnPhaseChanged += OnPhase;
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
                round.OnGameOver -= OnGameOver;
            }
            if (conductor != null) conductor.OnBeat -= OnBeat;
            if (player != null)
            {
                player.OnHealthChanged -= OnHealth;
                player.OnSpriteChanged -= OnPlayerSprite;
                player.OnChargedChanged -= OnCharged;
            }
        }

        private void Start()
        {
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
            SetDots(-1);
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
                enemySlot.color = Color.Lerp(Color.white, enemyTarget, enemyFlash); // 평상시 자연색, 이벤트 시 잠깐 틴트
            }
            if (playerSlot != null)
            {
                if (playerSpriteTimer > 0f)
                {
                    playerSpriteTimer -= Time.deltaTime;
                    if (playerSpriteTimer <= 0f) SetPlayerIdle();
                }
                playerFlash = Mathf.MoveTowards(playerFlash, 0f, Time.deltaTime / flashFade);
                playerSlot.color = Color.Lerp(Color.white, playerTarget, playerFlash); // 평상시 자연색
            }
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
            enemyTarget = PoseColor(e);
            enemyFlash = 1f;
            if (e != null && e.Sprite != null)
            {
                FitSprite(enemySlot, e.Sprite, enemyWorldHeight); // 자세 스프라이트(원본 비율 자동)
                enemySpriteTimer = actionSpriteHold;
            }
            if (feedbackLabel != null) feedbackLabel.text = e != null ? e.DisplayName : "";
        }

        private void OnJudged(int slot, Enemy e, JudgeResult r)
        {
            Color c = ResultColor(r.Type);
            enemyTarget = c; enemyFlash = 1f;
            playerTarget = c; playerFlash = 1f;
            if (feedbackLabel != null)
                feedbackLabel.text = string.IsNullOrEmpty(r.Feedback) ? $"{r.Input} → {r.Type}" : r.Feedback;
        }

        private void OnPhase(int cycle, PhaseSO p)
        {
            if (phaseLabel != null) phaseLabel.text = p != null ? p.PhaseName : "(균등)";
        }

        private void OnGameOver()
        {
            if (gameOverLabel != null) { gameOverLabel.enabled = true; gameOverLabel.text = "GAME OVER"; }
        }

        private void OnBeat(int beatInCycle) => SetDots(beatInCycle);

        private void OnHealth(int current, int max)
        {
            if (hearts == null) return;
            for (int i = 0; i < hearts.Length; i++)
                if (hearts[i] != null) hearts[i].color = i < current ? heartFull : heartEmpty;
        }

        private void OnPlayerSprite(Sprite s)
        {
            FitSprite(playerSlot, s, playerWorldHeight); // 행동 스프라이트(원본 비율 자동)
            if (s != null) playerSpriteTimer = actionSpriteHold;
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
            FitSprite(enemySlot, enemyIdleSprite, enemyWorldHeight); // null이면 현재 placeholder 유지
        }

        private void SetPlayerIdle()
        {
            FitSprite(playerSlot, player != null ? player.IdleSprite : null, playerWorldHeight);
        }

        [Header("idle 스프라이트")]
        [Tooltip("적 슬롯 idle(기본) 스프라이트 — 없으면 현재 placeholder 유지")]
        [SerializeField] private Sprite enemyIdleSprite;

        // 스프라이트를 원본 비율 유지(SpriteRenderer 자동)하며 목표 월드 높이로 스케일. null이면 현재 스프라이트 유지.
        private static void FitSprite(SpriteRenderer sr, Sprite s, float worldHeight)
        {
            if (sr == null || s == null) return;
            sr.sprite = s;
            float h = s.bounds.size.y;
            if (h > 0f && worldHeight > 0f)
                sr.transform.localScale = Vector3.one * (worldHeight / h);
        }

        private void SetDots(int active)
        {
            if (beatDots == null) return;
            for (int i = 0; i < beatDots.Length; i++)
            {
                if (beatDots[i] == null) continue;
                Color onColor = i < Conductor.BeatsPerMeasure ? presentDotColor : responseDotColor;
                beatDots[i].color = i == active ? onColor : dotOffColor;
            }
        }
    }
}
