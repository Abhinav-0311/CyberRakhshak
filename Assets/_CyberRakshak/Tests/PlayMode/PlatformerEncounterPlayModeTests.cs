using System.Collections;
using System.Linq;
using CyberRakshak.Platformer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CyberRakshak.Tests
{
    public sealed class PlatformerEncounterPlayModeTests
    {
        [UnityTest]
        public IEnumerator Level1_HasFourPlatformerEnemiesAndPlayerHealth()
        {
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Level01.unity");
            yield return null;
            yield return new WaitForSeconds(0.25f);

            Assert.That(Object.FindObjectsByType<PlatformerEnemy>(FindObjectsSortMode.None).Length, Is.EqualTo(4));
            Assert.That(GameObject.Find("PlayerArmature")?.GetComponent<PlayerHealth>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<PlatformerHud>(), Is.Not.Null);
        }
    }
}
