using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace CyberRakshak.Tests
{
    public class Level1PlaythroughTest
    {
        [UnityTest]
        public IEnumerator InspectLevel1_LogsFindings()
        {
            // Load the scene
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Level01.unity");
            
            // Wait for it to fully load
            yield return null;
            yield return new WaitForSeconds(1f);

            string report = "\n=== CLI PLAYTHROUGH FINDINGS ===\n";

            // Find Player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                report += $"- Player found: {player.name} at {player.transform.position}\n";
            } else {
                report += "- No Player found!\n";
            }

            // Find PATCH
            GameObject patch = GameObject.Find("PATCH");
            if (patch != null) {
                report += $"- PATCH found: {patch.name}\n";
            } else {
                report += "- No PATCH companion found.\n";
            }

            // Count Geometry
            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            int cubes = 0, walls = 0, gates = 0, keys = 0;
            foreach(var r in renderers) {
                string n = r.gameObject.name.ToLower();
                if (n.Contains("cube")) cubes++;
                if (n.Contains("wall")) walls++;
                if (n.Contains("gate") || n.Contains("firewall")) gates++;
                if (n.Contains("key")) keys++;
            }
            report += $"- Geometry: {walls} Walls, {gates} Gates, {keys} Keys, {cubes} Cubes.\n";

            // Count Triggers
            var colliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);
            int triggers = colliders.Count(c => c.isTrigger);
            report += $"- Triggers: {triggers} trigger volumes.\n";

            report += "================================\n";
            
            Debug.Log(report);
            
            // The test passes if it reaches this point without crashing
            Assert.Pass();
        }
    }
}
