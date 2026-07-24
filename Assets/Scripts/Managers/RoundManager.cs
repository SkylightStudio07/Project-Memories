using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// P0 코어 루프 오케스트레이터.
    /// 제시 마디: 결정론적 시퀀스로 적을 스포트라이트 박마다 순서대로 드러낸다.
    /// 응답 마디: 각 응답 스포트라이트 박에 <b>타이밍 판정창</b>을 두고,
    ///   그 창에 <b>가장 먼저 들어온 입력 하나로 그 박을 확정</b>한다.
    ///   - 틀리게 누르면 그 박은 오답으로 잠긴다(고쳐 눌러도 봐주지 않음).
    ///   - 창을 벗어난 입력은 무시. 마감까지 안 답한 박은 무입력 미스.
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private Conductor conductor;
        [SerializeField] private PlayerData player;
        [SerializeField] private InputReader input;
        [SerializeField] private RhythmPatternSO pattern;
        [SerializeField] private List<Enemy> enemyPool = new List<Enemy>();

        [Header("시퀀스 (인스펙터 조정)")]
        [Tooltip("고정 시드 → 재현 가능. 랜덤화 시 매판 달라짐")]
        [SerializeField] private int seed = 12345;
        [SerializeField] private bool randomizeSeed = false;

        [Header("페이즈 (인스펙터 조정)")]
        [Tooltip("순환할 페이즈들. 비우면 균등 등장")]
        [SerializeField] private List<PhaseSO> phases = new List<PhaseSO>();
        [Tooltip("한 페이즈가 유지되는 사이클 수")]
        [SerializeField, Min(1)] private int cyclesPerPhase = 2;

        [Header("판정 (인스펙터 조정)")]
        [Tooltip("타이밍 판정창: 박 대비 ±비율 (0.35 = ±35%). 클수록 널널")]
        [SerializeField, Range(0.05f, 0.49f)] private float hitWindowRatio = 0.35f;
        [Tooltip("마감까지 안 답한 박을 오답으로 취급(적 defaultOutcome 적용)")]
        [SerializeField] private bool noInputCountsAsMiss = true;
        [SerializeField] private bool verboseLog = true;

        // 이벤트 (UI가 구독)
        public event Action<int, Enemy> OnEnemyRevealed;              // (slot, enemy) 제시
        public event Action<int, Enemy, JudgeResult> OnJudged;       // (slot, enemy, result) 판정
        public event Action<int, PhaseSO> OnPhaseChanged;            // (cycleIndex, phase) 페이즈 시작
        public event Action OnGameOver;

        public PhaseSO CurrentPhase { get; private set; }

        private sealed class ResponseNote
        {
            public int slot;
            public Enemy enemy;
            public double time;   // 이 박의 이상적 시각(SongPosition 기준)
            public bool consumed; // 이미 입력으로 확정됐는가
        }

        private EnemySequenceProvider provider;
        private List<Enemy> currentCycle = new List<Enemy>();
        private List<int> spotlightBeats = new List<int>();
        private readonly List<ResponseNote> notes = new List<ResponseNote>();
        private bool inResponse;
        private bool isOver;

        public int Seed => seed;

        private void Awake()
        {
            if (randomizeSeed) seed = Environment.TickCount;
            provider = new EnemySequenceProvider(seed, enemyPool);
            spotlightBeats = pattern != null ? pattern.SpotlightBeatIndices() : new List<int>();
        }

        private void OnEnable()
        {
            if (conductor != null)
            {
                conductor.OnPresentMeasureStart += HandlePresentStart;
                conductor.OnResponseMeasureStart += HandleResponseStart;
                conductor.OnBeat += HandleBeat;
            }
            if (input != null) input.OnAction += HandleInput;
            if (player != null) player.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            if (conductor != null)
            {
                conductor.OnPresentMeasureStart -= HandlePresentStart;
                conductor.OnResponseMeasureStart -= HandleResponseStart;
                conductor.OnBeat -= HandleBeat;
            }
            if (input != null) input.OnAction -= HandleInput;
            if (player != null) player.OnDied -= HandleDied;
        }

        private void HandlePresentStart(int cycleIndex)
        {
            if (isOver) return;

            // 직전 응답에서 안 답한 박 = 무입력 미스로 마감
            FlushUnanswered();

            PhaseSO phase = PhaseForCycle(cycleIndex);
            if (phase != CurrentPhase)
            {
                CurrentPhase = phase;
                OnPhaseChanged?.Invoke(cycleIndex, phase);
                if (verboseLog) Debug.Log($"[Round] >> 페이즈: {(phase != null ? phase.PhaseName : "(균등)")}");
            }

            int count = pattern != null ? pattern.SpotlightCount : 0;
            currentCycle = provider.GenerateCycleWeighted(cycleIndex, count, phase);
            if (verboseLog) Debug.Log($"[Round] === 사이클 {cycleIndex} 제시 시작 (적 {count}) ===");
        }

        private void HandleResponseStart(int cycleIndex)
        {
            if (isOver) return;

            // 응답 박마다 판정 노트를 만든다 (박의 이상적 시각을 미리 계산)
            notes.Clear();
            for (int k = 0; k < spotlightBeats.Count && k < currentCycle.Count; k++)
            {
                int beatInCycle = spotlightBeats[k] + Conductor.BeatsPerMeasure;
                int globalBeat = cycleIndex * Conductor.BeatsPerCycle + beatInCycle;
                notes.Add(new ResponseNote
                {
                    slot = k,
                    enemy = currentCycle[k],
                    time = conductor != null ? conductor.BeatToTime(globalBeat) : 0.0,
                    consumed = false,
                });
            }
            inResponse = true;
            if (verboseLog) Debug.Log($"[Round] --- 사이클 {cycleIndex} 응답 시작 (노트 {notes.Count}) ---");
        }

        // 제시 마디: 스포트라이트 박마다 적을 드러낸다. (응답 판정은 입력 타이밍 기반)
        private void HandleBeat(int beatInCycle)
        {
            if (isOver) return;
            if (beatInCycle >= Conductor.BeatsPerMeasure) return; // 응답 마디는 타이밍 기반

            int beatInMeasure = beatInCycle % Conductor.BeatsPerMeasure;
            if (pattern == null || !pattern.IsSpotlight(beatInMeasure)) return;

            int slot = spotlightBeats.IndexOf(beatInMeasure);
            if (slot < 0 || slot >= currentCycle.Count) return;

            OnEnemyRevealed?.Invoke(slot, currentCycle[slot]);
            if (verboseLog) Debug.Log($"[Round] 제시 slot{slot}: {currentCycle[slot]?.DisplayName}");
        }

        // 응답 마디 입력: 판정창에 들어오는 가장 가까운 미확정 박을 그 입력으로 확정
        private void HandleInput(PlayerAction action)
        {
            if (isOver || !inResponse || conductor == null) return;

            double now = conductor.SongPosition;
            double window = Mathf.Max(0f, hitWindowRatio) * conductor.SecondsPerBeat;

            int best = -1;
            double bestDist = double.MaxValue;
            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i].consumed) continue;
                double d = Math.Abs(now - notes[i].time);
                if (d <= window && d < bestDist) { bestDist = d; best = i; }
            }

            if (best < 0) return; // 창 밖 입력(빗나감) — 무시. 미답 박은 마감 시 미스.

            notes[best].consumed = true;
            ApplyJudge(notes[best].slot, notes[best].enemy, action, isMiss: false);
        }

        private void FlushUnanswered()
        {
            if (!inResponse) return;
            if (noInputCountsAsMiss)
                for (int i = 0; i < notes.Count; i++)
                    if (!notes[i].consumed)
                        ApplyJudge(notes[i].slot, notes[i].enemy, PlayerAction.None, isMiss: true);
            notes.Clear();
            inResponse = false;
        }

        private void ApplyJudge(int slot, Enemy enemy, PlayerAction action, bool isMiss)
        {
            JudgeResult result = JudgeSystem.Judge(enemy, action); // 표: 정/오답
            bool charged = player != null && player.IsCharged;

            // 공격이 '정답'이면 방어력·HP·강공격을 반영
            if (action == PlayerAction.Attack && result.Cleared && enemy != null && player != null)
            {
                int power = charged ? player.ChargedAttackPower : player.AttackPower;
                int dmg = (charged && player.ChargedPiercesArmor) ? power : Mathf.Max(0, power - enemy.Armor);
                if (dmg < enemy.MaxHp)
                    result = new JudgeResult(action, OutcomeType.Safe, 0, false,
                        enemy.Armor > 0 ? "방어에 막힘 — 차징→강공격 필요" : "위력 부족 — 차징 필요");
                else if (charged)
                    result = new JudgeResult(action, OutcomeType.Cleared, 0, true, "강공격! 방어 관통");
            }

            // 차징 처리
            if (action == PlayerAction.Attack && player != null) player.ConsumeCharge();
            else if (action == PlayerAction.Charge && player != null && result.Type != OutcomeType.Punished)
                player.SetCharged(true);

            if (result.PlayerDamage > 0 && player != null) player.TakeDamage(result.PlayerDamage);
            OnJudged?.Invoke(slot, enemy, result);
            if (verboseLog)
                Debug.Log($"[Round] {(isMiss ? "무입력" : "응답")} slot{slot}: {enemy?.DisplayName} + {action}{(charged ? "(강)" : "")} → {result.Type} (dmg {result.PlayerDamage}) HP {(player != null ? player.CurrentHp : -1)}");
        }

        private PhaseSO PhaseForCycle(int cycleIndex)
        {
            if (phases == null || phases.Count == 0) return null;
            int block = Mathf.Max(1, cyclesPerPhase);
            int idx = (cycleIndex / block) % phases.Count;
            return phases[idx];
        }

        private void HandleDied()
        {
            if (isOver) return;
            isOver = true;
            if (conductor != null) conductor.StopClock();
            OnGameOver?.Invoke();
            if (verboseLog) Debug.Log("[Round] ===== GAME OVER =====");
        }
    }
}
