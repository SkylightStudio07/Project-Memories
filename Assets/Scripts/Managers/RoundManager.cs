using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatMemories
{
    public enum RhythmTimingResult
    {
        Success,
        TooEarly,
        TooLate,
    }

    /// <summary>
    /// P0 코어 루프 오케스트레이터.
    /// 제시 구간: 결정론적 시퀀스로 적을 스포트라이트 박마다 순서대로 드러낸다.
    /// 응답 구간: 각 박의 슬롯 게이지와 전역 Early/Late Offset으로 입력을 판정한다.
    ///   - Too Early는 행동을 잠그고 정박에서 쉼으로 처리한다.
    ///   - Success는 선택 행동을 처리한다.
    ///   - Too Late는 Offset 끝에서 쉼으로 처리한다.
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        [Header("스테이지 (지정 시 아래 값을 덮어씀)")]
        [SerializeField] private StageSO stage;
        [Tooltip("씬에서 지정한 입력 모드를 유지한다. 개별 기능 테스트 씬에서만 사용.")]
        [SerializeField] private bool keepSceneInputMode;

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
        [SerializeField] private bool repeatPhasePlan = true;
        [SerializeField, Min(0)] private int phasePreparationBeats;

        [Header("판정 (인스펙터 조정)")]
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
        public event Action<EnemyPreviewCue> OnEnemyPreviewed;
        public event Action<int, Enemy, JudgeResult> OnJudged;       // (slot, enemy, result) 판정
        public event Action<int, RhythmTimingResult> OnTimingJudged; // 빠름/정확/느림 판정
        public event Action<int, RhythmTimingResult, double> OnTimingFrameResolved;
        public event Action<int, PhaseSO> OnPhasePreparing;          // (cycleIndex, phase) 페이즈 준비(전환 직전)
        public event Action<int, PhaseSO> OnPhaseChanged;            // (cycleIndex, phase) 페이즈 시작
        public event Action<int> OnCycleStarted;                     // 큐 등 사이클 단위 표시 초기화
        public event Action<int> OnScoreChanged;
        public event Action<int, bool> OnScoreAwarded;               // (획득량, 처치 보너스 여부)
        public event Action<bool> OnAttackLanded;                    // 강공격 여부
        public event Action<int, int> OnEnemyHealthChanged;          // (현재 HP, 최대 HP)
        public event Action OnGameOver;
        public event Action OnStageCleared;                          // 적 HP가 0이 된 응답 종료
        public event Action OnFinalStageCleared;
        public event Action<StageSO> OnStageApplied;
        public event Action<int, int, int> OnEnemyPageTransitionStarted;

        public PhaseSO CurrentPhase { get; private set; }
        public StageSO CurrentStage => stage;
        public int Score { get; private set; }
        public int CurrentEnemyHp { get; private set; }
        public int EnemyMaxHp { get; private set; } = 1;
        public int CurrentEnemyPage { get; private set; } = 1;
        public int EnemyPageCount { get; private set; } = 1;

        private sealed class ResponseNote
        {
            public int slot;
            public int globalBeat;
            public Enemy enemy;
            public double openTime;   // 슬롯 게이지 시작
            public double earlyTime;  // 성공 판정 시작
            public double targetTime; // 정박
            public double closeTime;  // 느림 Offset 끝
            public bool consumed; // 이미 입력으로 확정됐는가
            public bool lockedByEarlyInput;
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
        private bool initialized;
        private bool stageClearPending;
        private int stageStartCycleIndex;
        private int responseEndCycleIndex = -1;
        private GameObject backgroundInstance;

        public int Seed => seed;

        /// <summary>스테이지 지정(StageManager가 Awake 전에 호출 — DefaultExecutionOrder).</summary>
        public void SetStage(StageSO s)
        {
            stage = s;
            if (initialized && stage != null) ApplyStage(stage);
        }

        private void Awake()
        {
            if (randomizeSeed) seed = Environment.TickCount;
            initialized = true;
            if (stage != null) ApplyStage(stage);
            else RebuildStageRuntime();
        }

        /// <summary>스테이지 데이터로 이 매니저·플레이어·입력·비트를 구성한다.</summary>
        private void ApplyStage(StageSO s)
        {
            if (s.enemyPool != null && s.enemyPool.Count > 0) enemyPool = new List<Enemy>(s.enemyPool);
            if (s.pattern != null) pattern = s.pattern;
            if (s.phases != null) phases = new List<PhaseSO>(s.phases);
            cyclesPerPhase = Mathf.Max(1, s.cyclesPerPhase);
            repeatPhasePlan = s.repeatPhasePlan;
            phasePreparationBeats = Mathf.Max(0, s.phasePreparationBeats);
            if (input != null && !keepSceneInputMode) input.Mode = s.keyMode;
            if (conductor != null) conductor.Bpm = s.bpm;
            if (player != null) player.SetMaxHp(s.playerMaxHp);
            if (backgroundInstance != null) Destroy(backgroundInstance);
            backgroundInstance = s.backgroundPrefab != null ? Instantiate(s.backgroundPrefab) : null;
            EnemyMaxHp = Mathf.Max(1, s.enemyMaxHp);
            CurrentEnemyHp = EnemyMaxHp;
            EnemyPageCount = Mathf.Max(1, s.enemyPageCount);
            CurrentEnemyPage = 1;

            stageStartCycleIndex = conductor != null && conductor.IsRunning
                ? conductor.CycleIndex + 1
                : 0;
            stageClearPending = false;
            isOver = false;
            CurrentPhase = null;
            currentCycle.Clear();
            notes.Clear();
            pendingInputs.Clear();
            inResponse = false;
            responseEndPending = false;
            responseEndFrame = -1;
            responseEndCycleIndex = -1;
            inputLockedUntilRealtime = double.NegativeInfinity;
            RebuildStageRuntime();
            OnStageApplied?.Invoke(s);
            OnEnemyHealthChanged?.Invoke(CurrentEnemyHp, EnemyMaxHp);

            if (verboseLog) Debug.Log($"[Round] 스테이지 적용: {s.stageNumber} {s.displayName} (키 {s.keyMode}, 적 {(s.enemyPool != null ? s.enemyPool.Count : 0)})");
        }

        private void RebuildStageRuntime()
        {
            provider = new EnemySequenceProvider(seed, enemyPool);
            spotlightBeats = pattern != null ? pattern.SpotlightBeatIndices() : new List<int>();
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
                double lateOffset = conductor != null
                    ? Math.Min(conductor.LateOffset, beatDuration * 0.45)
                    : 0.0;
                double targetTime = beatEnd - lateOffset;
                double earlyOffset = conductor != null
                    ? Math.Min(conductor.EarlyOffset, targetTime - beatStart)
                    : 0.0;
                notes.Add(new ResponseNote
                {
                    slot = k,
                    globalBeat = globalBeat,
                    enemy = currentCycle[k],
                    openTime = beatStart,
                    earlyTime = targetTime - earlyOffset,
                    targetTime = targetTime,
                    closeTime = beatEnd,
                    consumed = false,
                    lockedByEarlyInput = false,
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
            responseEndCycleIndex = cycleIndex;
            stageClearPending |= CurrentEnemyHp <= 0;
            if (!stageClearPending && StartsPhasePreparation(cycleIndex))
            {
                PhaseSO nextPhase = PhaseForCycle(cycleIndex + 1);
                OnPhasePreparing?.Invoke(cycleIndex + 1, nextPhase);
                conductor?.QueuePreparationBeats(phasePreparationBeats);
            }
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

            Enemy enemy = currentCycle[slot];
            bool hidden = CurrentPhase != null && CurrentPhase.ShouldHidePreview(enemy);
            OnEnemyPreviewed?.Invoke(new EnemyPreviewCue(slot, enemy, hidden));
            if (!hidden) OnEnemyRevealed?.Invoke(slot, enemy);
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
                bool stageCleared = stageClearPending && !isOver;
                responseEndPending = false;
                responseEndFrame = -1;
                stageClearPending = false;
                if (stageCleared)
                    ResolveEnemyHpDepletion(responseEndCycleIndex);
                responseEndCycleIndex = -1;
            }
        }

        private void QueueInput(TimedPlayerAction timedAction)
        {
            if (!isOver && (conductor == null || !conductor.IsPreparing))
                pendingInputs.Enqueue(timedAction);
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

            ResponseNote note = notes[current];
            if (note.lockedByEarlyInput) return;
            if (inputSongTime < note.earlyTime)
            {
                note.lockedByEarlyInput = true;
                ResolveTimingFrame(note, RhythmTimingResult.TooEarly, inputSongTime);
                OnTimingJudged?.Invoke(note.slot, RhythmTimingResult.TooEarly);
                return;
            }

            note.consumed = true;
            input?.NotifyAccepted(timedAction.Action);
            ResolveTimingFrame(note, RhythmTimingResult.Success, inputSongTime);
            OnTimingJudged?.Invoke(note.slot, RhythmTimingResult.Success);
            double timingRange = inputSongTime <= note.targetTime
                ? note.targetTime - note.earlyTime
                : note.closeTime - note.targetTime;
            float responseRatio = timingRange > 0.0
                ? Mathf.Clamp01((float)(Math.Abs(inputSongTime - note.targetTime) / timingRange))
                : 1f;
            ApplyJudge(
                note.slot,
                note.enemy,
                timedAction.Action,
                isMiss: false,
                responseRatio: responseRatio);
        }

        private void RefreshPendingNoteWindows()
        {
            if (conductor == null) return;

            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i].consumed) continue;
                double beatStart = conductor.BeatToTime(notes[i].globalBeat);
                double beatEnd = conductor.BeatToTime(notes[i].globalBeat + 1);
                double beatDuration = beatEnd - beatStart;
                double lateOffset = Math.Min(conductor.LateOffset, beatDuration * 0.45);
                double targetTime = beatEnd - lateOffset;
                double earlyOffset = Math.Min(conductor.EarlyOffset, targetTime - beatStart);
                notes[i].openTime = beatStart;
                notes[i].earlyTime = targetTime - earlyOffset;
                notes[i].targetTime = targetTime;
                notes[i].closeTime = beatEnd;
            }
        }

        private void ExpireElapsedNotes(double now)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                ResponseNote note = notes[i];
                if (note.consumed) continue;
                if (note.lockedByEarlyInput && now >= note.targetTime)
                {
                    note.consumed = true;
                    ApplyJudge(
                        note.slot,
                        note.enemy,
                        PlayerAction.None,
                        isMiss: true,
                        responseRatio: 1f);
                    continue;
                }
                if (now < note.closeTime) continue;

                note.consumed = true;
                ResolveTimingFrame(note, RhythmTimingResult.TooLate, note.closeTime);
                OnTimingJudged?.Invoke(note.slot, RhythmTimingResult.TooLate);
                ApplyJudge(
                    note.slot,
                    note.enemy,
                    PlayerAction.None,
                    isMiss: true,
                    responseRatio: 1f);
                inputLockedUntilRealtime = Math.Max(
                    inputLockedUntilRealtime,
                    Time.realtimeSinceStartupAsDouble + missInputLockDuration);
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
                ResolveTimingFrame(
                    notes[i],
                    RhythmTimingResult.TooLate,
                    notes[i].closeTime);
                OnTimingJudged?.Invoke(notes[i].slot, RhythmTimingResult.TooLate);
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

            if (result.Cleared)
                ReplaceInterruptedFollowUp(slot, enemy);

            // 차징 처리
            if (action == PlayerAction.Attack && player != null) player.ConsumeCharge();
            else if (action == PlayerAction.Charge && player != null)
                player.SetCharged(result.Type != OutcomeType.Punished);

            if (result.PlayerDamage > 0 && player != null) player.TakeDamage(result.PlayerDamage);
            if (result.Cleared) DamageEnemy();
            if (result.Cleared) AwardScore(enemy, action, responseRatio);
            if (action == PlayerAction.Attack && result.Cleared) OnAttackLanded?.Invoke(charged);
            OnJudged?.Invoke(slot, enemy, result);
            if (verboseLog)
                Debug.Log($"[Round] {(isMiss ? "무입력" : "응답")} slot{slot}: {enemy?.DisplayName} + {action}{(charged ? "(강)" : "")} → {result.Type} (dmg {result.PlayerDamage}) HP {(player != null ? player.CurrentHp : -1)}");
        }

        private void AwardScore(Enemy enemy, PlayerAction action, float responseRatio)
        {
            int strengthBonus = enemy != null
                ? enemy.MaxHp * hpScoreWeight + enemy.Armor * armorScoreWeight
                : 0;
            int timingBonus = Mathf.RoundToInt((1f - Mathf.Clamp01(responseRatio)) * maxTimingBonus);
            int totalPoints = clearScore + strengthBonus + timingBonus;
            if (action == PlayerAction.Attack) totalPoints += hitScore;
            QueueScore(totalPoints, true);
        }

        private void ReplaceInterruptedFollowUp(int slot, Enemy enemy)
        {
            Enemy replacement = enemy != null ? enemy.InterruptedFollowUp : null;
            int nextSlot = slot + 1;
            if (replacement == null
                || nextSlot < 0
                || nextSlot >= currentCycle.Count
                || currentCycle[nextSlot] != enemy.ForcedFollowUp)
                return;

            currentCycle[nextSlot] = replacement;
            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i].slot != nextSlot || notes[i].consumed) continue;
                notes[i].enemy = replacement;
                break;
            }

            if (verboseLog)
                Debug.Log($"[Round] slot{nextSlot}: {enemy.ForcedFollowUp?.DisplayName} interrupted -> {replacement.DisplayName}");
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

        /// <summary>응답 슬롯 게이지의 BPM 기반 진행률(슬롯 시작→정박)을 반환한다.</summary>
        public bool TryGetTimingSlotProgress(int slot, out float progress)
        {
            progress = 0f;
            if (isOver || !inResponse || conductor == null) return false;
            RefreshPendingNoteWindows();

            for (int i = 0; i < notes.Count; i++)
            {
                ResponseNote note = notes[i];
                if (note.slot != slot) continue;

                double now = conductor.SongPosition;
                if (now < note.openTime || now >= note.closeTime)
                    return false;

                double duration = note.targetTime - note.openTime;
                progress = duration > 0.0
                    ? Mathf.Clamp01((float)((now - note.openTime) / duration))
                    : 1f;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 판정 프레임 Scale. Early 시작 Min, 정박 1, Late 끝 Max.
        /// </summary>
        public bool TryGetTimingFrameScale(int slot, out float scale)
        {
            return TryGetTimingFrameScale(slot, new Vector2(0f, 1.25f), out scale);
        }

        public bool TryGetTimingFrameScale(
            int slot,
            Vector2 scaleRange,
            out float scale)
        {
            scale = 0f;
            if (isOver || !inResponse || conductor == null) return false;
            RefreshPendingNoteWindows();

            for (int i = 0; i < notes.Count; i++)
            {
                ResponseNote note = notes[i];
                if (note.slot != slot) continue;

                double now = conductor.SongPosition;
                if (now < note.openTime || now >= note.closeTime) return false;
                scale = CalculateTimingFrameScale(note, now, scaleRange);
                return true;
            }
            return false;
        }

        private void ResolveTimingFrame(
            ResponseNote note,
            RhythmTimingResult result,
            double songTime)
        {
            OnTimingFrameResolved?.Invoke(note.slot, result, songTime);
        }

        public bool TryGetTimingFrameScaleAt(
            int slot,
            double songTime,
            Vector2 scaleRange,
            out float scale)
        {
            scale = Mathf.Clamp(scaleRange.x, 0f, 1f);
            for (int i = 0; i < notes.Count; i++)
            {
                ResponseNote note = notes[i];
                if (note.slot != slot) continue;
                scale = CalculateTimingFrameScale(note, songTime, scaleRange);
                return true;
            }
            return false;
        }

        /// <summary>슬롯 포커스가 가장 작아져야 하는 실제 Perfect 시각.</summary>
        public bool TryGetTimingFrameTargetTime(int slot, out double targetTime)
        {
            targetTime = 0.0;
            if (isOver || !inResponse || conductor == null) return false;
            RefreshPendingNoteWindows();

            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i].slot != slot) continue;
                targetTime = notes[i].targetTime;
                return true;
            }
            return false;
        }

        private static float CalculateTimingFrameScale(
            ResponseNote note,
            double songTime,
            Vector2 scaleRange)
        {
            float minScale = Mathf.Clamp(scaleRange.x, 0f, 1f);
            float maxScale = Mathf.Max(1f, scaleRange.y);

            if (songTime <= note.earlyTime) return minScale;
            if (songTime <= note.targetTime)
            {
                double earlyDuration = note.targetTime - note.earlyTime;
                float earlyProgress = earlyDuration > 0.0
                    ? Mathf.Clamp01((float)((songTime - note.earlyTime) / earlyDuration))
                    : 1f;
                return Mathf.Lerp(minScale, 1f, earlyProgress);
            }

            double lateDuration = note.closeTime - note.targetTime;
            float lateProgress = lateDuration > 0.0
                ? Mathf.Clamp01((float)((songTime - note.targetTime) / lateDuration))
                : 1f;
            return Mathf.Lerp(1f, maxScale, lateProgress);
        }

        /// <summary>기존 Cursor 호출 호환용. 새 UI는 <see cref="TryGetTimingSlotProgress"/>를 사용한다.</summary>
        public bool TryGetInputWindowProgress(int slot, out float progress) =>
            TryGetTimingSlotProgress(slot, out progress);

        private PhaseSO PhaseForCycle(int cycleIndex)
        {
            if (phases == null || phases.Count == 0) return null;
            int block = Mathf.Max(1, cyclesPerPhase);
            int localCycle = Mathf.Max(0, cycleIndex - stageStartCycleIndex);
            int idx = localCycle / block;
            idx = repeatPhasePlan ? idx % phases.Count : Mathf.Min(idx, phases.Count - 1);
            return phases[idx];
        }

        private bool StartsPhasePreparation(int cycleIndex)
        {
            if (phasePreparationBeats <= 0 || phases == null || phases.Count < 2)
                return false;

            PhaseSO current = PhaseForCycle(cycleIndex);
            PhaseSO next = PhaseForCycle(cycleIndex + 1);
            return current != next;
        }

        private void DamageEnemy()
        {
            if (CurrentEnemyHp <= 0) return;

            CurrentEnemyHp = Mathf.Max(0, CurrentEnemyHp - 1);
            stageClearPending |= CurrentEnemyHp == 0;
            OnEnemyHealthChanged?.Invoke(CurrentEnemyHp, EnemyMaxHp);
        }

        private void BeginNextEnemyPage(int completedCycleIndex)
        {
            stageClearPending = false;
            CurrentEnemyPage++;
            CurrentEnemyHp = EnemyMaxHp;
            stageStartCycleIndex = Mathf.Max(0, completedCycleIndex + 1);
            CurrentPhase = null;
            currentCycle.Clear();
            notes.Clear();
            pendingInputs.Clear();
            inResponse = false;
            inputLockedUntilRealtime = double.NegativeInfinity;

            int transitionBeats = stage != null
                ? Mathf.Max(0, stage.enemyPageTransitionBeats)
                : 0;
            OnEnemyHealthChanged?.Invoke(CurrentEnemyHp, EnemyMaxHp);
            OnEnemyPageTransitionStarted?.Invoke(
                CurrentEnemyPage,
                EnemyPageCount,
                transitionBeats);
            conductor?.QueuePreparationBeats(transitionBeats);

            if (verboseLog)
                Debug.Log(
                    $"[Round] 보스 페이지 {CurrentEnemyPage}/{EnemyPageCount} 시작 " +
                    $"(HP {CurrentEnemyHp}, 전환 {transitionBeats}비트)");
        }

        private void ResolveEnemyHpDepletion(int completedCycleIndex)
        {
            if (CurrentEnemyPage < EnemyPageCount)
                BeginNextEnemyPage(completedCycleIndex);
            else
                OnStageCleared?.Invoke();
        }

        public void StopAtStageClear()
        {
            if (isOver) return;
            isOver = true;
            pendingInputs.Clear();
            responseEndPending = false;
            if (conductor != null) conductor.StopClock();
            OnFinalStageCleared?.Invoke();
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
