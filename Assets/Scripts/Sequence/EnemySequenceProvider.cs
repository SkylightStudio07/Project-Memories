using System.Collections.Generic;

namespace BeatMemories
{
    /// <summary>
    /// 적(자세) 시퀀스를 결정론적으로 생성한다.
    /// 반환값은 <c>(seed, cycleIndex, slotIndex)</c>의 순수 함수이므로
    ///  - 재현성: 같은 시드 → 항상 같은 시퀀스
    ///  - 독립성: 호출 순서·다른 시스템의 난수와 무관 (전역 RNG 미사용)
    /// 를 동시에 보장한다. 페이즈 가중 선택도 같은 해시를 쓰므로 결정론적이다.
    /// </summary>
    public sealed class EnemySequenceProvider
    {
        private readonly int seed;
        private readonly IReadOnlyList<Enemy> pool;

        // 정답 액션(자세)별 그룹핑 — 페이즈 가중 선택용. 키 순서는 enum 값 오름차순(안정적).
        private readonly List<PlayerAction> answerKeys = new List<PlayerAction>();
        private readonly Dictionary<PlayerAction, List<Enemy>> byAnswer = new Dictionary<PlayerAction, List<Enemy>>();

        public EnemySequenceProvider(int seed, IReadOnlyList<Enemy> pool)
        {
            this.seed = seed;
            this.pool = pool ?? throw new System.ArgumentNullException(nameof(pool));
            BuildGroups();
        }

        public int Seed => seed;
        public int PoolCount => pool.Count;

        // ── 균등 선택(페이즈 없음) ──────────────────────────────
        /// <summary>지정 사이클/슬롯의 적을 균등 선택(순수 함수). 풀이 비면 null.</summary>
        public Enemy Get(int cycleIndex, int slotIndex)
        {
            if (pool.Count == 0) return null;
            uint h = Hash((uint)seed, (uint)cycleIndex, (uint)slotIndex);
            return pool[(int)(h % (uint)pool.Count)];
        }

        public List<Enemy> GenerateCycle(int cycleIndex, int count)
        {
            int n = count < 0 ? 0 : count;
            var list = new List<Enemy>(n);
            for (int i = 0; i < n; i++) list.Add(Get(cycleIndex, i));
            return list;
        }

        // ── 페이즈 가중 선택 ────────────────────────────────────
        /// <summary>
        /// 페이즈 가중치에 따라 결정론적으로 적을 선택.
        /// 1) (seed,cycle,slot) 해시로 정답 액션(자세)을 가중 추첨,
        /// 2) 그 그룹 안에서 다시 해시로 한 마리 선택.
        /// phase가 null이거나 유효 가중치가 없으면 균등 선택으로 폴백.
        /// </summary>
        public Enemy GetWeighted(int cycleIndex, int slotIndex, PhaseSO phase)
        {
            if (pool.Count == 0) return null;
            if (phase != null && phase.HasEnemyWeights)
                return GetEnemyWeighted(cycleIndex, slotIndex, phase, true);
            if (phase == null || answerKeys.Count == 0) return Get(cycleIndex, slotIndex);

            float total = 0f;
            for (int i = 0; i < answerKeys.Count; i++)
                total += phase.GetWeight(answerKeys[i]); // 빈 그룹은 answerKeys에 애초에 없음

            if (total <= 0f) return Get(cycleIndex, slotIndex);

            float r = Norm(Hash((uint)seed, (uint)cycleIndex, (uint)slotIndex)) * total;
            PlayerAction chosen = answerKeys[0];
            float acc = 0f;
            for (int i = 0; i < answerKeys.Count; i++)
            {
                acc += phase.GetWeight(answerKeys[i]);
                if (r < acc) { chosen = answerKeys[i]; break; }
            }

            var group = byAnswer[chosen];
            uint h2 = Hash((uint)seed ^ 0x9E3779B9u, (uint)cycleIndex, (uint)slotIndex);
            return group[(int)(h2 % (uint)group.Count)];
        }

        public List<Enemy> GenerateCycleWeighted(int cycleIndex, int count, PhaseSO phase)
        {
            int n = count < 0 ? 0 : count;
            var list = new List<Enemy>(n);
            for (int i = 0; i < n; i++)
            {
                bool hasRoomForFollowUp = i + 1 < n;
                Enemy enemy = phase != null && phase.HasEnemyWeights
                    ? GetEnemyWeighted(cycleIndex, i, phase, hasRoomForFollowUp)
                    : GetWeighted(cycleIndex, i, phase);
                list.Add(enemy);

                Enemy followUp = enemy != null ? enemy.ForcedFollowUp : null;
                if (followUp == null || i + 1 >= n) continue;

                list.Add(followUp);
                i++;
            }
            return list;
        }

        private Enemy GetEnemyWeighted(
            int cycleIndex,
            int slotIndex,
            PhaseSO phase,
            bool allowForcedFollowUp)
        {
            IReadOnlyList<EnemyWeight> entries = phase.EnemyWeights;
            float total = 0f;
            for (int i = 0; i < entries.Count; i++)
            {
                EnemyWeight entry = entries[i];
                if (entry == null
                    || entry.enemy == null
                    || (!allowForcedFollowUp && entry.enemy.ForcedFollowUp != null))
                    continue;
                total += System.Math.Max(0f, entry.weight);
            }

            if (total <= 0f) return null;

            float r = Norm(Hash((uint)seed, (uint)cycleIndex, (uint)slotIndex)) * total;
            Enemy fallback = null;
            float accumulated = 0f;
            for (int i = 0; i < entries.Count; i++)
            {
                EnemyWeight entry = entries[i];
                if (entry == null
                    || entry.enemy == null
                    || (!allowForcedFollowUp && entry.enemy.ForcedFollowUp != null)
                    || entry.weight <= 0f)
                    continue;

                fallback = entry.enemy;
                accumulated += entry.weight;
                if (r < accumulated) return entry.enemy;
            }

            return fallback;
        }

        // ── 내부 ────────────────────────────────────────────────
        private void BuildGroups()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                var e = pool[i];
                if (e == null) continue;
                PlayerAction ans = PrimaryAnswer(e);
                if (!byAnswer.TryGetValue(ans, out var list))
                {
                    list = new List<Enemy>();
                    byAnswer[ans] = list;
                }
                list.Add(e);
            }
            // 안정적 키 순서(enum 값 오름차순)
            foreach (var kv in byAnswer)
                if (kv.Value.Count > 0) answerKeys.Add(kv.Key);
            answerKeys.Sort((a, b) => ((int)a).CompareTo((int)b));
        }

        /// <summary>적의 '정답' 액션 = 처리(Cleared)를 만드는 첫 액션. 없으면 None.</summary>
        private static PlayerAction PrimaryAnswer(Enemy e)
        {
            var outcomes = e.Data != null ? e.Data.outcomes : null;
            if (outcomes != null)
                for (int i = 0; i < outcomes.Count; i++)
                    if (outcomes[i] != null && outcomes[i].type == OutcomeType.Cleared)
                        return outcomes[i].action;
            return PlayerAction.None;
        }

        private static float Norm(uint h) => (h & 0xFFFFFFu) / (float)0x1000000; // [0,1)

        // (seed, cycle, slot)을 잘 섞는 순수 해시. FNV-1a + 추가 확산.
        private static uint Hash(uint seed, uint cycle, uint slot)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = (h ^ seed) * 16777619u;
                h = (h ^ cycle) * 16777619u;
                h = (h ^ slot) * 16777619u;
                h ^= h >> 15; h *= 2246822519u;
                h ^= h >> 13; h *= 3266489917u;
                h ^= h >> 16;
                return h;
            }
        }
    }
}
