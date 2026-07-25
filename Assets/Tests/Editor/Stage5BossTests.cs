using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BeatMemories.Tests
{
    public class Stage5BossTests
    {
        private readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
        private Assembly runtimeAssembly;
        private Type roundType;
        private Type stageType;

        [SetUp]
        public void SetUp()
        {
            runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(assembly => assembly.GetType("BeatMemories.RoundManager") != null);
            roundType = runtimeAssembly.GetType("BeatMemories.RoundManager", true);
            stageType = runtimeAssembly.GetType("BeatMemories.StageSO", true);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = created.Count - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(created[i]);
            created.Clear();
        }

        [Test]
        public void CumulativeStagesUseRequestedEnemyWeights()
        {
            AssertEnemyWeights(
                "Assets/Data/Phases/Phase_Basic_Intro.asset",
                ("Assets/Data/Enemies/Enemy_aggressive.asset", 2f),
                ("Assets/Data/Enemies/Enemy_defenseless.asset", 1f));
            AssertEnemyWeights(
                "Assets/Data/Phases/Phase_Stage2_Defense.asset",
                ("Assets/Data/Enemies/Stage 2 Attack.asset", 2f),
                ("Assets/Data/Enemies/Stage 2 Guard.asset", 2f),
                ("Assets/Data/Enemies/Stage 2 Idle.asset", 1f));
            AssertEnemyWeights(
                "Assets/Data/Phases/Phase_Stage3_Charge.asset",
                ("Assets/Data/Enemies/Stage 3 Attack.asset", 2f),
                ("Assets/Data/Enemies/Stage 3 Guard.asset", 2f),
                ("Assets/Data/Enemies/Stage 3 Charge.asset", 1f),
                ("Assets/Data/Enemies/Stage 3 Idle.asset", 1f));
            AssertEnemyWeights(
                "Assets/Data/Phases/Phase_Stage5_Visible.asset",
                ("Assets/Data/Enemies/Stage 5 Attack.asset", 2f),
                ("Assets/Data/Enemies/Stage 5 Guard.asset", 2f),
                ("Assets/Data/Enemies/Stage 5 Charge.asset", 1f),
                ("Assets/Data/Enemies/Stage 5 Idle.asset", 1f));
            AssertEnemyWeights(
                "Assets/Data/Phases/Phase_Stage5_HiddenAttack.asset",
                ("Assets/Data/Enemies/Stage 5 Attack.asset", 2f),
                ("Assets/Data/Enemies/Stage 5 Guard.asset", 2f),
                ("Assets/Data/Enemies/Stage 5 Charge.asset", 1f),
                ("Assets/Data/Enemies/Stage 5 Idle.asset", 1f));
        }

        [Test]
        public void StageFiveUsesTwoPageBossRulesAndStageTwoMap()
        {
            UnityEngine.Object stage = Load("Assets/Data/Stages/Stage_5.asset");
            var serialized = new SerializedObject(stage);

            Assert.That(serialized.FindProperty("stageNumber").intValue, Is.EqualTo(5));
            Assert.That(serialized.FindProperty("keyMode").enumValueIndex, Is.EqualTo(1));
            Assert.That(serialized.FindProperty("cyclesPerPhase").intValue, Is.EqualTo(2));
            Assert.That(serialized.FindProperty("phasePreparationBeats").intValue, Is.EqualTo(4));
            Assert.That(serialized.FindProperty("repeatPhasePlan").boolValue, Is.False);
            Assert.That(serialized.FindProperty("bpm").floatValue, Is.EqualTo(90f));
            Assert.That(serialized.FindProperty("playerMaxHp").intValue, Is.EqualTo(7));
            Assert.That(serialized.FindProperty("enemyMaxHp").intValue, Is.EqualTo(8));
            Assert.That(serialized.FindProperty("enemyPageCount").intValue, Is.EqualTo(2));
            Assert.That(serialized.FindProperty("enemyPageTransitionBeats").intValue, Is.EqualTo(4));
            Assert.That(
                serialized.FindProperty("cutPreviewBottomHalfOnSecondPage").boolValue,
                Is.True);

            AssertAssetPath(
                serialized.FindProperty("enemySprite").objectReferenceValue,
                "Assets/Resource/Art/Character/Stage5/Stage5_Idle_Cutout.png");
            AssertAssetPath(
                serialized.FindProperty("enemyPageTransitionSprite").objectReferenceValue,
                "Assets/Resource/Art/Character/Stage5/Stage5_Attack_Cutout.png");
            AssertAssetPath(
                serialized.FindProperty("backgroundSprite").objectReferenceValue,
                "Assets/Resource/Art/Background/Stage2_AbandonedFactory_BackLayer_v1.png");
            AssertAssetPath(
                serialized.FindProperty("floorSprite").objectReferenceValue,
                "Assets/Resource/Art/Background/Stage2_AbandonedFactory_FloorLayer_v1.png");
            AssertAssetPath(
                serialized.FindProperty("backgroundPrefab").objectReferenceValue,
                "Assets/Data/Stages/환경요소_액트3.prefab");

            SerializedProperty phases = serialized.FindProperty("phases");
            Assert.That(phases.arraySize, Is.EqualTo(2));
            AssertAssetPath(
                phases.GetArrayElementAtIndex(0).objectReferenceValue,
                "Assets/Data/Phases/Phase_Stage5_Visible.asset");
            AssertAssetPath(
                phases.GetArrayElementAtIndex(1).objectReferenceValue,
                "Assets/Data/Phases/Phase_Stage5_HiddenAttack.asset");

            SerializedProperty roster = new SerializedObject(
                    Load("Assets/Data/Stages/StageRoster.asset"))
                .FindProperty("stages");
            Assert.That(roster.arraySize, Is.EqualTo(5));
            AssertAssetPath(
                roster.GetArrayElementAtIndex(4).objectReferenceValue,
                "Assets/Data/Stages/Stage_5.asset");
        }

        [TestCase(
            "Assets/Data/Stages/Stage_3.asset",
            "Assets/Data/Phases/Phase_Stage3_Charge.asset")]
        [TestCase(
            "Assets/Data/Stages/Stage_5.asset",
            "Assets/Data/Phases/Phase_Stage5_Visible.asset")]
        public void WeightedSequencesKeepChargeFollowUpsAdjacent(
            string stagePath,
            string phasePath)
        {
            Type enemyType = runtimeAssembly.GetType("BeatMemories.Enemy", true);
            Type providerType = runtimeAssembly.GetType(
                "BeatMemories.EnemySequenceProvider",
                true);
            SerializedProperty poolProperty = new SerializedObject(Load(stagePath))
                .FindProperty("enemyPool");
            Array pool = Array.CreateInstance(enemyType, poolProperty.arraySize);
            for (int i = 0; i < poolProperty.arraySize; i++)
                pool.SetValue(
                    poolProperty.GetArrayElementAtIndex(i).objectReferenceValue,
                    i);

            UnityEngine.Object phase = Load(phasePath);
            object firstProvider = Activator.CreateInstance(
                providerType,
                new object[] { 12345, pool });
            object secondProvider = Activator.CreateInstance(
                providerType,
                new object[] { 12345, pool });
            MethodInfo generate = providerType.GetMethod("GenerateCycleWeighted");

            for (int cycleIndex = 0; cycleIndex < 100; cycleIndex++)
            {
                IList<object> first = ((System.Collections.IList)generate.Invoke(
                        firstProvider,
                        new object[] { cycleIndex, 4, phase }))
                    .Cast<object>()
                    .ToList();
                IList<object> second = ((System.Collections.IList)generate.Invoke(
                        secondProvider,
                        new object[] { cycleIndex, 4, phase }))
                    .Cast<object>()
                    .ToList();

                Assert.That(first, Is.EqualTo(second), $"cycle {cycleIndex}");
                for (int slot = 0; slot < first.Count; slot++)
                {
                    object forced = enemyType.GetProperty("ForcedFollowUp")
                        .GetValue(first[slot]);
                    if (forced == null) continue;
                    Assert.That(slot, Is.LessThan(first.Count - 1));
                    Assert.That(first[slot + 1], Is.SameAs(forced));
                }
                Assert.That(
                    enemyType.GetProperty("ForcedFollowUp")
                        .GetValue(first[first.Count - 1]),
                    Is.Null,
                    "마지막 슬롯에는 후속 행동이 필요한 충전이 나오면 안 된다.");
            }
        }

        [Test]
        public void StageFiveEnemiesReuseGuardChargeAndUnblockableRules()
        {
            Type judgeType = runtimeAssembly.GetType("BeatMemories.JudgeSystem", true);
            Type actionType = runtimeAssembly.GetType("BeatMemories.PlayerAction", true);
            MethodInfo judge = judgeType.GetMethod("Judge");
            UnityEngine.Object guard = Load("Assets/Data/Enemies/Stage 5 Guard.asset");
            UnityEngine.Object charge = Load("Assets/Data/Enemies/Stage 5 Charge.asset");
            UnityEngine.Object chargedAttack = Load(
                "Assets/Data/Enemies/Stage 5 Charged Attack.asset");

            object normalVsGuard = judge.Invoke(
                null,
                new[] { guard, Enum.ToObject(actionType, 2), (object)false });
            object chargedVsGuard = judge.Invoke(
                null,
                new[] { guard, Enum.ToObject(actionType, 2), (object)true });
            object cancelCharge = judge.Invoke(
                null,
                new[] { charge, Enum.ToObject(actionType, 2), (object)false });

            Assert.That(ResultField<bool>(normalVsGuard, "Cleared"), Is.False);
            Assert.That(ResultField<bool>(chargedVsGuard, "Cleared"), Is.True);
            Assert.That(ResultField<bool>(cancelCharge, "Cleared"), Is.True);

            foreach (int input in new[] { 0, 1, 2, 4 })
            {
                object result = judge.Invoke(
                    null,
                    new[] { chargedAttack, Enum.ToObject(actionType, input), (object)false });
                Assert.That(ResultField<bool>(result, "Cleared"), Is.False);
                Assert.That(ResultField<int>(result, "PlayerDamage"), Is.EqualTo(2));
            }
        }

        [Test]
        public void FirstHpDepletionRestoresBossAndSecondDepletionClears()
        {
            var roundObject = new GameObject("Stage 5 Boss Test");
            created.Add(roundObject);
            Component round = roundObject.AddComponent(roundType);
            PrivateMethod(roundType, "Awake").Invoke(round, null);

            ScriptableObject stage = ScriptableObject.CreateInstance(stageType);
            created.Add(stage);
            stageType.GetField("enemyMaxHp").SetValue(stage, 8);
            stageType.GetField("enemyPageCount").SetValue(stage, 2);
            stageType.GetField("enemyPageTransitionBeats").SetValue(stage, 4);
            roundType.GetMethod("SetStage").Invoke(round, new object[] { stage });

            int pageEvent = 0;
            int transitionBeats = -1;
            int clearCount = 0;
            EventInfo pageTransition = roundType.GetEvent("OnEnemyPageTransitionStarted");
            Action<int, int, int> pageHandler = (page, count, beats) =>
            {
                pageEvent = page;
                transitionBeats = beats;
            };
            pageTransition.AddEventHandler(round, pageHandler);
            EventInfo stageClear = roundType.GetEvent("OnStageCleared");
            Action clearHandler = () => clearCount++;
            stageClear.AddEventHandler(round, clearHandler);

            MethodInfo damage = PrivateMethod(roundType, "DamageEnemy");
            MethodInfo resolve = PrivateMethod(roundType, "ResolveEnemyHpDepletion");
            for (int i = 0; i < 8; i++) damage.Invoke(round, null);
            resolve.Invoke(round, new object[] { 3 });

            Assert.That(PublicInt(round, "CurrentEnemyPage"), Is.EqualTo(2));
            Assert.That(PublicInt(round, "CurrentEnemyHp"), Is.EqualTo(8));
            Assert.That(pageEvent, Is.EqualTo(2));
            Assert.That(transitionBeats, Is.EqualTo(4));
            Assert.That(clearCount, Is.Zero);

            for (int i = 0; i < 8; i++) damage.Invoke(round, null);
            resolve.Invoke(round, new object[] { 7 });
            Assert.That(PublicInt(round, "CurrentEnemyHp"), Is.Zero);
            Assert.That(clearCount, Is.EqualTo(1));

            pageTransition.RemoveEventHandler(round, pageHandler);
            stageClear.RemoveEventHandler(round, clearHandler);
        }

        [Test]
        public void PagePreparationQueuedAfterResponseEndStartsBeforeNextCycle()
        {
            Type conductorType = runtimeAssembly.GetType("BeatMemories.Conductor", true);
            var conductorObject = new GameObject("Late Preparation Test");
            created.Add(conductorObject);
            Component conductor = conductorObject.AddComponent(conductorType);

            SetPrivate(conductorType, conductor, "pendingBeatDispatch", true);
            conductorType.GetField(
                    "<IsRunning>k__BackingField",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(conductor, true);
            conductorType.GetField(
                    "<TotalBeats>k__BackingField",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(conductor, 8);
            conductorType.GetMethod("QueuePreparationBeats")
                .Invoke(conductor, new object[] { 4 });
            PrivateMethod(conductorType, "Update").Invoke(conductor, null);

            Assert.That(
                (bool)conductorType.GetProperty("IsPreparing").GetValue(conductor),
                Is.True);
            Assert.That(
                (bool)conductorType.GetField(
                        "pendingBeatDispatch",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(conductor),
                Is.False);
        }

        [Test]
        public void SecondPageCutsVisibleSlotsToBottomHalfAndRestoresHiddenSlots()
        {
            Type controllerType = runtimeAssembly.GetType(
                "BeatMemories.BossPagePresentationController",
                true);
            var controllerObject = new GameObject("Boss Preview Cut");
            created.Add(controllerObject);
            Component controller = controllerObject.AddComponent(controllerType);

            var slots = new Image[4];
            for (int i = 0; i < slots.Length; i++)
            {
                var slotObject = new GameObject($"Slot {i}", typeof(RectTransform), typeof(Image));
                created.Add(slotObject);
                slots[i] = slotObject.GetComponent<Image>();
                slots[i].type = Image.Type.Simple;
                slots[i].fillAmount = 1f;
            }

            SetPrivate(controllerType, controller, "previewSlots", slots);
            SetPrivate(controllerType, controller, "originalsCaptured", false);
            PrivateMethod(controllerType, "CaptureOriginalSlotState").Invoke(controller, null);
            PrivateMethod(controllerType, "SetPreviewCutActive").Invoke(
                controller,
                new object[] { true });

            foreach (Image slot in slots)
            {
                Assert.That(slot.type, Is.EqualTo(Image.Type.Filled));
                Assert.That(slot.fillMethod, Is.EqualTo(Image.FillMethod.Vertical));
                Assert.That(slot.fillOrigin, Is.EqualTo((int)Image.OriginVertical.Bottom));
                Assert.That(slot.fillAmount, Is.EqualTo(0.5f));
            }

            Type cueType = runtimeAssembly.GetType("BeatMemories.EnemyPreviewCue", true);
            object hiddenCue = Activator.CreateInstance(cueType, new object[] { 1, null, true });
            PrivateMethod(controllerType, "OnEnemyPreviewed").Invoke(
                controller,
                new[] { hiddenCue });
            Assert.That(slots[1].type, Is.EqualTo(Image.Type.Simple));
            Assert.That(slots[1].fillAmount, Is.EqualTo(1f));
            Assert.That(slots[0].fillAmount, Is.EqualTo(0.5f));

            ScriptableObject nextStage = ScriptableObject.CreateInstance(stageType);
            created.Add(nextStage);
            PrivateMethod(controllerType, "OnStageApplied").Invoke(
                controller,
                new object[] { nextStage });
            foreach (Image slot in slots)
            {
                Assert.That(slot.type, Is.EqualTo(Image.Type.Simple));
                Assert.That(slot.fillAmount, Is.EqualTo(1f));
            }
        }

        private static void AssertEnemyWeights(
            string phasePath,
            params (string path, float weight)[] expected)
        {
            var serialized = new SerializedObject(Load(phasePath));
            SerializedProperty weights = serialized.FindProperty("enemyWeights");
            Assert.That(weights.arraySize, Is.EqualTo(expected.Length), phasePath);
            for (int i = 0; i < expected.Length; i++)
            {
                SerializedProperty entry = weights.GetArrayElementAtIndex(i);
                AssertAssetPath(
                    entry.FindPropertyRelative("enemy").objectReferenceValue,
                    expected[i].path);
                Assert.That(
                    entry.FindPropertyRelative("weight").floatValue,
                    Is.EqualTo(expected[i].weight),
                    expected[i].path);
            }
        }

        private static UnityEngine.Object Load(string path)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            Assert.That(asset, Is.Not.Null, path);
            return asset;
        }

        private static void AssertAssetPath(UnityEngine.Object asset, string expected)
            => Assert.That(AssetDatabase.GetAssetPath(asset), Is.EqualTo(expected));

        private static MethodInfo PrivateMethod(Type type, string name)
            => type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);

        private static void SetPrivate(Type type, object target, string name, object value)
            => type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);

        private static T ResultField<T>(object result, string name)
            => (T)result.GetType().GetField(name).GetValue(result);

        private int PublicInt(Component target, string name)
            => (int)roundType.GetProperty(name).GetValue(target);
    }
}
