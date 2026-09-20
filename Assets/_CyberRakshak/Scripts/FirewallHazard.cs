using UnityEngine;
using System.Collections;

namespace CyberRakshak
{
    public class FirewallHazard : MonoBehaviour
    {
        [Tooltip("The time in seconds it takes to extinguish the fire after water hits it.")]
        public float extinguishDelay = 2.0f;

        [Tooltip("The particle systems to stop when extinguished.")]
        public ParticleSystem[] fireParticles;
        
        private bool isExtinguished = false;
        private Collider hazardCollider;
        private ParticleSystem denseFirewall;
        private GameObject denseFirewallRoot;
        private AudioSource[] fireAudio;
        private static Material firewallParticleMaterial;

        private void Awake()
        {
            hazardCollider = GetComponent<Collider>();
            if (fireParticles == null || fireParticles.Length == 0)
            {
                fireParticles = GetComponentsInChildren<ParticleSystem>();
            }

            fireAudio = GetComponentsInChildren<AudioSource>();
            // Restored review-approved firewall treatment: dense layers use the authored fire material.
            DisableLegacyFireParticles();
            CreateDenseFirewallVisual();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isExtinguished) return;

            // Check if the object is water
            if (other.GetComponent<WaterObject>() != null || other.CompareTag("Water"))
            {
                BeginExtinguish();
            }
        }

        /// <summary>Allows an authored lever/water sequence to clear this gate without particle-collider coupling.</summary>
        public void BeginExtinguish()
        {
            if (!isExtinguished)
            {
                StartCoroutine(ExtinguishRoutine());
            }
        }

        private void DisableLegacyFireParticles()
        {
            foreach (ParticleSystem particleSystem in fireParticles)
            {
                if (particleSystem != null)
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ParticleSystemRenderer legacyRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                    if (legacyRenderer != null)
                    {
                        legacyRenderer.enabled = false;
                    }
                }
            }
        }

        private void CreateDenseFirewallVisual()
        {
            if (hazardCollider == null || denseFirewall != null)
            {
                return;
            }

            Bounds gateBounds = hazardCollider.bounds;
            denseFirewallRoot = new GameObject("DenseFirewallVisual");
            denseFirewallRoot.transform.position = new Vector3(
                gateBounds.center.x,
                gateBounds.min.y + 0.08f,
                gateBounds.center.z);
            denseFirewallRoot.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

            // Two smaller layers look like rising flame rather than a row of large fireballs.
            // Longer lifetime plus higher vertical start speed creates a readable wall, not a ground-level strip.
            denseFirewall = CreateFlameLayer("FirewallCore", 0.5f, 1.15f, 2.7f, 4.0f, 3.0f, 5.8f, 68f, 2300, gateBounds);
            CreateFlameLayer("FirewallCrest", 0.75f, 1.5f, 3.0f, 4.5f, 4.2f, 7.6f, 34f, 1400, gateBounds);
            denseFirewall.Play();
        }

        private ParticleSystem CreateFlameLayer(
            string layerName,
            float minimumSize,
            float maximumSize,
            float minimumLifetime,
            float maximumLifetime,
            float minimumSpeed,
            float maximumSpeed,
            float particlesPerUnit,
            int maximumParticles,
            Bounds gateBounds)
        {
            GameObject layerObject = new GameObject(layerName);
            layerObject.transform.SetParent(denseFirewallRoot.transform, false);
            ParticleSystem particleSystem = layerObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particleSystem.main;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(minimumLifetime, maximumLifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(minimumSpeed, maximumSpeed);
            main.startSize = new ParticleSystem.MinMaxCurve(minimumSize, maximumSize);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.2f, 0.015f, 0.95f), new Color(1f, 0.82f, 0.08f, 1f));
            main.maxParticles = maximumParticles;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = Mathf.Clamp(gateBounds.size.x * particlesPerUnit, 260f, maximumParticles);

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            // Emit in a narrow strip at floor level; the rotated emitter makes start speed rise in world Y.
            shape.scale = new Vector3(gateBounds.size.x, 0.2f, 0.18f);

            ParticleSystem.NoiseModule noise = particleSystem.noise;
            noise.enabled = true;
            noise.strength = 0.42f;
            noise.frequency = 0.65f;
            noise.scrollSpeed = 0.9f;

            ParticleSystem.ColorOverLifetimeModule colourOverLifetime = particleSystem.colorOverLifetime;
            colourOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.96f, 0.2f), 0f),
                    new GradientColorKey(new Color(1f, 0.18f, 0.01f), 0.5f),
                    new GradientColorKey(new Color(0.45f, 0.01f, 0f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.12f),
                    new GradientAlphaKey(1f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                });
            colourOverLifetime.color = gradient;

            ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            ParticleSystem source = fireParticles != null && fireParticles.Length > 0 ? fireParticles[0] : null;
            ParticleSystemRenderer sourceRenderer = source != null ? source.GetComponent<ParticleSystemRenderer>() : null;
            if (sourceRenderer != null && sourceRenderer.sharedMaterial != null)
            {
                renderer.sharedMaterial = sourceRenderer.sharedMaterial;
            }
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            particleSystem.Play();
            return particleSystem;
        }

        private static Material GetCompatibleParticleMaterial()
        {
            if (firewallParticleMaterial != null)
            {
                return firewallParticleMaterial;
            }

            Shader shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            if (shader == null)
            {
                shader = Shader.Find("Particles/Standard Unlit");
            }

            if (shader == null)
            {
                Debug.LogWarning("Firewall visual: no compatible built-in particle shader was found.");
                return null;
            }

            firewallParticleMaterial = new Material(shader)
            {
                name = "RuntimeFirewallParticleMaterial"
            };
            firewallParticleMaterial.color = Color.white;
            firewallParticleMaterial.mainTexture = CreateSoftFlameTexture();
            return firewallParticleMaterial;
        }

        private static Texture2D CreateSoftFlameTexture()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "RuntimeSoftFlameTexture",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            for (int y = 0; y < size; y++)
            {
                float vertical = y / (float)(size - 1);
                // Wide bright base tapering to a narrow transparent tip: this turns every billboard into a flame tongue.
                float halfWidth = Mathf.Lerp(.44f, .035f, vertical);
                float horizontal = Mathf.Abs((y * .07f + 0.5f) % 1f - 0.5f); // a subtle non-uniform taper
                for (int x = 0; x < size; x++)
                {
                    horizontal = Mathf.Abs(x / (float)(size - 1) - 0.5f);
                    float across = Mathf.Clamp01(1f - horizontal / halfWidth);
                    float alpha = across * across * (1f - Mathf.SmoothStep(.78f, 1f, vertical));
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            return texture;
        }

        private IEnumerator ExtinguishRoutine()
        {
            isExtinguished = true;
            Debug.Log("Firewall is being extinguished...");

            yield return new WaitForSeconds(extinguishDelay);

            // Stop emitting fire
            foreach (var ps in fireParticles)
            {
                if (ps != null)
                {
                    var emission = ps.emission;
                    emission.enabled = false;
                }
            }

            if (denseFirewallRoot != null)
            {
                denseFirewallRoot.SetActive(false);
            }

            foreach (AudioSource audioSource in fireAudio)
            {
                if (audioSource != null)
                {
                    audioSource.Stop();
                }
            }

            // Disable the collider so the player can pass through
            if (hazardCollider != null)
            {
                hazardCollider.enabled = false;
            }

            Debug.Log("Firewall extinguished. Path is clear.");
        }
    }
}
