using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace BeatMemories.Tests
{
    public class Stage1PhaseTests
    {
        [Test]
        public void StageOneUsesOnlyThePublicBasicPhase()
        {
            Object stage = AssetDatabase.LoadAssetAtPath<Object>(
                "Assets/Data/Stages/Stage_1.asset");
            Assert.That(stage, Is.Not.Null);

            var serializedStage = new SerializedObject(stage);
            SerializedProperty phases = serializedStage.FindProperty("phases");

            Assert.That(phases.arraySize, Is.EqualTo(1));
            Assert.That(
                AssetDatabase.GetAssetPath(
                    phases.GetArrayElementAtIndex(0).objectReferenceValue),
                Is.EqualTo("Assets/Data/Phases/Phase_Basic_Intro.asset"));
            Assert.That(
                serializedStage.FindProperty("cyclesPerPhase").intValue,
                Is.EqualTo(4));
            Assert.That(
                serializedStage.FindProperty("repeatPhasePlan").boolValue,
                Is.False);

            Object phase = phases.GetArrayElementAtIndex(0).objectReferenceValue;
            var serializedPhase = new SerializedObject(phase);

            Assert.That(serializedPhase.FindProperty("kind").enumValueIndex, Is.Zero);
            Assert.That(
                serializedPhase.FindProperty("preparationTintStrength").floatValue,
                Is.Zero);
            Assert.That(
                serializedPhase.FindProperty("activeTintStrength").floatValue,
                Is.Zero);
        }
    }
}
