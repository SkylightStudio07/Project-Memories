using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace BeatMemories.Tests
{
    public class Stage4CombatTests
    {
        private Assembly runtimeAssembly;
        private Type enemyType;
        private Type playerActionType;
        private Type judgeSystemType;
        private Type providerType;

        [SetUp]
        public void SetUp()
        {
            runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(assembly => assembly.GetType("BeatMemories.Enemy") != null);
            enemyType = RuntimeType("BeatMemories.Enemy");
            playerActionType = RuntimeType("BeatMemories.PlayerAction");
            judgeSystemType = RuntimeType("BeatMemories.JudgeSystem");
            providerType = RuntimeType("BeatMemories.EnemySequenceProvider");
        }

        [Test]
        public void StageFourUsesRequestedRulesAndLaboratoryAssets()
        {
            UnityEngine.Object stage = Load("Assets/Data/Stages/Stage_4.asset");
            var serialized = new SerializedObject(stage);

            Assert.That(serialized.FindProperty("stageNumber").intValue, Is.EqualTo(4));
            Assert.That(serialized.FindProperty("keyMode").enumValueIndex, Is.EqualTo(1));
            Assert.That(serialized.FindProperty("cyclesPerPhase").intValue, Is.EqualTo(2));
            Assert.That(serialized.FindProperty("phasePreparationBeats").intValue, Is.EqualTo(4));
            Assert.That(serialized.FindProperty("repeatPhasePlan").boolValue, Is.False);
            Assert.That(serialized.FindProperty("bpm").floatValue, Is.EqualTo(90f));
            Assert.That(serialized.FindProperty("playerMaxHp").intValue, Is.EqualTo(7));

            AssertAssetPath(
                serialized.FindProperty("enemySprite").objectReferenceValue,
                "Assets/Resource/Art/Character/nai-32e0958a-5be4-4b5e-a686-7772e715a348.png");
            AssertAssetPath(
                serialized.FindProperty("backgroundSprite").objectReferenceValue,
                "Assets/Resource/Art/Background/Stage4_ChemBaronLab_BackLayer_v1.png");
            AssertAssetPath(
                serialized.FindProperty("floorSprite").objectReferenceValue,
                "Assets/Resource/Art/Background/Stage4_ChemBaronLab_FloorLayer_v1.png");
            AssertAssetPath(
                serialized.FindProperty("backgroundPrefab").objectReferenceValue,
                "Assets/Data/Stages/환경요소_5.prefab");

            SerializedProperty phases = serialized.FindProperty("phases");
            Assert.That(phases.arraySize, Is.EqualTo(2));
            AssertAssetPath(
                phases.GetArrayElementAtIndex(0).objectReferenceValue,
                "Assets/Data/Phases/Phase_Stage4_Visible.asset");
            AssertAssetPath(
                phases.GetArrayElementAtIndex(1).objectReferenceValue,
                "Assets/Data/Phases/Phase_Stage4_HiddenAttack.asset");

            SerializedProperty roster = new SerializedObject(
                    Load("Assets/Data/Stages/StageRoster.asset"))
                .FindProperty("stages");
            Assert.That(roster.arraySize, Is.EqualTo(4));
            AssertAssetPath(
                roster.GetArrayElementAtIndex(3).objectReferenceValue,
                "Assets/Data/Stages/Stage_4.asset");
        }

        [Test]
        public void BothStageFourPhasesUseTwoTwoOneOneWeights()
        {
            AssertWeights("Assets/Data/Phases/Phase_Stage4_Visible.asset");
            AssertWeights("Assets/Data/Phases/Phase_Stage4_HiddenAttack.asset");
        }

        [Test]
        public void GeneratedCyclesKeepEveryChargeNextToItsChargedAttack()
        {
            UnityEngine.Object stage = Load("Assets/Data/Stages/Stage_4.asset");
            SerializedProperty poolProperty = new SerializedObject(stage).FindProperty("enemyPool");
            Array pool = Array.CreateInstance(enemyType, poolProperty.arraySize);
            for (int i = 0; i < poolProperty.arraySize; i++)
                pool.SetValue(poolProperty.GetArrayElementAtIndex(i).objectReferenceValue, i);

            UnityEngine.Object phase = Load("Assets/Data/Phases/Phase_Stage4_Visible.asset");
            for (int seed = 0; seed < 100; seed++)
            {
                object provider = Activator.CreateInstance(providerType, new object[] { seed, pool });
                for (int cycleIndex = 0; cycleIndex < 8; cycleIndex++)
                {
                    IList cycle = (IList)providerType.GetMethod("GenerateCycleWeighted")
                        .Invoke(provider, new object[] { cycleIndex, 4, phase });
                    Assert.That(cycle.Count, Is.EqualTo(4));

                    for (int slot = 0; slot < cycle.Count; slot++)
                    {
                        UnityEngine.Object enemy = (UnityEngine.Object)cycle[slot];
                        string id = (string)enemyType.GetProperty("Id").GetValue(enemy);
                        Assert.That(id, Is.Not.EqualTo("stage4_charged_attack"));
                        if (id != "stage4_charge") continue;

                        Assert.That(slot, Is.LessThan(cycle.Count - 1));
                        UnityEngine.Object followUp = (UnityEngine.Object)cycle[slot + 1];
                        Assert.That(
                            (string)enemyType.GetProperty("Id").GetValue(followUp),
                            Is.EqualTo("stage4_charged_attack"));
                        slot++;
                    }
                }
            }
        }

        [Test]
        public void StageFourCombatRulesMatchPreviousMechanics()
        {
            UnityEngine.Object attack = Load("Assets/Data/Enemies/Stage 4 Attack.asset");
            UnityEngine.Object guard = Load("Assets/Data/Enemies/Stage 4 Guard.asset");
            UnityEngine.Object charge = Load("Assets/Data/Enemies/Stage 4 Charge.asset");
            UnityEngine.Object chargedAttack =
                Load("Assets/Data/Enemies/Stage 4 Charged Attack.asset");
            UnityEngine.Object idle = Load("Assets/Data/Enemies/Stage 4 Idle.asset");

            Assert.That(ResultField<bool>(Judge(attack, 1), "Cleared"), Is.False);
            Assert.That(ResultField<int>(Judge(attack, 1), "PlayerDamage"), Is.Zero);
            Assert.That(ResultField<bool>(Judge(guard, 2), "Cleared"), Is.False);
            Assert.That(ResultField<bool>(Judge(guard, 2, true), "Cleared"), Is.True);
            Assert.That(ResultField<bool>(Judge(charge, 2), "Cleared"), Is.True);
            Assert.That(ResultField<bool>(Judge(idle, 2), "Cleared"), Is.True);
            Assert.That(ResultField<int>(Judge(idle, 1), "PlayerDamage"), Is.Zero);

            foreach (int input in new[] { 0, 1, 2, 4 })
            {
                object result = Judge(chargedAttack, input, true);
                Assert.That(ResultField<bool>(result, "Cleared"), Is.False);
                Assert.That(ResultField<int>(result, "PlayerDamage"), Is.EqualTo(2));
            }
        }

        [Test]
        public void HiddenPhaseOnlyHidesAttackActionsAndSanitizesCue()
        {
            UnityEngine.Object phase =
                Load("Assets/Data/Phases/Phase_Stage4_HiddenAttack.asset");
            var serializedPhase = new SerializedObject(phase);
            Assert.That(
                serializedPhase.FindProperty("activeTintStrength").floatValue,
                Is.EqualTo(0.85f));
            MethodInfo shouldHide = phase.GetType().GetMethod("ShouldHidePreview");
            UnityEngine.Object attack = Load("Assets/Data/Enemies/Stage 4 Attack.asset");
            UnityEngine.Object chargedAttack =
                Load("Assets/Data/Enemies/Stage 4 Charged Attack.asset");
            UnityEngine.Object guard = Load("Assets/Data/Enemies/Stage 4 Guard.asset");
            UnityEngine.Object charge = Load("Assets/Data/Enemies/Stage 4 Charge.asset");
            UnityEngine.Object idle = Load("Assets/Data/Enemies/Stage 4 Idle.asset");

            Assert.That((bool)shouldHide.Invoke(phase, new[] { attack }), Is.True);
            Assert.That((bool)shouldHide.Invoke(phase, new[] { chargedAttack }), Is.True);
            Assert.That((bool)shouldHide.Invoke(phase, new[] { guard }), Is.False);
            Assert.That((bool)shouldHide.Invoke(phase, new[] { charge }), Is.False);
            Assert.That((bool)shouldHide.Invoke(phase, new[] { idle }), Is.False);

            Type cueType = RuntimeType("BeatMemories.EnemyPreviewCue");
            object hiddenCue = Activator.CreateInstance(cueType, new object[] { 0, attack, true });
            Assert.That(cueType.GetField("VisibleEnemy").GetValue(hiddenCue), Is.Null);
            Assert.That((bool)cueType.GetField("IsHidden").GetValue(hiddenCue), Is.True);
        }

        private void AssertWeights(string path)
        {
            var serialized = new SerializedObject(Load(path));
            SerializedProperty weights = serialized.FindProperty("enemyWeights");
            Assert.That(weights.arraySize, Is.EqualTo(4));
            CollectionAssert.AreEqual(
                new[] { 2f, 2f, 1f, 1f },
                Enumerable.Range(0, weights.arraySize)
                    .Select(i => weights.GetArrayElementAtIndex(i)
                        .FindPropertyRelative("weight").floatValue)
                    .ToArray());
        }

        private Type RuntimeType(string name)
            => runtimeAssembly.GetType(name, throwOnError: true);

        private object Judge(
            UnityEngine.Object enemy,
            int input,
            bool chargedAttack = false)
        {
            return judgeSystemType.GetMethod("Judge").Invoke(
                null,
                new[] { enemy, Enum.ToObject(playerActionType, input), (object)chargedAttack });
        }

        private static T ResultField<T>(object result, string name)
            => (T)result.GetType().GetField(name).GetValue(result);

        private static UnityEngine.Object Load(string path)
        {
            UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(path);
            Assert.That(asset, Is.Not.Null, path);
            return asset;
        }

        private static void AssertAssetPath(UnityEngine.Object asset, string expectedPath)
        {
            Assert.That(asset, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(asset), Is.EqualTo(expectedPath));
        }
    }
}
