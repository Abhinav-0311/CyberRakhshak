using CyberRakshak.Platformer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CyberRakshak.Runtime
{
    /// <summary>Creates explicit finish volumes from the authored end surfaces in the two reviewable modules.</summary>
    public static class ModuleExitBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneCallbacks()
        {
            SceneManager.sceneLoaded -= CreateExitForLoadedModule;
            SceneManager.sceneLoaded += CreateExitForLoadedModule;
        }

        private static void CreateExitForLoadedModule(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Game_Tutorial")
            {
                EnsureTutorialFallRecovery();
                CreateTutorialExit();
            }
            else if (scene.name == "Game_Level01")
            {
                CreateLevelOneExit();
            }
        }

        private static void EnsureTutorialFallRecovery()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            CharacterController controller = player != null ? player.GetComponentInChildren<CharacterController>() : null;
            if (controller != null && controller.GetComponent<SceneFallRecovery>() == null)
            {
                controller.gameObject.AddComponent<SceneFallRecovery>();
            }
        }

        private static void CreateTutorialExit()
        {
            if (GameObject.Find("Tutorial_Complete_Zone") != null)
            {
                return;
            }

            GameObject ground = GameObject.Find("TrainingGround");
            Collider groundCollider = ground != null ? ground.GetComponent<Collider>() : null;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (groundCollider == null || player == null)
            {
                Debug.LogWarning("Tutorial completion zone was not created because the ground or player is missing.");
                return;
            }

            Bounds bounds = groundCollider.bounds;
            bool exitAtPositiveZ = player.transform.position.z <= bounds.center.z;
            float z = exitAtPositiveZ ? bounds.max.z - 1.5f : bounds.min.z + 1.5f;
            CreateZone("Tutorial_Complete_Zone", new Vector3(bounds.center.x, bounds.max.y + 2f, z),
                new Vector3(Mathf.Max(4f, bounds.size.x - 2f), 4f, 2f), true);
        }

        private static void CreateLevelOneExit()
        {
            if (GameObject.Find("Level1_Complete_Zone") != null)
            {
                return;
            }

            TreadmillPlatform[] treadmills = Object.FindObjectsByType<TreadmillPlatform>(FindObjectsSortMode.None);
            if (treadmills.Length == 0)
            {
                Debug.LogWarning("Level 1 completion zone was not created because no treadmill was found.");
                return;
            }

            Collider firstSurface = treadmills[0].GetComponent<Collider>();
            if (firstSurface == null) return;
            Bounds endBounds = firstSurface.bounds;
            foreach (TreadmillPlatform treadmill in treadmills)
            {
                Collider collider = treadmill.GetComponent<Collider>();
                if (collider != null && collider.bounds.max.z > endBounds.max.z)
                {
                    endBounds = collider.bounds;
                }
            }

            // Finish on the authored landing platform, never in the gap after the last belt.
            Transform platforms = treadmills[0].transform.parent.parent;
            if (platforms == null) return;
            foreach (Collider surface in platforms.GetComponentsInChildren<Collider>())
            {
                if (surface.enabled && !surface.isTrigger && surface.bounds.min.z >= endBounds.max.z &&
                    surface.bounds.max.y >= endBounds.max.y - 1f)
                {
                    endBounds = surface.bounds;
                }
            }

            CreateZone("Level1_Complete_Zone", new Vector3(endBounds.center.x, endBounds.max.y + 2f, endBounds.center.z),
                new Vector3(Mathf.Max(8f, endBounds.size.x + 4f), 4f, 2.5f), false);
        }

        private static void CreateZone(string name, Vector3 position, Vector3 size, bool completesTutorial)
        {
            GameObject zone = new GameObject(name);
            zone.transform.position = position;
            BoxCollider collider = zone.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;
            ModuleCompletionTrigger completion = zone.AddComponent<ModuleCompletionTrigger>();
            completion.Configure(completesTutorial, "LevelSelect", 2f);
            GameObject sign = new GameObject("FinishSign", typeof(TMPro.TextMeshPro));
            sign.transform.SetParent(zone.transform, false);
            sign.transform.localPosition = Vector3.up;
            TMPro.TextMeshPro label = sign.GetComponent<TMPro.TextMeshPro>();
            label.text = "TRAINING EXIT";
            label.fontSize = 5f;
            label.alignment = TMPro.TextAlignmentOptions.Center;
            label.color = Color.cyan;
        }
    }
}
