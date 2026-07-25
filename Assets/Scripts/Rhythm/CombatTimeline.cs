using System;

namespace BeatMemories
{
    public enum CombatSection
    {
        Preparation,
        Preview,
        Response,
    }

    /// <summary>하나의 절대 박이 전투의 어느 구간에 속하는지 나타내는 순수 값.</summary>
    public readonly struct CombatTimelinePosition
    {
        public readonly int TotalBeat;
        public readonly int PhaseIndex;
        public readonly int PhaseBeat;
        public readonly int ExchangeIndex;
        public readonly int ExchangeInPhase;
        public readonly int BeatInCycle;
        public readonly int BeatInMeasure;
        public readonly CombatSection Section;

        public CombatTimelinePosition(
            int totalBeat,
            int phaseIndex,
            int phaseBeat,
            int exchangeIndex,
            int exchangeInPhase,
            int beatInCycle,
            int beatInMeasure,
            CombatSection section)
        {
            TotalBeat = totalBeat;
            PhaseIndex = phaseIndex;
            PhaseBeat = phaseBeat;
            ExchangeIndex = exchangeIndex;
            ExchangeInPhase = exchangeInPhase;
            BeatInCycle = beatInCycle;
            BeatInMeasure = beatInMeasure;
            Section = section;
        }

        public bool IsPreparation => Section == CombatSection.Preparation;
        public bool IsPreview => Section == CombatSection.Preview;
        public bool IsResponse => Section == CombatSection.Response;
    }

    /// <summary>
    /// 준비 4박 뒤에 제시 4박→응답 4박 교환을 반복하는 전투 타임라인.
    /// Unity 상태를 사용하지 않는 순수 계산이라 경계 박을 독립적으로 검증할 수 있다.
    /// </summary>
    public static class CombatTimeline
    {
        public const int BeatsPerMeasure = 4;
        public const int BeatsPerExchange = BeatsPerMeasure * 2;
        public const int DefaultPreparationBeats = BeatsPerMeasure;
        public const int DefaultExchangesPerPhase = 2;

        public static int BeatsPerPhase(int exchangesPerPhase, int preparationBeats = DefaultPreparationBeats)
        {
            Validate(exchangesPerPhase, preparationBeats);
            return preparationBeats + exchangesPerPhase * BeatsPerExchange;
        }

        public static CombatTimelinePosition Resolve(
            int totalBeat,
            int exchangesPerPhase = DefaultExchangesPerPhase,
            int preparationBeats = DefaultPreparationBeats)
        {
            if (totalBeat < 0) throw new ArgumentOutOfRangeException(nameof(totalBeat));
            Validate(exchangesPerPhase, preparationBeats);

            int beatsPerPhase = BeatsPerPhase(exchangesPerPhase, preparationBeats);
            int phaseIndex = totalBeat / beatsPerPhase;
            int phaseBeat = totalBeat % beatsPerPhase;

            if (phaseBeat < preparationBeats)
            {
                return new CombatTimelinePosition(
                    totalBeat,
                    phaseIndex,
                    phaseBeat,
                    -1,
                    -1,
                    -1,
                    phaseBeat,
                    CombatSection.Preparation);
            }

            int activeBeat = phaseBeat - preparationBeats;
            int exchangeInPhase = activeBeat / BeatsPerExchange;
            int beatInCycle = activeBeat % BeatsPerExchange;
            int exchangeIndex = phaseIndex * exchangesPerPhase + exchangeInPhase;
            CombatSection section = beatInCycle < BeatsPerMeasure
                ? CombatSection.Preview
                : CombatSection.Response;

            return new CombatTimelinePosition(
                totalBeat,
                phaseIndex,
                phaseBeat,
                exchangeIndex,
                exchangeInPhase,
                beatInCycle,
                beatInCycle % BeatsPerMeasure,
                section);
        }

        /// <summary>이 박을 보내기 전에 직전 응답 마디를 마감해야 하는가.</summary>
        public static bool StartsAfterResponse(
            int totalBeat,
            int exchangesPerPhase = DefaultExchangesPerPhase,
            int preparationBeats = DefaultPreparationBeats)
        {
            if (totalBeat <= 0) return false;
            CombatTimelinePosition previous = Resolve(totalBeat - 1, exchangesPerPhase, preparationBeats);
            return previous.IsResponse && previous.BeatInMeasure == BeatsPerMeasure - 1;
        }

        private static void Validate(int exchangesPerPhase, int preparationBeats)
        {
            if (exchangesPerPhase < 1)
                throw new ArgumentOutOfRangeException(nameof(exchangesPerPhase));
            if (preparationBeats < 1)
                throw new ArgumentOutOfRangeException(nameof(preparationBeats));
        }
    }
}
