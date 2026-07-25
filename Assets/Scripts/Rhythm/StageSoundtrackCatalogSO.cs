using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// Maps a stage and enemy page to the fixed-tempo audio loop used by the
    /// DSP rhythm clock.
    /// </summary>
    [CreateAssetMenu(
        fileName = "StageSoundtrackCatalog",
        menuName = "Beat Memories/Stage Soundtrack Catalog",
        order = 5)]
    public sealed class StageSoundtrackCatalogSO : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            [SerializeField] private StageSO stage;
            [SerializeField, Min(1)] private int enemyPage = 1;
            [SerializeField] private AudioClip clip;
            [SerializeField, Min(1f)] private float bpm = 96f;
            [SerializeField, Min(1)] private int loopBeats = 32;
            [SerializeField, Range(0f, 1f)] private float volume = 0.35f;

            public StageSO Stage => stage;
            public int EnemyPage => Mathf.Max(1, enemyPage);
            public AudioClip Clip => clip;
            public float Bpm => Mathf.Max(1f, bpm);
            public int LoopBeats => Mathf.Max(1, loopBeats);
            public float Volume => Mathf.Clamp01(volume);

            public bool Matches(StageSO candidateStage, int candidatePage)
                => stage == candidateStage
                   && EnemyPage == Mathf.Max(1, candidatePage);
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => entries;

        public bool TryGetCue(StageSO stage, int enemyPage, out Entry cue)
        {
            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    Entry candidate = entries[i];
                    if (candidate == null || !candidate.Matches(stage, enemyPage))
                        continue;

                    cue = candidate;
                    return true;
                }
            }

            cue = null;
            return false;
        }
    }
}
