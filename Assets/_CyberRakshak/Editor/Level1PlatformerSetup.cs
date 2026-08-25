using System.Linq;
using CyberRakshak.Platformer;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CyberRakshak.Editor
{
    /// <summary>Editor-only inspection utility for placing the Level 1 platformer encounter on existing props.</summary>
    public static class Level1PlatformerSetup
    {
        private const string ScenePath = "Assets/_CyberRakshak/Scenes/Game_Level01.unity";
        private const string SpaceManPrefabPath = "Assets/_CyberRakshak/FreeAnimatedSpaceMan/Prefab/space_man_model.prefab";
        private const string FirewallPrefabPath = "Assets/_CyberRakshak/External assets/PyroParticles/Prefab/Prefab/WallOfFire.prefab";

        [MenuItem("CyberRakshak/Report Level 1 Prop Positions")]
        public static void ReportLevel1PropPositions()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var lines = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(transform => transform.name.Contains("Barrel") || transform.name.Contains("Cube") || transform.name.Contains("Server"))
                .Select(transform => $"{transform.name} @ {transform.position}")
                .OrderBy(line => line);

            Debug.Log("LEVEL 1 PROP POSITIONS\n" + string.Join("\n", lines));
        }

        [MenuItem("CyberRakshak/Configure Level 1 Platformer Encounter")]
        public static void ConfigureLevel1PlatformerEncounter()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject previousEncounter = GameObject.Find("PlatformerEncounter");
            if (previousEncounter != null)
            {
                Object.DestroyImmediate(previousEncounter);
            }

            GameObject spaceManPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpaceManPrefabPath);
            if (spaceManPrefab == null)
            {
                throw new System.InvalidOperationException($"Missing SpaceMan prefab at {SpaceManPrefabPath}.");
            }

            GameObject encounter = new GameObject("PlatformerEncounter");
            encounter.AddComponent<Level1PlatformerBootstrap>();

            string[] propNames =
            {
                "Barrel_Large_Blue",
                "Barrel_Large_Blue (1)",
                "Barrel_Large_Blue (2)",
                "Barrel_Large_Blue (3)"
            };

            foreach (string propName in propNames)
            {
                GameObject prop = GameObject.Find(propName);
                if (prop == null)
                {
                    throw new System.InvalidOperationException($"Level 1 spawn prop was not found: {propName}.");
                }

                CreateSpawnAndEnemy(encounter.transform, prop.transform, spaceManPrefab);
            }

            CreateFirewallBlocker(encounter.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Configured four SpaceMan platformer enemies from the Level 1 blue barrel spawn props.");
        }

        private static void CreateSpawnAndEnemy(Transform encounter, Transform prop, GameObject spaceManPrefab)
        {
            GameObject spawn = new GameObject($"{prop.name}_EnemySpawn");
            spawn.transform.SetParent(encounter, true);
            spawn.transform.position = prop.position;

            // Move from the prop toward the runway, leaving the visual spawn prop intact.
            float towardRunway = prop.position.x < 0f ? 1f : -1f;
            Vector3 enemyPosition = prop.position + new Vector3(towardRunway * 2.4f, 0f, 0f);
            GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(spaceManPrefab, encounter);
            enemy.name = $"FirewallSpaceMan_{prop.name}";
            enemy.transform.position = enemyPosition;
            enemy.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            enemy.transform.localScale = Vector3.one * 5f;

            CapsuleCollider hitbox = enemy.GetComponent<CapsuleCollider>();
            if (hitbox == null)
            {
                hitbox = enemy.AddComponent<CapsuleCollider>();
            }

            hitbox.center = Vector3.zero;
            hitbox.radius = 0.24f;
            hitbox.height = 0.76f;
            hitbox.isTrigger = true;

            Rigidbody body = enemy.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = enemy.AddComponent<Rigidbody>();
            }

            body.isKinematic = false;
            body.useGravity = true;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            enemy.AddComponent<PlatformerEnemy>();
        }

        private static void CreateFirewallBlocker(Transform encounter)
        {
            GameObject yellowStructure = GameObject.Find("TrainingGround (3)");
            GameObject firewallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FirewallPrefabPath);
            if (yellowStructure == null || firewallPrefab == null)
                throw new System.InvalidOperationException("Level 1 firewall source or yellow structure is missing.");

            GameObject blocker = new GameObject("FirewallBlocker");
            blocker.transform.SetParent(encounter, true);
            blocker.transform.position = yellowStructure.transform.position + new Vector3(0f, 0f, -5.2f);
            blocker.transform.rotation = Quaternion.identity;

            GameObject fire = (GameObject)PrefabUtility.InstantiatePrefab(firewallPrefab, blocker.transform);
            fire.name = "WallOfFire_Hazard";
            fire.transform.localPosition = Vector3.zero;
            fire.transform.localRotation = Quaternion.identity;
            fire.transform.localScale = new Vector3(12f, 1.5f, 1f);

            BoxCollider wallCollider = blocker.AddComponent<BoxCollider>();
            wallCollider.center = new Vector3(0f, 1.1f, 0f);
            wallCollider.size = new Vector3(30f, 2.2f, 0.8f);
        }
    }
}
