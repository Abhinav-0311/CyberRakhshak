using CyberRakshak.PATCH;
using UnityEngine;

namespace CyberRakshak.Platformer
{
    /// <summary>Single route gate for the patrol encounter; it clears only after the authored enemies are defeated.</summary>
    public sealed class EnemySectionBarrier : MonoBehaviour
    {
        private const float Width = 17f;
        private const float Height = 3.2f;
        private const float Depth = .35f;

        private bool cleared;

        public static void EnsureCreated()
        {
            if (FindFirstObjectByType<EnemySectionBarrier>() != null)
            {
                return;
            }

            PlatformerEnemy[] enemies = FindObjectsByType<PlatformerEnemy>(FindObjectsSortMode.None);
            if (enemies.Length == 0)
            {
                return;
            }

            float furthestEnemyZ = enemies[0].transform.position.z;
            foreach (PlatformerEnemy enemy in enemies)
            {
                furthestEnemyZ = Mathf.Max(furthestEnemyZ, enemy.transform.position.z);
            }

            Vector3 position = new Vector3(0f, Height * .5f, furthestEnemyZ + 11f);
            if (Physics.Raycast(position + Vector3.up * 8f, Vector3.down, out RaycastHit ground, 16f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                position.y = ground.point.y + Height * .5f;
            }

            GameObject gate = new GameObject("Patrol_Clearance_Barrier");
            gate.transform.position = position;
            gate.AddComponent<EnemySectionBarrier>();
        }

        private void Awake()
        {
            BoxCollider blocker = gameObject.AddComponent<BoxCollider>();
            blocker.size = new Vector3(Width, Height, Depth);

            Material material = CreateBarrierMaterial();
            CreateBar("Top", new Vector3(0f, Height * .5f, 0f), new Vector3(Width, .16f, Depth), material);
            CreateBar("Bottom", new Vector3(0f, -Height * .5f, 0f), new Vector3(Width, .16f, Depth), material);
            CreateBar("Left", new Vector3(-Width * .5f, 0f, 0f), new Vector3(.16f, Height, Depth), material);
            CreateBar("Right", new Vector3(Width * .5f, 0f, 0f), new Vector3(.16f, Height, Depth), material);
        }

        private void Update()
        {
            if (!cleared && FindObjectsByType<PlatformerEnemy>(FindObjectsSortMode.None).Length == 0)
            {
                cleared = true;
                PlatformerHud.Instance?.SetObjective("USE THE LAUNCH PAD TO REACH WATER CONTROL");
                PatchDialoguePresenter.Ensure().Show("PATCH", "Access barrier cleared. Proceed to the firewall controls.", 4f);
                Destroy(gameObject);
            }
        }

        private void CreateBar(string name, Vector3 localPosition, Vector3 scale, Material material)
        {
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = name;
            bar.transform.SetParent(transform, false);
            bar.transform.localPosition = localPosition;
            bar.transform.localScale = scale;
            Destroy(bar.GetComponent<BoxCollider>());
            bar.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static Material CreateBarrierMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new Material(shader);
            Color cyan = new Color(.08f, .8f, 1f, 1f);
            material.color = cyan;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", cyan);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", cyan * 1.5f);
            }
            return material;
        }
    }
}
