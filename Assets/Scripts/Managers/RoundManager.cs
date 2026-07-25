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

        [Header("판정 (인스펙터 조정)")]
        [Tooltip("마감까지 안 답한 박을 오답으로 취급(적 defaultOutcome 적용)")]
        [SerializeField] private bool noInputCountsAsMiss = true;
        [Tooltip("박 종료 전 입력 창이 닫히는 지점(박 길이 비율)")]
        [SerializeField, Range(0f, 0.45f)] private float inputEndOffset = 0.12f;
        [Tooltip("Miss 직후 다음 입력을 잠그는 시간(초)")]
        [SerializeField, Min(0f)] private float missInputLockDuration = 0.005f;
        [SerializeField] private bool verboseLog = true;

        [Header("점수 (인스펙터 조정)")]
        [SerializeField, Min(0)] private int hitScore = 100;
        [SerializeField, Min(0)] private int clearScore = 100;
        [SerializeField, Min(0)] private int maxTimingBonus = 100;
        [SerializeField, Min(0)] private int hpScoreWeight = 25;
        [SerializeField, Min(0)] private int armorScoreWeight = 50;

        // 이벤트 (UI가 구독)
        public event Action<int, Enemy> OnEnemyRevealed;              // (slot, enemy) 제시
        public event Action<int, Enemy, JudgeResult> OnJudged;       // (slot, enemy, result) 판정
        public event Action<int, PhaseSO> OnPhasePreparing;          // (cycleIndex, phase) 페이즈 준비(전환 직전)
        public event Action<int, PhaseSO> OnPhaseChanged;            // (cycleIndex, phase) 페이즈 시작
        public event Action<int> OnCycleStarted;                     // 큐 등 사이클 단위 표시 초기화
        public event Action<int> OnScoreChanged;
        public event Action<int, bool> OnScoreAwarded;               // (획득량, 처치 보너스 여부)
        public event Action<bool> OnAttackLanded;                    // 강공격 여부
        public event Action OnGameOver;
        public event Action OnStageCleared;                          // 스테이지 클리어(현재 모델 미발생 — 연출 구독용)

        public PhaseSO CurrentPhase { get; private set; }
        public int Score { get; private set; }

        private sealed class ResponseNote
        {
            public int slot;
            public int globalBeat;
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
        private double inputLockedUntilRealtime = double.NegativeInfinity;
        private bool isOver;

        public int Seed => seed;

        /// <summary>스테이지 지정(StageManager가 Awake 전에 호출 — DefaultExecutionOrder).</summary>
        public void SetStage(StageSO s) => stage = s;

        private void Awake()
        {
            if (stage != null) ApplyStage(stage);
            if (randomizeSeed) seed = Environment.TickCount;
            provider = new EnemySequenceProvider(seed, enemyPool);
            spotlightBeats = pattern != null ? pattern.SpotlightBeatIndices() : new List<int>();
        }

        /// <summary>스테이지 데이터로 이 매니저·플레이어·입력·비트를 구성한다.</summary>
        private void ApplyStage(StageSO s)
        {
            if (s.enemyPool != null && s.enemyPool.Count > 0) enemyPool = new List<Enemy>(s.enemyPool);
            if (s.pattern != null) pattern = s.pattern;
            if (s.phases != null) phases = new List<PhaseSO>(s.phases);
            cyclesPerPhase = Mathf.Max(1, s.cyclesPerPhase);
            if (input != null) input.Mode = s.keyMode;
            if (conductor != null) conductor.Bpm = s.bpm;
            if (player != null) player.SetMaxHp(s.playerMaxHp);
            if (s.backgroundPrefab != null) Instantiate(s.backgroundPrefab);
            if (verboseLog) Debug.Log($"[Round] 스테이지 적용: {s.stageNumber} {s.displayName} (키 {s.keyMode}, 적 {(s.enemyPool != null ? s.enemyPool.Count : 0)})");
        }

        private void OnEnable()
        {
            if (conductor != null)
            {
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
                conductor.OnPresentMeasureStart -= HandlePresentStart;
                conductor.OnResponseMeasureStart -= HandleResponseStart;
                conductor.OnResponseMeasureEnd -= HandleResponseEnd;
                conductor.OnBeat -= HandleBeat;
            }
            if (input != null) input.OnTimedAction -= QueueInput;
            if (player != null) player.OnDied -= HandleDied;
        }

        private void HandlePresentStart(int cycleIndex)
        {
            if (isOver) return;

            // 정상 경계에서는 LateUpdate가 그 프레임 입력을 먼저 소비한 뒤 마감한다.
            // 종료 이벤트 없이 사이클이 넘어온 경우에만 즉시 안전 마감한다.
            if (!responseEndPending) FlushUnanswered();
            if (isOver) return;

            PhaseSO phase = PhaseForCycle(cycleIndex);
            if (phase != CurrentPhase)
            {
                CurrentPhase = phase;
                OnPhasePreparing?.Invoke(cycleIndex, phase);
                OnPhaseChanged?.Invoke(cycleIndex, phase);
                if (verboseLog) Debug.Log($"[Round] >> 페이즈: {(phase != null ? phase.PhaseName : "(균등)")}");
            }

            int count = pattern != null ? pattern.SpotlightCount : 0;
            currentCycle = provider.GenerateCycleWeighted(cycleIndex, count, phase);
            inputLockedUntilRealtime = double.NegativeInfinity;
            OnCycleStarted?.Invoke(cycleIndex);
            if (verboseLog) Debug.Log($"[Round] === 사이클 {cycleIndex} 제시 시작 (적 {count}) ===");
        }

        private void HandleResponseStart(int cycleIndex)
        {
            if (isOver) return;

            // 각 응답 행동은 대응하는 한 박 전체를 입력 구간으로 사용한다.
            notes.Clear();
            for (int k = 0; k < spotlightBeats.Count && k < currentCycle.Count; k++)
            {
                int beatInCycle = Conductor.ResponseStartBeat + k;
                int globalBeat = cycleIndex * Conductor.BeatsPerCycle + beatInCycle;
                double beatStart = conductor != null ? conductor.BeatToTime(globalBeat) : 0.0;
                double beatEnd = conductor != null ? conductor.BeatToTime(globalBeat + 1) : beatStart;
                double beatDuration = beatEnd - beatStart;
                notes.Add(new ResponseNote
                {
                    slot = k,
                    globalBeat = globalBeat,
                    enemy = currentCycle[k],
                    openTime = beatStart,
                    closeTime = beatEnd - beatDuration * inputEndOffset,
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

            OnEnemyRevealed?.Invoke(slot, currentCycle[slot]);
            if (verboseLog) Debug.Log($"[Round] 제시 slot{slot}: {currentCycle[slot]?.DisplayName}");
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

            RefreshPendingNoteWindows();
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

            RefreshPendingNoteWindows();
            double inputSongTime = conductor.RealtimeToSongPosition(timedAction.Realtime);
            ExpireElapsedNotes(inputSongTime);
            if (isOver || timedAction.Realtime < inputLockedUntilRealtime) return;

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

            if (current < 0) return;

            notes[current].consumed = true;
            input?.NotifyAccepted(timedAction.Action);
            double duration = notes[current].closeTime - notes[current].openTime;
            float responseRatio = duration > 0.0
                ? Mathf.Clamp01((float)((inputSongTime - notes[current].openTime) / duration))
                : 1f;
            ApplyJudge(
                notes[current].slot,
                notes[current].enemy,
                timedAction.Action,
                isMiss: false,
                responseRatio: responseRatio);
            conductor.AdvanceResponseBeatNow(notes[current].slot);
        }

        private void RefreshPendingNoteWindows()
        {
            if (conductor == null) return;

            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i].consumed) continue;
                double beatStart = conductor.BeatToTime(notes[i].globalBeat);
                double beatEnd = conductor.BeatToTime(notes[i].globalBeat + 1);
                notes[i].openTime = beatStart;
                notes[i].closeTime = beatEnd - (beatEnd - beatStart) * inputEndOffset;
            }
        }

        private void ExpireElapsedNotes(double now)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i].consumed || now < notes[i].closeTime) continue;

                notes[i].consumed = true;
                if (noInputCountsAsMiss)
                {
                    ApplyJudge(
                        notes[i].slot,
                        notes[i].enemy,
                        PlayerAction.None,
                        isMiss: true,
                        responseRatio: 1f);
                    inputLockedUntilRealtime = Math.Max(
                        inputLockedUntilRealtime,
                        Time.realtimeSinceStartupAsDouble + missInputLockDuration);
                }

                conductor?.AdvanceResponseBeatNow(notes[i].slot);
                if (isOver) break;
                break; // 시간축이 다음 Beat 시작으로 이동했으므로 다음 노트는 다음 프레임에 검사
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
                    ApplyJudge(
                        notes[i].slot,
                        notes[i].enemy,
                        PlayerAction.None,
                        isMiss: true,
                        responseRatio: 1f);
                if (isOver) break;
            }

            notes.Clear();
            inResponse = false;
            responseEndPending = false;
            responseEndFrame = -1;
        }

        private void ApplyJudge(int slot, Enemy enemy, PlayerAction action, bool isMiss, float responseRatio)
        {
            bool charged = player != null && player.IsCharged;
            JudgeResult result = JudgeSystem.Judge(enemy, action, charged); // 양측 행동 조합 판정

            // 공격이 '정답'이면 방어력·HP·강공격을 반영
            if (action == PlayerAction.Attack && result.Cleared && enemy != null && player != null)
            {
                float power = charged
                    ? player.AttackPower * player.ChargedAttackMultiplier
                    : player.AttackPower;
                float dmg = (charged && player.ChargedPiercesArmor)
                    ? power
                    : Mathf.Max(0f, power - enemy.Armor);
                if (dmg < enemy.MaxHp)
                    result = new JudgeResult(
                        action,
                        result.PlayerDamage > 0 ? OutcomeType.Punished : OutcomeType.Safe,
                        result.PlayerDamage,
                        false,
                        enemy.Armor > 0 ? "방어에 막힘 — 차징→강공격 필요" : "위력 부족 — 차징 필요");
                else if (charged)
                    result = new JudgeResult(
                        action,
                        OutcomeType.Cleared,
                        result.PlayerDamage,
                        true,
                        "강공격! 방어 관통");
            }

            // 차징 처리
            if (action == PlayerAction.Attack && player != null) player.ConsumeCharge();
            else if (action == PlayerAction.Charge && player != null)
                player.SetCharged(result.Type != OutcomeType.Punished);

            if (result.PlayerDamage > 0 && player != null) player.TakeDamage(result.PlayerDamage);
            if (result.Cleared) AwardScore(enemy, action, responseRatio);
            if (action == PlayerAction.Attack && result.Cleared) OnAttackLanded?.Invoke(charged);
            OnJudged?.Invoke(slot, enemy, result);
            if (verboseLog)
                Debug.Log($"[Round] {(isMiss ? "무입력" : "응답")} slot{slot}: {enemy?.DisplayName} + {action}{(charged ? "(강)" : "")} → {result.Type} (dmg {result.PlayerDamage}) HP {(player != null ? player.CurrentHp : -1)}");
        }

        private void AwardScore(Enemy enemy, PlayerAction action, float responseRatio)
        {
            if (action == PlayerAction.Attack) QueueScore(hitScore, false);

            int strengthBonus = enemy != null
                ? enemy.MaxHp * hpScoreWeight + enemy.Armor * armorScoreWeight
                : 0;
            int timingBonus = Mathf.RoundToInt((1f - Mathf.Clamp01(responseRatio)) * maxTimingBonus);
            QueueScore(clearScore + strengthBonus + timingBonus, true);
        }

        private void QueueScore(int points, bool isClearBonus)
        {
            if (points <= 0) return;
            if (OnScoreAwarded != null) OnScoreAwarded.Invoke(points, isClearBonus);
            else CommitScore(points);
        }

        /// <summary>점수 획득 연출이 HUD에 도착했을 때 실제 점수에 반영한다.</summary>
        public void CommitScore(int points)
        {
            if (points <= 0) return;
            Score += points;
            OnScoreChanged?.Invoke(Score);
        }

        /// <summary>응답 슬롯의 현재 입력 가능 여부와 Cursor 진행률을 반환한다.</summary>
        public bool TryGetInputWindowProgress(int slot, out float progress)
        {
            progress = 0f;
            if (!inResponse || conductor == null) return false;
            RefreshPendingNoteWindows();

            for (int i = 0; i < notes.Count; i++)
            {
                ResponseNote note = notes[i];
                if (note.slot != slot || note.consumed) continue;

                double now = conductor.SongPosition;
                if (now < note.openTime
                    || now >= note.closeTime
                    || Time.realtimeSinceStartupAsDouble < inputLockedUntilRealtime)
                    return false;

                double duration = note.closeTime - note.openTime;
                progress = duration > 0.0
                    ? Mathf.Clamp01((float)((now - note.openTime) / duration))
                    : 1f;
                return true;
            }
            return false;
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
            pendingInputs.Clear();
            responseEndPending = false;
            if (conductor != null) conductor.StopClock();
            OnGameOver?.Invoke();
            if (verboseLog) Debug.Log("[Round] ===== GAME OVER =====");
        }
    }
}
