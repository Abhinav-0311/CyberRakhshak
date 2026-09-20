using UnityEngine;

namespace CyberRakshak.Platformer
{
    /// <summary>Procedural low "blop" so the encounter has feedback without an unlicensed audio asset.</summary>
    public static class PlatformerSfx
    {
        private const string SfxKey = "CyberRakshak.Sfx";
        private static AudioClip blopClip;

        public static void PlayBlop(Vector3 position)
        {
            float volume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxKey, 1f));
            if (volume <= 0f)
            {
                return;
            }

            AudioClip clip = GetBlopClip();
            GameObject emitter = new GameObject("EnemyBlopSfx");
            emitter.transform.position = position;
            AudioSource source = emitter.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = 0f;
            source.volume = volume * .9f;
            source.Play();
            Object.Destroy(emitter, clip.length + .05f);
        }

        private static AudioClip GetBlopClip()
        {
            if (blopClip != null)
            {
                return blopClip;
            }

            const int sampleRate = 22050;
            const float duration = 0.16f;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            blopClip = AudioClip.Create("EnemyBlop", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - (t / duration);
                float frequency = Mathf.Lerp(185f, 78f, t / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.55f;
            }

            blopClip.SetData(data, 0);
            return blopClip;
        }
    }
}
