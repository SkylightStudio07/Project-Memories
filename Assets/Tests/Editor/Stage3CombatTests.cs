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
    public class Stage3CombatTests
    {
        private readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
        private Assembly runtimeAssembly;
        private Type enemyType;
        private Type playerActionType;
        private Type judgeSystemType;
        private Type providerType;
        private Type roundManagerType;

        [SetUp]
        public void SetUp()
        {
            runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(assembly => assembly.GetType("BeatMemories.Enemy") != null);
            enemyType = RuntimeType("BeatMemories.Enemy");
            playerActionType = RuntimeType("BeatMemories.PlayerAction");
            judgeSystemType = RuntimeType("BeatMemories.JudgeSystem");
            providerType = RuntimeType("BeatMemories.EnemySequenceProvider");
            roundManagerType = RuntimeType("BeatMemories.RoundManager");
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
                UnityEngine.Object.DestroyImmediate(created[i]);
            created.Clear();
        }

        [Test]
        public void AttackCancelsCharge()
        {
            UnityEngine.Object charge = CreateEnemy(action: 4);

            object result = Judge(charge, input: 2);

            Assert.That(ResultField<bool>(result, "Cleared"), Is.True);
            Assert.That(ResultField<int>(result, "PlayerDamage"), Is.Zero);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        public void NonAttackDoesNotCancelCharge(int input)
        {
            UnityEngine.Object charge = CreateEnemy(action: 4);

            object result = Judge(charge, input);

            Assert.That(ResultField<bool>(result, "Cleared"), Is.False);
            Assert.That(Convert.ToInt32(ResultField<object>(result, "Type")), Is.EqualTo(1));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        public void ChargedAttackAlwaysDealsTwoDamage(int input)
        {
            UnityEngine.Object attack = CreateEnemy(
                action: 2,
                attackDamage: 2,
                unblockable: true,
                invulnerable: true,
                fixedDamage: true);

            object result = Judge(attack, input, chargedAttack: true);

            Assert.That(ResultField<bool>(result, "Cleared"), Is.False);
            Assert.That(Convert.ToInt32(ResultField<object>(result, "Type")), Is.EqualTo(2));
            Assert.That(ResultField<int>(result, "PlayerDamage"), Is.EqualTo(2));
        }

        [Test]
        public void InterruptedIdleIsVulnerable()
        {
            UnityEngine.Object idle = CreateEnemy(action: 0);

            Assert.That(ResultField<bool>(Judge(idle, input: 2), "Cleared"), Is.True);
            Assert.That(
                Convert.ToInt32(ResultField<object>(Judge(idle, input: 1), "Type")),
                Is.EqualTo(1));
        }

        [Test]
        public void GeneratedCycleKeepsChargeAttackPairsAdjacent()
        {
            UnityEngine.Object chargedAttack = CreateEnemy(action: 2);
            UnityEngine.Object charge = CreateEnemy(action: 4);
            SetDataField(charge, "forcedFollowUp", chargedAttack);

            Array pool = Array.CreateInstance(enemyType, 1);
            pool.SetValue(charge, 0);
            object provider = Activator.CreateInstance(providerType, new object[] { 12345, pool });
            IList cycle = (IList)providerType
                .GetMethod("GenerateCycleWeighted")
                .Invoke(provider, new object[] { 0, 4, null });

            Assert.That(cycle.Count, Is.EqualTo(4));
            Assert.That(cycle[0], Is.SameAs(charge));
            Assert.That(cycle[1], Is.SameAs(chargedAttack));
            Assert.That(cycle[2], Is.SameAs(charge));
            Assert.That(cycle[3], Is.SameAs(chargedAttack));
        }

        [Test]
        public void ClearingChargeReplacesOnlyItsImmediateFollowUp()
        {
            UnityEngine.Object chargedAttack = CreateEnemy(action: 2);
            UnityEngine.Object interruptedIdle = CreateEnemy(action: 0);
            UnityEngine.Object charge = CreateEnemy(action: 4);
            SetDataField(charge, "forcedFollowUp", chargedAttack);
            SetDataField(charge, "interruptedFollowUp", interruptedIdle);

            GameObject gameObject = new GameObject("RoundManager Test");
            created.Add(gameObject);
            Component round = gameObject.AddComponent(roundManagerType);
            Type listType = typeof(List<>).MakeGenericType(enemyType);
            IList cycle = (IList)Activator.CreateInstance(listType);
            cycle.Add(charge);
            cycle.Add(chargedAttack);
            cycle.Add(charge);
            cycle.Add(chargedAttack);
            roundManagerType.GetField(
                    "currentCycle",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(round, cycle);

            roundManagerType.GetMethod(
                    "ReplaceInterruptedFollowUp",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(round, new object[] { 0, charge });

            Assert.That(cycle[1], Is.SameAs(interruptedIdle));
            Assert.That(cycle[3], Is.SameAs(chargedAttack));
        }

        [Test]
        public void StageThreeUsesBallroomAssetsAndAdvances()
        {
            UnityEngine.Object stage2 = AssetDatabase.LoadMainAssetAtPath(
                "Assets/Data/Stages/Stage_2.asset");
            UnityEngine.Object stage3 = AssetDatabase.LoadMainAssetAtPath(
                "Assets/Data/Stages/Stage_3.asset");
            Assert.That(stage2, Is.Not.Null);
            Assert.That(stage3, Is.Not.Null);

            var stage2Serialized = new SerializedObject(stage2);
            var stage3Serialized = new SerializedObject(stage3);
            Assert.That(stage2Serialized.FindProperty("repeatPhasePlan").boolValue, Is.False);
            Assert.That(stage3Serialized.FindProperty("repeatPhasePlan").boolValue, Is.False);
            Assert.That(stage3Serialized.FindProperty("keyMode").enumValueIndex, Is.EqualTo(1));
            Assert.That(stage3Serialized.FindProperty("cyclesPerPhase").intValue, Is.EqualTo(2));
            Assert.That(stage3Serialized.FindProperty("bpm").floatValue, Is.EqualTo(90f));
            Assert.That(stage3Serialized.FindProperty("playerMaxHp").intValue, Is.EqualTo(7));

            SerializedProperty pool = stage3Serialized.FindProperty("enemyPool");
            Assert.That(pool.arraySize, Is.EqualTo(4));
            AssertAssetPath(
                pool.GetArrayElementAtIndex(0).objectReferenceValue,
                "Assets/Data/Enemies/Stage 3 Attack.asset");
            AssertAssetPath(
                pool.GetArrayElementAtIndex(1).objectReferenceValue,
                "Assets/Data/Enemies/Stage 3 Guard.asset");
            AssertAssetPath(
                pool.GetArrayElementAtIndex(2).objectReferenceValue,
                "Assets/Data/Enemies/Stage 3 Charge.asset");
            AssertAssetPath(
                pool.GetArrayElementAtIndex(3).objectReferenceValue,
                "Assets/Data/Enemies/Stage 3 Idle.asset");
            AssertAssetPath(
                stage3Serialized.FindProperty("enemySprite").objectReferenceValue,
                "Assets/Resource/Art/Character/nai-3213a06d-1ed2-4159-af6f-9ddee9285821.png");
            AssertAssetPath(
                stage3Serialized.FindProperty("backgroundSprite").objectReferenceValue,
                "Assets/Resource/Art/Background/Stage3_DeceptiveBallroom_BackLayer_v1.png");
            AssertAssetPath(
                stage3Serialized.FindProperty("floorSprite").objectReferenceValue,
                "Assets/Resource/Art/Background/Stage3_DeceptiveBallroom_FloorLayer_v1.png");
            AssertAssetPath(
                stage3Serialized.FindProperty("backgroundPrefab").objectReferenceValue,
                "Assets/Data/Stages/환경요소_액트4.prefab");
        }

        [Test]
        public void ExistingAttackEnemyKeepsOneDamageDefault()
        {
            UnityEngine.Object enemy = AssetDatabase.LoadMainAssetAtPath(
                "Assets/Data/Enemies/Enemy_aggressive.asset");
            Assert.That(enemy, Is.Not.Null);

            object result = Judge(enemy, input: 0);

            Assert.That(ResultField<int>(result, "PlayerDamage"), Is.EqualTo(1));
        }

        private Type RuntimeType(string name)
            => runtimeAssembly.GetType(name, throwOnError: true);

        private UnityEngine.Object CreateEnemy(
            int action,
            int attackDamage = 1,
            bool unblockable = false,
            bool invulnerable = false,
            bool fixedDamage = false)
        {
            UnityEngine.Object enemy = ScriptableObject.CreateInstance(enemyType);
            created.Add(enemy);
            SetDataField(enemy, "action", Enum.ToObject(playerActionType, action));
            SetDataField(enemy, "attackDamage", attackDamage);
            SetDataField(enemy, "unblockableAttack", unblockable);
            SetDataField(enemy, "invulnerableWhileActing", invulnerable);
            SetDataField(enemy, "fixedAttackDamage", fixedDamage);
            SetDataField(enemy, "maxHp", 1);
            return enemy;
        }

        private object Judge(UnityEngine.Object enemy, int input, bool chargedAttack = false)
        {
            return judgeSystemType.GetMethod("Judge").Invoke(
                null,
                new[] { enemy, Enum.ToObject(playerActionType, input), (object)chargedAttack });
        }

        private void SetDataField(UnityEngine.Object enemy, string field, object value)
        {
            object data = enemyType.GetProperty("Data").GetValue(enemy);
            data.GetType().GetField(field).SetValue(data, value);
        }

        private static T ResultField<T>(object result, string name)
            => (T)result.GetType().GetField(name).GetValue(result);

        private static void AssertAssetPath(UnityEngine.Object asset, string expectedPath)
        {
            Assert.That(asset, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(asset), Is.EqualTo(expectedPath));
        }
    }
}
