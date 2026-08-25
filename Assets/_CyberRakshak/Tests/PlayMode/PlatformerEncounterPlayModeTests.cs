using System.Collections;
using System.Linq;
using CyberRakshak.Platformer;
using CyberRakshak.Runtime;
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

        [UnityTest]
        public IEnumerator Spaceman_StompFromStarterAssetsFeet_DefeatsEnemy()
        {
            GameObject enemyObject = new GameObject("StompTarget");
            enemyObject.transform.position = new Vector3(0f, 1f, 40f);
            PlatformerEnemy enemy = enemyObject.AddComponent<PlatformerEnemy>();

            GameObject playerObject = new GameObject("StompPlayer");
            playerObject.transform.position = new Vector3(0f, 1.25f, 40f);
            CharacterController playerCollider = playerObject.AddComponent<CharacterController>();
            playerCollider.center = new Vector3(0f, .93f, 0f);
            playerCollider.height = 1.8f;
            playerObject.AddComponent<PlayerHealth>();

            yield return null;

            Assert.That(enemy.TryResolvePlayerContact(playerCollider), Is.True);
            Assert.That(enemy.IsDefeated, Is.True);

            yield return new WaitForSeconds(0.4f);
            Assert.That(enemyObject == null, Is.True);
            Object.Destroy(playerObject);
        }
    }
}
