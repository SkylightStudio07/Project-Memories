using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace BeatMemories.Tests
{
    public class StageTransitionHudTests
    {
        private readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = created.Count - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(created[i]);
            created.Clear();
        }

        [Test]
        public void ApplyingStageRefreshesEnemyHudBeforeNextFrame()
        {
            Assembly runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(assembly => assembly.GetType("BeatMemories.RoundManager") != null);
            Type roundType = runtimeAssembly.GetType("BeatMemories.RoundManager", true);
            Type hudType = runtimeAssembly.GetType("BeatMemories.HudView", true);
            Type stageType = runtimeAssembly.GetType("BeatMemories.StageSO", true);

            var roundObject = new GameObject("Round");
            var hudObject = new GameObject("HUD");
            created.Add(roundObject);
            created.Add(hudObject);
            Component round = roundObject.AddComponent(roundType);
            Component hud = hudObject.AddComponent(hudType);
            roundType.GetMethod(
                "Awake",
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(round, null);

            FieldInfo roundField = hudType.GetField(
                "round",
                BindingFlags.Instance | BindingFlags.NonPublic);
            roundField.SetValue(hud, round);
            hudType.GetMethod(
                "OnEnable",
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(hud, null);

            var texture = new Texture2D(1, 1);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f));
            created.Add(sprite);
            created.Add(texture);

            ScriptableObject stage = ScriptableObject.CreateInstance(stageType);
            created.Add(stage);
            stageType.GetField("enemySprite").SetValue(stage, sprite);

            roundType.GetMethod("SetStage").Invoke(round, new object[] { stage });

            FieldInfo idleSpriteField = hudType.GetField(
                "enemyIdleSprite",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(
                idleSpriteField.GetValue(hud),
                Is.SameAs(sprite),
                "스테이지 적용 프레임에 HUD가 새 적을 반영해야 첫 예고를 지우지 않는다.");
        }
    }
}
