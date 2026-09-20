using System.Collections;
using CyberRakshak.Platformer;
using CyberRakshak.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace CyberRakshak.Tests
{
    public sealed class Level1PlaythroughTest
    {
        [UnityTest]
        public IEnumerator Level1_HasRequiredPlayerRouteWiring()
        {
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Level01.unity");
            yield return null;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject gate = GameObject.Find("Firewall_Gate");
            LeverSwitch lever = Object.FindFirstObjectByType<LeverSwitch>();
            GameObject completionZone = GameObject.Find("Level1_Complete_Zone");

            Assert.That(player, Is.Not.Null, "Level 1 needs a tagged player.");
            Assert.That(player.GetComponent<PlayerHealth>(), Is.Not.Null, "Level 1 player needs health/restart handling.");
            Assert.That(Object.FindFirstObjectByType<PlatformerHud>(), Is.Not.Null, "Level 1 needs its health HUD.");
            Assert.That(Object.FindObjectsByType<PlatformerEnemy>(FindObjectsSortMode.None).Length, Is.EqualTo(4));
            Assert.That(gate?.GetComponent<Collider>(), Is.Not.Null, "Firewall_Gate needs a collider.");
            Assert.That(lever?.waterfall, Is.Not.Null, "Water lever needs its waterfall reference.");
            Assert.That(lever?.firewall, Is.Not.Null, "Water lever needs the exact firewall reference.");
            Assert.That(completionZone, Is.Not.Null, "Level 1 needs a completion trigger after the treadmill section.");
            Assert.That(completionZone?.GetComponent<BoxCollider>(), Is.Not.Null, "Level 1 completion zone needs a collider.");
            Assert.That(completionZone != null && completionZone.GetComponent<BoxCollider>().isTrigger, Is.True, "Level 1 completion collider must be a trigger.");
        }

        [UnityTest]
        public IEnumerator Tutorial_HasFallRecoveryAndCompletionZone()
        {
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Tutorial.unity");
            yield return null;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject completionZone = GameObject.Find("Tutorial_Complete_Zone");
            Assert.That(player?.GetComponent<SceneFallRecovery>(), Is.Not.Null, "Tutorial player needs fall recovery.");
            Assert.That(completionZone, Is.Not.Null, "Tutorial needs a completion trigger.");
            Assert.That(completionZone?.GetComponent<BoxCollider>(), Is.Not.Null, "Tutorial completion zone needs a collider.");
            Assert.That(completionZone != null && completionZone.GetComponent<BoxCollider>().isTrigger, Is.True, "Tutorial completion collider must be a trigger.");
        }
    }
}
