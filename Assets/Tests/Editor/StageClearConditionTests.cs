using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace BeatMemories.Tests
{
    public class StageClearConditionTests
    {
        private readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
        private Type roundManagerType;
        private Type stageType;
        private Type phaseType;

        [SetUp]
        public void SetUp()
        {
            Assembly runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(assembly => assembly.GetType("BeatMemories.RoundManager") != null);
            roundManagerType = runtimeAssembly.GetType("BeatMemories.RoundManager", true);
            stageType = runtimeAssembly.GetType("BeatMemories.StageSO", true);
            phaseType = runtimeAssembly.GetType("BeatMemories.PhaseSO", true);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = created.Count - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(created[i]);
            created.Clear();
        }

        [Test]
        public void StageClearIsQueuedOnlyWhenEnemyHpReachesZero()
        {
            Component round = CreateRoundWithEnemyHp(2);
            MethodInfo handleResponseEnd = PrivateMethod("HandleResponseEnd");
            MethodInfo damageEnemy = PrivateMethod("DamageEnemy");
            FieldInfo clearPending = PrivateField("stageClearPending");

            handleResponseEnd.Invoke(round, new object[] { 99 });
            Assert.That(clearPending.GetValue(round), Is.False,
                "페이즈 계획이 끝나도 적 HP가 남아 있으면 클리어하면 안 된다.");

            damageEnemy.Invoke(round, null);
            Assert.That(PublicInt(round, "CurrentEnemyHp"), Is.EqualTo(1));
            Assert.That(clearPending.GetValue(round), Is.False);

            damageEnemy.Invoke(round, null);
            Assert.That(PublicInt(round, "CurrentEnemyHp"), Is.Zero);
            Assert.That(clearPending.GetValue(round), Is.True,
                "적 HP가 0이 되면 현재 응답 종료 시 클리어를 예약해야 한다.");
        }

        private Component CreateRoundWithEnemyHp(int enemyMaxHp)
        {
            var gameObject = new GameObject("Stage Clear Test");
            created.Add(gameObject);
            Component round = gameObject.AddComponent(roundManagerType);

            ScriptableObject stage = ScriptableObject.CreateInstance(stageType);
            created.Add(stage);
            stageType.GetField("enemyMaxHp").SetValue(stage, enemyMaxHp);
            stageType.GetField("cyclesPerPhase").SetValue(stage, 1);
            stageType.GetField("repeatPhasePlan").SetValue(stage, false);

            ScriptableObject phase = ScriptableObject.CreateInstance(phaseType);
            created.Add(phase);
            ((IList)stageType.GetField("phases").GetValue(stage)).Add(phase);

            roundManagerType.GetMethod("SetStage").Invoke(round, new object[] { stage });
            Assert.That(PublicInt(round, "EnemyMaxHp"), Is.EqualTo(enemyMaxHp));
            Assert.That(PublicInt(round, "CurrentEnemyHp"), Is.EqualTo(enemyMaxHp));
            return round;
        }

        private MethodInfo PrivateMethod(string name)
            => roundManagerType.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo PrivateField(string name)
            => roundManagerType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);

        private int PublicInt(Component target, string name)
            => (int)roundManagerType.GetProperty(name).GetValue(target);
    }
}
