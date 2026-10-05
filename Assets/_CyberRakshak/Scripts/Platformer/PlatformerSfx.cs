using UnityEngine;

namespace CyberRakshak.Platformer
{
    /// <summary>Procedural low "blop" so the encounter has feedback without an unlicensed audio asset.</summary>
    public static class PlatformerSfx
    {
        private const string SfxKey = "CyberRakshak.Sfx";
        private static AudioClip blopClip;
        private static AudioClip patchVoiceClip;

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
            source.playOnAwake = false;
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
            const float duration = 0.22f;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            blopClip = AudioClip.Create("EnemyBlop", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Min(1f, t / .004f) * Mathf.Pow(1f - t / duration, 2f);
                float phase = 2f * Mathf.PI * (320f * t + .5f * (95f - 320f) * t * t / duration);
                data[i] = (Mathf.Sin(phase) + .18f * Mathf.Sin(2f * phase)) * envelope * .75f;
            }

            blopClip.SetData(data, 0);
            return blopClip;
        }

        public static void PlayPatchChatter(AudioSource source, char character)
        {
            float volume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxKey, 1f));
            if (source == null || volume <= 0f) return;
            if (patchVoiceClip == null)
            {
                const int sampleRate = 22050;
                const float duration = .065f;
                var samples = new float[Mathf.CeilToInt(sampleRate * duration)];
                for (int i = 0; i < samples.Length; i++)
                {
                    float t = i / (float)sampleRate;
                    float envelope = Mathf.Sin(Mathf.PI * t / duration);
                    float phase = 2f * Mathf.PI * 260f * t;
                    // Short, voiced robotic syllables rather than a sustained alert beep.
                    samples[i] = envelope * (.55f * Mathf.Sin(phase) + .25f * Mathf.Sin(3f * phase) +
                        .12f * Mathf.Sin(5f * phase)) * (.7f + .3f * Mathf.Sin(2f * Mathf.PI * 35f * t));
                }
                patchVoiceClip = AudioClip.Create("PatchChatter", samples.Length, 1, sampleRate, false);
                patchVoiceClip.SetData(samples, 0);
            }
            source.clip = patchVoiceClip;
            source.volume = volume * .35f;
            source.pitch = .9f + character % 7 * .07f;
            source.Play();
        }
    }
}
