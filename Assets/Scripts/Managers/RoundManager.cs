using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// P0 코어 루프 오케스트레이터.
    /// 제시 구간: 결정론적 시퀀스로 적을 스포트라이트 박마다 순서대로 드러낸다.
    /// 응답 구간: 네 번의 제시 직후 이어지는 각 박의
    ///   <b>가장 먼저 들어온 입력 하나로 그 행동을 확정</b>한다.
    ///   - 틀리게 누르면 그 박은 오답으로 잠긴다(고쳐 눌러도 봐주지 않음).
    ///   - 창을 벗어난 입력은 무시. 마감까지 안 답한 박은 무입력 미스.
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        [Header("스테이지 (지정 시 아래 값을 덮어씀)")]
        [SerializeField] private StageSO stage;

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
        [Tooltip("마지막 페이즈 뒤 처음으로 순환. 끄면 마지막 응답 뒤 스테이지 클리어")]
        [SerializeField] private bool repeatPhasePlan = true;

        [Header("판정 (인스펙터 조정)")]
        [Tooltip("마감까지 안 답한 박을 오답으로 취급(적 defaultOutcome 적용)")]
        [SerializeField] private bool noInputCountsAsMiss = true;
        [SerializeField] private bool verboseLog = true;

        // 이벤트 (UI가 구독)
        public event Action<EnemyPreviewCue> OnEnemyPreviewed;        // 정제된 제시(차폐 시 실제 적 없음)
        public event Action<int, Enemy, JudgeResult> OnJudged;       // (slot, enemy, result) 판정
        public event Action<int, PhaseSO> OnPhasePreparing;          // (phaseIndex, phase) 준비 시작
        public event Action<int, PhaseSO> OnPhaseChanged;            // (phaseIndex, phase) 본체 시작
        public event Action OnGameOver;
        public event Action OnStageCleared;

        public PhaseSO CurrentPhase { get; private set; }
        public int CurrentPhaseIndex { get; private set; } = -1;

        private sealed class ResponseNote
        {
            public int slot;
            public Enemy enemy;
            public double openTime;  // 입력 구간의 시작(SongPosition 기준)
            public double closeTime; // 입력 구간의 끝. [openTime, closeTime)
            public bool consumed; // 이미 입력으로 확정됐는가
        }

        private EnemySequenceProvider provider;
        private List<Enemy> currentCycle = new List<Enemy>();
        private List<int> spotlightBeats = new List<int>();
        private readonly List<ResponseNote> notes = new List<ResponseNote>();
        private readonly Queue<TimedPlayerAction> pendingInputs = new Queue<TimedPlayerAction>();
        private bool inResponse;
        private bool responseEndPending;
        private int responseEndFrame = -1;
        private bool isOver;
        private int activePhaseIndex = -1;

        public int Seed => seed;

        /// <summary>스테이지 지정(StageManager가 Awake 전에 호출 — DefaultExecutionOrder).</summary>
        public void SetStage(StageSO s) => stage = s;

        private void Awake()
        {
            if (stage != null) ApplyStage(stage);
            if (randomizeSeed) seed = Environment.TickCount;
            provider = new EnemySequenceProvider(seed, enemyPool);
            spotlightBeats = pattern != null ? pattern.SpotlightBeatIndices() : new List<int>();
            if (conductor != null) conductor.ConfigureTimeline(cyclesPerPhase);
        }

        /// <summary>스테이지 데이터로 이 매니저·플레이어·입력·비트를 구성한다.</summary>
        private void ApplyStage(StageSO s)
        {
            if (s.enemyPool != null && s.enemyPool.Count > 0) enemyPool = new List<Enemy>(s.enemyPool);
            if (s.pattern != null) pattern = s.pattern;
            if (s.phases != null) phases = new List<PhaseSO>(s.phases);
            cyclesPerPhase = Mathf.Max(1, s.cyclesPerPhase);
            repeatPhasePlan = s.repeatPhasePlan;
            if (input != null) input.Mode = s.keyMode;
            if (conductor != null)
            {
                conductor.Bpm = s.bpm;
                conductor.StartDelay = s.startDelay;
                conductor.ConfigureTimeline(cyclesPerPhase);
            }
            if (player != null) player.SetMaxHp(s.playerMaxHp);
            if (s.backgroundPrefab != null) Instantiate(s.backgroundPrefab);
            if (verboseLog) Debug.Log($"[Round] 스테이지 적용: {s.stageNumber} {s.displayName} (키 {s.keyMode}, 적 {(s.enemyPool != null ? s.enemyPool.Count : 0)})");
        }

        private void OnEnable()
        {
            if (conductor != null)
            {
                conductor.OnPreparationMeasureStart += HandlePreparationStart;
                conductor.OnPresentMeasureStart += HandlePresentStart;
                conductor.OnResponseMeasureStart += HandleResponseStart;
                conductor.OnResponseMeasureEnd += HandleResponseEnd;
                conductor.OnBeat += HandleBeat;
            }
            if (input != null) input.OnTimedAction += QueueInput;
            if (player != null) player.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            if (conductor != null)
            {
                conductor.OnPreparationMeasureStart -= HandlePreparationStart;
                conductor.OnPresentMeasureStart -= HandlePresentStart;
                conductor.OnResponseMeasureStart -= HandleResponseStart;
                conductor.OnResponseMeasureEnd -= HandleResponseEnd;
                conductor.OnBeat -= HandleBeat;
            }
            if (input != null) input.OnTimedAction -= QueueInput;
            if (player != null) player.OnDied -= HandleDied;
        }

        private void HandlePreparationStart(int phaseIndex)
        {
            if (isOver) return;

            PhaseSO phase = PhaseForIndex(phaseIndex);
            if (phase == null && IsPhasePlanComplete(phaseIndex))
            {
                HandleStageCleared();
                return;
            }

            CurrentPhaseIndex = phaseIndex;
            CurrentPhase = phase;
            activePhaseIndex = -1;
            OnPhasePreparing?.Invoke(phaseIndex, phase);
            if (verboseLog)
                Debug.Log($"[Round] >> 준비: {(phase != null ? phase.PhaseName : "(균등)")}");
        }

        private void HandlePresentStart(int cycleIndex)
        {
            if (isOver) return;

            // 정상 경계에서는 LateUpdate가 그 프레임 입력을 먼저 소비한 뒤 마감한다.
            // 종료 이벤트 없이 사이클이 넘어온 경우에만 즉시 안전 마감한다.
            if (!responseEndPending) FlushUnanswered();
            if (isOver) return;

            int phaseIndex = conductor != null
                ? conductor.PhaseIndex
                : cycleIndex / Mathf.Max(1, cyclesPerPhase);
            PhaseSO phase = CurrentPhaseIndex == phaseIndex ? CurrentPhase : PhaseForIndex(phaseIndex);
            if (CurrentPhaseIndex != phaseIndex)
            {
                CurrentPhaseIndex = phaseIndex;
                CurrentPhase = phase;
            }
            if (activePhaseIndex != phaseIndex)
            {
                activePhaseIndex = phaseIndex;
                OnPhaseChanged?.Invoke(phaseIndex, phase);
                if (verboseLog)
                    Debug.Log($"[Round] >> 페이즈: {(phase != null ? phase.PhaseName : "(균등)")}");
            }

            int count = pattern != null ? pattern.SpotlightCount : 0;
            int exchangeInPhase = conductor != null
                ? conductor.ExchangeInPhase
                : cycleIndex % Mathf.Max(1, cyclesPerPhase);
            currentCycle = GenerateCycle(cycleIndex, exchangeInPhase, count, phase);
            if (verboseLog) Debug.Log($"[Round] === 사이클 {cycleIndex} 제시 시작 (적 {count}) ===");
        }

        private void HandleResponseStart(int cycleIndex)
        {
            if (isOver) return;

            // 각 응답 행동은 대응하는 한 박 전체를 입력 구간으로 사용한다.
            notes.Clear();
            for (int k = 0; k < spotlightBeats.Count && k < currentCycle.Count; k++)
            {
                int globalBeat = conductor != null ? conductor.TotalBeats + k : 0;
                double openTime = conductor != null ? conductor.BeatToTime(globalBeat) : 0.0;
                notes.Add(new ResponseNote
                {
                    slot = k,
                    enemy = currentCycle[k],
                    openTime = openTime,
                    closeTime = conductor != null ? conductor.BeatToTime(globalBeat + 1) : openTime,
                    consumed = false,
                });
            }
            inResponse = true;
            if (verboseLog) Debug.Log($"[Round] --- 사이클 {cycleIndex} 응답 시작 (노트 {notes.Count}) ---");
        }

        private void HandleResponseEnd(int cycleIndex)
        {
            if (isOver) return;

            // Input System과 EventSystem이 같은 프레임의 입력을 모두 전달할 때까지 마감을 미룬다.
            responseEndPending = true;
            responseEndFrame = Time.frameCount;
        }

        // 제시 구간: 네 박 동안 스포트라이트 박마다 적을 드러낸다.
        private void HandleBeat(int beatInCycle)
        {
            if (isOver) return;
            if (beatInCycle >= Conductor.ResponseStartBeat) return;

            int beatInMeasure = beatInCycle;
            if (pattern == null || !pattern.IsSpotlight(beatInMeasure)) return;

            int slot = spotlightBeats.IndexOf(beatInMeasure);
            if (slot < 0 || slot >= currentCycle.Count) return;

            Enemy actualEnemy = currentCycle[slot];
            bool hidden = CurrentPhase != null && CurrentPhase.ShouldHidePreview(actualEnemy);
            var cue = new EnemyPreviewCue(slot, actualEnemy, hidden);
            OnEnemyPreviewed?.Invoke(cue);
            if (verboseLog)
                Debug.Log($"[Round] 제시 slot{slot}: {(hidden ? "[차폐]" : actualEnemy?.DisplayName)}");
        }

        private void LateUpdate()
        {
            if (isOver || conductor == null)
            {
                pendingInputs.Clear();
                return;
            }

            while (pendingInputs.Count > 0 && !isOver)
                ConsumeInput(pendingInputs.Dequeue());

            if (isOver) return;
            if (!inResponse)
            {
                responseEndPending = false;
                responseEndFrame = -1;
                return;
            }

            ExpireElapsedNotes(conductor.SongPosition);

            if (responseEndPending)
            {
                FlushUnanswered();
                responseEndPending = false;
                responseEndFrame = -1;
            }
        }

        private void QueueInput(TimedPlayerAction timedAction)
        {
            if (!isOver) pendingInputs.Enqueue(timedAction);
        }

        // 입력이 처리된 프레임이 아니라 Input System이 기록한 실제 발생 시각으로 행동 구간을 찾는다.
        private void ConsumeInput(TimedPlayerAction timedAction)
        {
            if (isOver || !inResponse || conductor == null) return;

            double inputSongTime = conductor.RealtimeToSongPosition(timedAction.Realtime);
            int current = -1;
            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i].consumed) continue;
                if (inputSongTime >= notes[i].openTime && inputSongTime < notes[i].closeTime)
                {
                    current = i;
                    break;
                }
            }

            // UI 포인터 이벤트는 Conductor 뒤에 전달될 수 있다. 같은 경계 프레임의 입력은
            // 새 제시 구간이 입력을 받지 않으므로 직전 네 번째 행동에 우선 귀속한다.
            if (current < 0 && responseEndPending && timedAction.Frame == responseEndFrame)
            {
                int last = notes.Count - 1;
                if (last >= 0 && !notes[last].consumed) current = last;
            }

            if (current < 0) return;

            notes[current].consumed = true;
            input?.NotifyAccepted(timedAction.Action);
            ApplyJudge(notes[current].slot, notes[current].enemy, timedAction.Action, isMiss: false);
        }

        private void ExpireElapsedNotes(double now)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i].consumed || now < notes[i].closeTime) continue;

                notes[i].consumed = true;
                if (noInputCountsAsMiss)
                    ApplyJudge(notes[i].slot, notes[i].enemy, PlayerAction.None, isMiss: true);

                if (isOver) break;
            }
        }

        private void FlushUnanswered()
        {
            if (!inResponse) return;

            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i].consumed) continue;

                notes[i].consumed = true;
                if (noInputCountsAsMiss)
                    ApplyJudge(notes[i].slot, notes[i].enemy, PlayerAction.None, isMiss: true);
                if (isOver) break;
            }

            notes.Clear();
            inResponse = false;
            responseEndPending = false;
            responseEndFrame = -1;
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

        private List<Enemy> GenerateCycle(int cycleIndex, int exchangeInPhase, int count, PhaseSO phase)
        {
            int n = Mathf.Max(0, count);
            var generated = new List<Enemy>(n);
            for (int slot = 0; slot < n; slot++)
            {
                if (phase != null
                    && phase.TryGetAuthoredEnemy(exchangeInPhase, slot, n, out Enemy authored))
                {
                    generated.Add(authored);
                }
                else
                {
                    generated.Add(provider.GetWeighted(cycleIndex, slot, phase));
                }
            }
            return generated;
        }

        private PhaseSO PhaseForIndex(int phaseIndex)
        {
            if (phases == null || phases.Count == 0) return null;
            if (phaseIndex < 0) return null;
            if (repeatPhasePlan) return phases[phaseIndex % phases.Count];
            return phaseIndex < phases.Count ? phases[phaseIndex] : null;
        }

        private bool IsPhasePlanComplete(int phaseIndex)
            => !repeatPhasePlan
               && phases != null
               && phases.Count > 0
               && phaseIndex >= phases.Count;

        private void HandleStageCleared()
        {
            if (isOver) return;
            isOver = true;
            pendingInputs.Clear();
            responseEndPending = false;
            if (conductor != null) conductor.StopClock();
            OnStageCleared?.Invoke();
            if (verboseLog) Debug.Log("[Round] ===== STAGE CLEAR =====");
        }

        private void HandleDied()
        {
            if (isOver) return;
            isOver = true;
            pendingInputs.Clear();
            responseEndPending = false;
            if (conductor != null) conductor.StopClock();
            OnGameOver?.Invoke();
            if (verboseLog) Debug.Log("[Round] ===== GAME OVER =====");
        }
    }
}
