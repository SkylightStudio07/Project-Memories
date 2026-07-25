using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace BeatMemories.Tests
{
    public class CharacterPrefabStructureTests
    {
        [Test]
        public void EveryStageUsesCharacterPrefabsWithEditableAnchors()
        {
            Object[] stages = AssetDatabase.FindAssets("t:StageSO")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Object>)
                .Where(stage => stage != null)
                .ToArray();

            Assert.That(stages, Is.Not.Empty);
            foreach (Object stage in stages)
            {
                SerializedObject serializedStage = new SerializedObject(stage);
                Object playerPrefab =
                    serializedStage.FindProperty("playerPrefab").objectReferenceValue;
                Object enemyPrefab =
                    serializedStage.FindProperty("enemyPrefab").objectReferenceValue;
                Assert.That(playerPrefab, Is.Not.Null, stage.name);
                Assert.That(enemyPrefab, Is.Not.Null, stage.name);
                AssertCharacterPrefab(playerPrefab, stage.name + " player");
                AssertCharacterPrefab(enemyPrefab, stage.name + " enemy");
            }
        }

        private static void AssertCharacterPrefab(Object view, string context)
        {
            SerializedObject serializedView = new SerializedObject(view);
            Assert.That(
                serializedView.FindProperty("characterData").objectReferenceValue,
                Is.Not.Null,
                context + " data");
            Assert.That(
                serializedView.FindProperty("spriteRenderer").objectReferenceValue,
                Is.Not.Null,
                context + " renderer");
            Assert.That(
                serializedView.FindProperty("laserMuzzle").objectReferenceValue,
                Is.Not.Null,
                context + " muzzle");
            Assert.That(
                serializedView.FindProperty("hitAnchor").objectReferenceValue,
                Is.Not.Null,
                context + " hit");
            Assert.That(
                serializedView.FindProperty("effectAnchor").objectReferenceValue,
                Is.Not.Null,
                context + " effect");
        }
    }
}
