using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace BeatMemories.Tests
{
    public class DayeonBgmSceneTests
    {
        private const string ScenePath =
            "Assets/Scenes/BeatMemories_Dayeon.unity";
        private const string RosterPath =
            "Assets/Data/Stages/StageRoster.asset";
        private const string TimingPath =
            "Assets/Data/Stages/RhythmTimingSettings_Dayeon_BGM.asset";
        private const string CatalogPath =
            "Assets/Data/Stages/StageSoundtrackCatalog_Dayeon_BGM.asset";
        private const string MixerPath =
            "Assets/Audio/Dayeon_BGM/Dayeon_BGM_Mixer.mixer";
        private const string TickPath =
            "Assets/Resource/Sounds/tick_Dayeon_BGM.wav";
        private const string TackPath =
            "Assets/Resource/Sounds/tack_Dayeon_BGM.wav";
        private const string SnarePath =
            "Assets/Audio/Dayeon_BGM/Preparation_Snare_48k.wav";

        private static readonly TrackSpec[] Tracks =
        {
            new TrackSpec(
                "Assets/Audio/Dayeon_BGM/Stage_1_94BPM_Loop.wav",
                "Assets/Data/Stages/Stage_1.asset",
                1,
                94f,
                32,
                980426,
                0.35f,
                AudioClipLoadType.DecompressOnLoad),
            new TrackSpec(
                "Assets/Audio/Dayeon_BGM/Stage_2_97BPM_Loop.wav",
                "Assets/Data/Stages/Stage_2.asset",
                1,
                97f,
                256,
                7600825,
                0.34f,
                AudioClipLoadType.Streaming),
            new TrackSpec(
                "Assets/Audio/Dayeon_BGM/Stage_3_137BPM_Loop.wav",
                "Assets/Data/Stages/Stage_3.asset",
                1,
                137f,
                48,
                1009051,
                0.40f,
                AudioClipLoadType.DecompressOnLoad),
            new TrackSpec(
                "Assets/Audio/Dayeon_BGM/Stage_4_93BPM_Loop.wav",
                "Assets/Data/Stages/Stage_4.asset",
                1,
                93f,
                40,
                1238710,
                0.39f,
                AudioClipLoadType.DecompressOnLoad),
            new TrackSpec(
                "Assets/Audio/Dayeon_BGM/Boss_1_95BPM_Loop.wav",
                "Assets/Data/Stages/Stage_5.asset",
                1,
                95f,
                184,
                5578105,
                0.39f,
                AudioClipLoadType.Streaming),
            new TrackSpec(
                "Assets/Audio/Dayeon_BGM/Boss_2_92BPM_Loop.wav",
                "Assets/Data/Stages/Stage_5.asset",
                2,
                92f,
                228,
                7137391,
                0.35f,
                AudioClipLoadType.Streaming),
        };

        [Test]
        public void RenderedTracksMatchFixedTempoFrameContracts()
        {
            foreach (TrackSpec spec in Tracks)
            {
                AudioClip clip =
                    AssetDatabase.LoadAssetAtPath<AudioClip>(spec.ClipPath);
                Assert.That(clip, Is.Not.Null, spec.ClipPath);
                Assert.That(clip.frequency, Is.EqualTo(48000), spec.ClipPath);
                Assert.That(clip.channels, Is.EqualTo(2), spec.ClipPath);
                Assert.That(
                    clip.samples,
                    Is.EqualTo(spec.Frames),
                    spec.ClipPath);

                double expectedFrames =
                    spec.LoopBeats * 60.0 / spec.Bpm * clip.frequency;
                Assert.That(
                    Math.Abs(clip.samples - expectedFrames),
                    Is.LessThan(0.6),
                    spec.ClipPath);

                for (int beat = 4;
                     beat < spec.LoopBeats;
                     beat += 4)
                {
                    double idealFrame =
                        beat * 60.0 / spec.Bpm * clip.frequency;
                    double renderedFrame =
                        beat / (double)spec.LoopBeats * clip.samples;
                    Assert.That(
                        Math.Abs(renderedFrame - idealFrame),
                        Is.LessThan(0.6),
                        $"{spec.ClipPath}, beat {beat}");
                }

                AssertPcmImporter(spec.ClipPath, spec.LoadType);
                AssertAudibleWithinFirstTenMilliseconds(clip, spec.ClipPath);
                AssertLoopBoundaryIsContinuous(clip, spec.ClipPath);
            }
        }

        [Test]
        public void ClicksAndPreparationSnareAreImmediateFortyEightKhzPcm()
        {
            foreach (string path in new[] { TickPath, TackPath, SnarePath })
            {
                AudioClip clip =
                    AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                Assert.That(clip, Is.Not.Null, path);
                Assert.That(clip.frequency, Is.EqualTo(48000), path);
                Assert.That(
                    FindRelativeAttackTime(clip, 0.1f),
                    Is.LessThan(0.001f),
                    path);
                AssertPcmImporter(
                    path,
                    AudioClipLoadType.DecompressOnLoad);
            }

            AudioClip snare =
                AssetDatabase.LoadAssetAtPath<AudioClip>(SnarePath);
            Assert.That(snare.samples, Is.EqualTo(5760));
        }

        [Test]
        public void CatalogUsesSharedRosterAndKeepsAllSixCueTempos()
        {
            Type catalogType = RuntimeType(
                "BeatMemories.StageSoundtrackCatalogSO");
            Type rosterType = RuntimeType("BeatMemories.StageRosterSO");
            UnityEngine.Object catalog =
                AssetDatabase.LoadAssetAtPath(CatalogPath, catalogType);
            UnityEngine.Object roster =
                AssetDatabase.LoadAssetAtPath(RosterPath, rosterType);

            Assert.That(catalog, Is.Not.Null);
            Assert.That(roster, Is.Not.Null);
            var catalogObject = new SerializedObject(catalog);
            var rosterObject = new SerializedObject(roster);
            SerializedProperty entries =
                catalogObject.FindProperty("entries");
            SerializedProperty stages =
                rosterObject.FindProperty("stages");
            Assert.That(entries.arraySize, Is.EqualTo(Tracks.Length));
            Assert.That(stages.arraySize, Is.EqualTo(5));

            var uniqueStages = new HashSet<UnityEngine.Object>();
            for (int i = 0; i < Tracks.Length; i++)
            {
                TrackSpec spec = Tracks[i];
                UnityEngine.Object stage =
                    AssetDatabase.LoadMainAssetAtPath(spec.StagePath);
                AudioClip clip =
                    AssetDatabase.LoadAssetAtPath<AudioClip>(spec.ClipPath);
                SerializedProperty entry =
                    entries.GetArrayElementAtIndex(i);

                Assert.That(stage, Is.Not.Null, spec.StagePath);
                Assert.That(
                    entry.FindPropertyRelative("stage").objectReferenceValue,
                    Is.SameAs(stage));
                Assert.That(
                    entry.FindPropertyRelative("enemyPage").intValue,
                    Is.EqualTo(spec.EnemyPage));
                Assert.That(
                    entry.FindPropertyRelative("clip").objectReferenceValue,
                    Is.SameAs(clip));
                Assert.That(
                    entry.FindPropertyRelative("bpm").floatValue,
                    Is.EqualTo(spec.Bpm).Within(0.0001f));
                Assert.That(
                    entry.FindPropertyRelative("loopBeats").intValue,
                    Is.EqualTo(spec.LoopBeats));
                Assert.That(
                    entry.FindPropertyRelative("volume").floatValue,
                    Is.EqualTo(spec.Volume).Within(0.0001f));

                if (uniqueStages.Add(stage))
                {
                    int rosterIndex = uniqueStages.Count - 1;
                    Assert.That(
                        stages.GetArrayElementAtIndex(rosterIndex)
                            .objectReferenceValue,
                        Is.SameAs(stage));
                    var stageObject = new SerializedObject(stage);
                    Assert.That(
                        stageObject.FindProperty("bpm").floatValue,
                        Is.EqualTo(90f).Within(0.0001f));
                    Assert.That(
                        stageObject.FindProperty("keyMode").enumValueIndex,
                        Is.EqualTo(1));
                    Assert.That(
                        stageObject.FindProperty("introDialogue")
                            .objectReferenceValue,
                        Is.Not.Null);
                }
            }
        }

        [Test]
        public void ProductionSceneUsesDspAudioGateBounceAndBossPresentation()
        {
            Type roundType = RuntimeType("BeatMemories.RoundManager");
            Type conductorType = RuntimeType("BeatMemories.Conductor");
            Type stageManagerType = RuntimeType("BeatMemories.StageManager");
            Type hudType = RuntimeType("BeatMemories.HudView");
            Type phaseType = RuntimeType(
                "BeatMemories.PhasePresentationController");
            Type audioType = RuntimeType(
                "BeatMemories.RhythmAudioController");
            Type bossType = RuntimeType(
                "BeatMemories.BossPagePresentationController");

            UnityEngine.Object timing =
                AssetDatabase.LoadMainAssetAtPath(TimingPath);
            UnityEngine.Object roster =
                AssetDatabase.LoadMainAssetAtPath(RosterPath);
            UnityEngine.Object catalog =
                AssetDatabase.LoadMainAssetAtPath(CatalogPath);
            UnityEngine.Object firstStage =
                AssetDatabase.LoadMainAssetAtPath(Tracks[0].StagePath);
            AudioClip snare =
                AssetDatabase.LoadAssetAtPath<AudioClip>(SnarePath);

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool closeSceneAfterTest = !scene.IsValid() || !scene.isLoaded;
            if (closeSceneAfterTest)
            {
                scene = EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Additive);
            }
            try
            {
                Component round = FindInScene(scene, roundType);
                Component conductor = FindInScene(scene, conductorType);
                Component stageManager =
                    FindInScene(scene, stageManagerType);
                Component hud = FindInScene(scene, hudType);
                Component phase = FindInScene(scene, phaseType);
                Component audio = FindInScene(scene, audioType);
                Component boss = FindInScene(scene, bossType);

                Assert.That(round, Is.Not.Null);
                Assert.That(conductor, Is.Not.Null);
                Assert.That(stageManager, Is.Not.Null);
                Assert.That(hud, Is.Not.Null);
                Assert.That(phase, Is.Not.Null);
                Assert.That(audio, Is.Not.Null);
                Assert.That(boss, Is.Not.Null);

                var roundObject = new SerializedObject(round);
                Assert.That(
                    roundObject.FindProperty("stage").objectReferenceValue,
                    Is.SameAs(firstStage));
                Assert.That(
                    roundObject.FindProperty("keepSceneInputMode").boolValue,
                    Is.True);

                var conductorObject = new SerializedObject(conductor);
                Assert.That(
                    conductorObject.FindProperty("timingSettings")
                        .objectReferenceValue,
                    Is.SameAs(timing));
                Assert.That(
                    conductorObject.FindProperty("playOnStart").boolValue,
                    Is.False);

                var managerObject = new SerializedObject(stageManager);
                Assert.That(
                    managerObject.FindProperty("roster")
                        .objectReferenceValue,
                    Is.SameAs(roster));
                Assert.That(
                    managerObject.FindProperty("rhythmAudio")
                        .objectReferenceValue,
                    Is.SameAs(audio));
                Assert.That(
                    managerObject.FindProperty("gateClockUntilAudioReady")
                        .boolValue,
                    Is.True);
                Assert.That(
                    managerObject.FindProperty("dialogueViewer")
                        .objectReferenceValue,
                    Is.Not.Null);
                Assert.That(
                    managerObject.FindProperty("stageClearBanner")
                        .objectReferenceValue,
                    Is.Not.Null);
                Assert.That(
                    managerObject.FindProperty("blackout")
                        .objectReferenceValue,
                    Is.Not.Null);

                var audioObject = new SerializedObject(audio);
                Assert.That(
                    audioObject.FindProperty("round").objectReferenceValue,
                    Is.SameAs(round));
                Assert.That(
                    audioObject.FindProperty("conductor")
                        .objectReferenceValue,
                    Is.SameAs(conductor));
                Assert.That(
                    audioObject.FindProperty("catalog")
                        .objectReferenceValue,
                    Is.SameAs(catalog));
                Assert.That(
                    audioObject.FindProperty("preparationSnare")
                        .objectReferenceValue,
                    Is.SameAs(snare));
                Assert.That(
                    audioObject.FindProperty(
                            "preparationSnareVolumeOverride")
                        .floatValue,
                    Is.EqualTo(0.8f).Within(0.0001f));
                Assert.That(
                    audioObject.FindProperty("musicOutput")
                        .objectReferenceValue,
                    Is.Not.Null);
                Assert.That(
                    audioObject.FindProperty("sfxOutput")
                        .objectReferenceValue,
                    Is.Not.Null);

                var phaseObject = new SerializedObject(phase);
                Assert.That(
                    phaseObject.FindProperty("rhythmAudio")
                        .objectReferenceValue,
                    Is.SameAs(audio));
                Assert.That(
                    phaseObject.FindProperty("backgroundMusic")
                        .objectReferenceValue,
                    Is.Null);
                Assert.That(
                    phaseObject.FindProperty(
                            "backgroundMusicFirstBeatOffset")
                        .floatValue,
                    Is.Zero);

                var hudObject = new SerializedObject(hud);
                Assert.That(
                    hudObject.FindProperty("useDspSyncedIdleBounce")
                        .boolValue,
                    Is.True);
                Assert.That(
                    hudObject.FindProperty(
                            "visualBeatOffsetMilliseconds")
                        .floatValue,
                    Is.Zero);

                var bossObject = new SerializedObject(boss);
                Assert.That(
                    bossObject.FindProperty("round").objectReferenceValue,
                    Is.SameAs(round));
                Assert.That(
                    bossObject.FindProperty("conductor")
                        .objectReferenceValue,
                    Is.SameAs(conductor));
                Assert.That(
                    bossObject.FindProperty("stageManager")
                        .objectReferenceValue,
                    Is.SameAs(stageManager));
                Assert.That(
                    bossObject.FindProperty("enemyActor")
                        .objectReferenceValue,
                    Is.Not.Null);
                Assert.That(
                    bossObject.FindProperty("previewContainer")
                        .objectReferenceValue,
                    Is.Not.Null);
                SerializedProperty previewSlots =
                    bossObject.FindProperty("previewSlots");
                Assert.That(previewSlots.arraySize, Is.EqualTo(4));
                for (int i = 0; i < previewSlots.arraySize; i++)
                {
                    Assert.That(
                        previewSlots.GetArrayElementAtIndex(i)
                            .objectReferenceValue,
                        Is.Not.Null);
                }

                Component[] components = scene.GetRootGameObjects()
                    .SelectMany(root =>
                        root.GetComponentsInChildren<Component>(true))
                    .ToArray();
                Assert.That(components, Has.None.Null);
            }
            finally
            {
                if (closeSceneAfterTest)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void CloneMixerHasRequestedHeadroomAndRoutingLevels()
        {
            AudioMixer mixer =
                AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            Assert.That(mixer, Is.Not.Null);

            AssertMixerVolume(mixer, "Master", -3f);
            AssertMixerVolume(mixer, "Music", 0f);
            AssertMixerVolume(mixer, "Metronome", 0f);
            AssertMixerVolume(mixer, "SFX", -9f);

            UnityEngine.Object timing =
                AssetDatabase.LoadMainAssetAtPath(TimingPath);
            var timingObject = new SerializedObject(timing);
            Assert.That(
                timingObject.FindProperty("bpm").floatValue,
                Is.EqualTo(94f));
            Assert.That(
                timingObject.FindProperty("metronomeVolume").floatValue,
                Is.EqualTo(0.65f).Within(0.0001f));
            Assert.That(
                timingObject.FindProperty("metronomeOutput")
                    .objectReferenceValue,
                Is.SameAs(FindMixerGroup(mixer, "Metronome")));
        }

        [Test]
        public void DspIdleBounceHasMaximumSquashExactlyOnTheBeat()
        {
            Type hudType = RuntimeType("BeatMemories.HudView");
            MethodInfo evaluate = hudType.GetMethod(
                "EvaluateIdleBeatScaleRatio",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(evaluate, Is.Not.Null);

            Type easeType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType("DG.Tweening.Ease"))
                .First(type => type != null);
            object outQuad = Enum.Parse(easeType, "OutQuad");
            object[] onBeat = { 0f, 0.98f, 0.25f, outQuad };
            object[] afterRestore = { 0.5f, 0.98f, 0.25f, outQuad };

            Assert.That(
                (float)evaluate.Invoke(null, onBeat),
                Is.EqualTo(0.98f).Within(0.00001f));
            Assert.That(
                (float)evaluate.Invoke(null, afterRestore),
                Is.EqualTo(1f).Within(0.00001f));
        }

        [Test]
        public void BossPageOneDepletionDoesNotStartFinalEnemyDeath()
        {
            Type hudType = RuntimeType("BeatMemories.HudView");
            MethodInfo shouldPlay = hudType.GetMethod(
                "ShouldPlayEnemyDeath",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(shouldPlay, Is.Not.Null);

            Assert.That(
                shouldPlay.Invoke(null, new object[] { 0, 1, 2 }),
                Is.EqualTo(false));
            Assert.That(
                shouldPlay.Invoke(null, new object[] { 0, 2, 2 }),
                Is.EqualTo(true));
            Assert.That(
                shouldPlay.Invoke(null, new object[] { 1, 2, 2 }),
                Is.EqualTo(false));
        }

        [Test]
        public void ProductionSceneIsBuiltAndSharedGameplayDataStaysCurrent()
        {
            UnityEngine.Object sharedStage =
                AssetDatabase.LoadMainAssetAtPath(
                    "Assets/Data/Stages/Stage_1.asset");
            UnityEngine.Object sharedTiming =
                AssetDatabase.LoadMainAssetAtPath(
                    "Assets/Resources/RhythmTimingSettings.asset");
            var stageObject = new SerializedObject(sharedStage);
            var timingObject = new SerializedObject(sharedTiming);

            Assert.That(
                stageObject.FindProperty("bpm").floatValue,
                Is.EqualTo(90f));
            Assert.That(
                timingObject.FindProperty("bpm"),
                Is.Null,
                "BPM belongs to the soundtrack catalog, not timing settings.");

            string productionScene = File.ReadAllText(ScenePath);
            string catalogGuid =
                AssetDatabase.AssetPathToGUID(CatalogPath);
            StringAssert.Contains(catalogGuid, productionScene);
            StringAssert.Contains("m_Name: ChargeButton", productionScene);

            string[] enabledScenePaths = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            CollectionAssert.Contains(enabledScenePaths, ScenePath);
            CollectionAssert.DoesNotContain(
                enabledScenePaths,
                "Assets/Scenes/BeatMemories_Dayeon_BGM.unity");
            Assert.That(
                File.Exists("Assets/Scenes/BeatMemories_Dayeon_BGM.unity"),
                Is.True);
        }

        private static void AssertPcmImporter(
            string path,
            AudioClipLoadType expectedLoadType)
        {
            AudioImporter importer =
                AssetImporter.GetAtPath(path) as AudioImporter;
            Assert.That(importer, Is.Not.Null, path);
            AudioImporterSampleSettings settings =
                importer.defaultSampleSettings;
            Assert.That(
                settings.loadType,
                Is.EqualTo(expectedLoadType),
                path);
            Assert.That(
                settings.compressionFormat,
                Is.EqualTo(AudioCompressionFormat.PCM),
                path);
            Assert.That(settings.preloadAudioData, Is.True, path);
            Assert.That(importer.loadInBackground, Is.False, path);
        }

        private static void AssertAudibleWithinFirstTenMilliseconds(
            AudioClip clip,
            string path)
        {
            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            WaveHeader wave = ReadPcm16WaveHeader(reader, path);
            Assert.That(wave.Frequency, Is.EqualTo(clip.frequency), path);
            Assert.That(wave.Channels, Is.EqualTo(clip.channels), path);
            Assert.That(wave.Frames, Is.EqualTo(clip.samples), path);
            int frames = Math.Min(wave.Frames, wave.Frequency / 100);
            int sampleCount = frames * wave.Channels;
            stream.Position = wave.DataOffset;
            int peak = 0;
            for (int i = 0; i < sampleCount; i++)
                peak = Math.Max(peak, Math.Abs((int)reader.ReadInt16()));

            Assert.That(
                peak,
                Is.GreaterThan(32),
                $"{path} has no audible first-beat attack in its first 10 ms.");
        }

        private static void AssertLoopBoundaryIsContinuous(
            AudioClip clip,
            string path)
        {
            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            WaveHeader wave = ReadPcm16WaveHeader(reader, path);
            int jump = 0;
            for (int channel = 0; channel < wave.Channels; channel++)
            {
                stream.Position =
                    wave.DataOffset + channel * sizeof(short);
                int first = reader.ReadInt16();
                stream.Position =
                    wave.DataOffset
                    + ((long)(wave.Frames - 1) * wave.Channels + channel)
                    * sizeof(short);
                int last = reader.ReadInt16();
                jump = Math.Max(
                    jump,
                    Math.Abs(first - last));
            }

            Assert.That(jump / 32768f, Is.LessThan(0.08f), path);
        }

        private static WaveHeader ReadPcm16WaveHeader(
            BinaryReader reader,
            string path)
        {
            const uint Riff = 0x46464952;
            const uint Wave = 0x45564157;
            const uint Format = 0x20746d66;
            const uint Data = 0x61746164;

            Assert.That(reader.ReadUInt32(), Is.EqualTo(Riff), path);
            reader.ReadUInt32();
            Assert.That(reader.ReadUInt32(), Is.EqualTo(Wave), path);

            ushort audioFormat = 0;
            ushort channels = 0;
            int frequency = 0;
            ushort bitsPerSample = 0;
            long dataOffset = -1;
            int dataBytes = 0;

            while (reader.BaseStream.Position + 8 <= reader.BaseStream.Length)
            {
                uint chunk = reader.ReadUInt32();
                uint chunkBytes = reader.ReadUInt32();
                long nextChunk =
                    reader.BaseStream.Position
                    + chunkBytes
                    + (chunkBytes & 1);

                if (chunk == Format)
                {
                    audioFormat = reader.ReadUInt16();
                    channels = reader.ReadUInt16();
                    frequency = reader.ReadInt32();
                    reader.BaseStream.Position += 6;
                    bitsPerSample = reader.ReadUInt16();
                }
                else if (chunk == Data)
                {
                    dataOffset = reader.BaseStream.Position;
                    dataBytes = checked((int)chunkBytes);
                }

                reader.BaseStream.Position = nextChunk;
            }

            Assert.That(audioFormat, Is.EqualTo(1), path);
            Assert.That(bitsPerSample, Is.EqualTo(16), path);
            Assert.That(channels, Is.GreaterThan(0), path);
            Assert.That(frequency, Is.GreaterThan(0), path);
            Assert.That(dataOffset, Is.GreaterThanOrEqualTo(0), path);
            int frames = dataBytes / (channels * sizeof(short));
            return new WaveHeader(
                channels,
                frequency,
                frames,
                dataOffset);
        }

        private static float FindRelativeAttackTime(
            AudioClip clip,
            float relativeThreshold)
        {
            Assert.That(clip.LoadAudioData(), Is.True);
            var samples = new float[clip.samples * clip.channels];
            Assert.That(clip.GetData(samples, 0), Is.True);

            float peak = samples.Max(sample => Mathf.Abs(sample));
            float threshold = peak * relativeThreshold;
            for (int frame = 0; frame < clip.samples; frame++)
            {
                for (int channel = 0;
                     channel < clip.channels;
                     channel++)
                {
                    if (Mathf.Abs(
                            samples[frame * clip.channels + channel])
                        >= threshold)
                    {
                        return frame / (float)clip.frequency;
                    }
                }
            }

            Assert.Fail($"No attack found in {clip.name}.");
            return -1f;
        }

        private static void AssertMixerVolume(
            AudioMixer mixer,
            string groupName,
            float expectedDecibels)
        {
            AudioMixerGroup group = FindMixerGroup(mixer, groupName);
            Type controllerType = mixer.GetType();
            object snapshot = controllerType
                .GetProperty("TargetSnapshot")
                ?.GetValue(mixer);
            MethodInfo getter = group
                .GetType()
                .GetMethod("GetValueForVolume");
            Assert.That(snapshot, Is.Not.Null, groupName);
            Assert.That(getter, Is.Not.Null, groupName);
            float actual = (float)getter.Invoke(
                group,
                new[] { mixer, snapshot });
            Assert.That(
                actual,
                Is.EqualTo(expectedDecibels).Within(0.001f),
                groupName);
        }

        private static AudioMixerGroup FindMixerGroup(
            AudioMixer mixer,
            string groupName)
        {
            AudioMixerGroup group = mixer
                .FindMatchingGroups(groupName)
                .FirstOrDefault(candidate =>
                    candidate.name == groupName);
            Assert.That(group, Is.Not.Null, groupName);
            return group;
        }

        private static Type RuntimeType(string fullName)
        {
            Assembly runtimeAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .First(assembly => assembly.GetType(fullName) != null);
            return runtimeAssembly.GetType(fullName, true);
        }

        private static Component FindInScene(
            Scene scene,
            Type componentType)
            => scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren(
                        componentType,
                        true))
                .Cast<Component>()
                .FirstOrDefault();

        private readonly struct TrackSpec
        {
            public TrackSpec(
                string clipPath,
                string stagePath,
                int enemyPage,
                float bpm,
                int loopBeats,
                int frames,
                float volume,
                AudioClipLoadType loadType)
            {
                ClipPath = clipPath;
                StagePath = stagePath;
                EnemyPage = enemyPage;
                Bpm = bpm;
                LoopBeats = loopBeats;
                Frames = frames;
                Volume = volume;
                LoadType = loadType;
            }

            public string ClipPath { get; }
            public string StagePath { get; }
            public int EnemyPage { get; }
            public float Bpm { get; }
            public int LoopBeats { get; }
            public int Frames { get; }
            public float Volume { get; }
            public AudioClipLoadType LoadType { get; }
        }

        private readonly struct WaveHeader
        {
            public WaveHeader(
                int channels,
                int frequency,
                int frames,
                long dataOffset)
            {
                Channels = channels;
                Frequency = frequency;
                Frames = frames;
                DataOffset = dataOffset;
            }

            public int Channels { get; }
            public int Frequency { get; }
            public int Frames { get; }
            public long DataOffset { get; }
        }
    }
}
