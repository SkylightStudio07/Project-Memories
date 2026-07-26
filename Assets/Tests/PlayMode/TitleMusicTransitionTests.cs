using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BeatMemories.Tests
{
    public class TitleMusicTransitionTests
    {
        private const string TitleSceneName = "Title";
        private const string GameSceneName = "BeatMemories_Dayeon";

        [UnityTest]
        public IEnumerator StartGameFadesBeforeLoadingAndIgnoresSecondClick()
        {
            SceneManager.LoadScene(TitleSceneName);
            yield return null;

            Type titleType = RuntimeType("Title");
            Component title = null;
            foreach (GameObject root in
                SceneManager.GetActiveScene().GetRootGameObjects())
            {
                title = root.GetComponentInChildren(titleType, true)
                    as Component;
                if (title != null) break;
            }
            Assert.That(title, Is.Not.Null);

            MethodInfo startGame = titleType.GetMethod("StartGame");
            Assert.That(startGame, Is.Not.Null);

            float startedAt = Time.realtimeSinceStartup;
            startGame.Invoke(title, null);
            startGame.Invoke(title, null);

            yield return new WaitForSecondsRealtime(0.15f);
            Assert.That(
                SceneManager.GetActiveScene().name,
                Is.EqualTo(TitleSceneName));

            while (SceneManager.GetActiveScene().name == TitleSceneName
                && Time.realtimeSinceStartup - startedAt < 2f)
            {
                yield return null;
            }

            float elapsed = Time.realtimeSinceStartup - startedAt;
            Assert.That(
                SceneManager.GetActiveScene().name,
                Is.EqualTo(GameSceneName));
            Assert.That(elapsed, Is.GreaterThanOrEqualTo(0.25f));
        }

        private static Type RuntimeType(string fullName)
        {
            Assembly runtimeAssembly = Array.Find(
                AppDomain.CurrentDomain.GetAssemblies(),
                assembly => assembly.GetType(fullName) != null);
            Assert.That(runtimeAssembly, Is.Not.Null, fullName);
            return runtimeAssembly.GetType(fullName, true);
        }
    }
}
