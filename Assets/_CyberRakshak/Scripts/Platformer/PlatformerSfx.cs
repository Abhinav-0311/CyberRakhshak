using UnityEngine;

namespace CyberRakshak.Platformer
{
    /// <summary>Procedural low "blop" so the encounter has feedback without an unlicensed audio asset.</summary>
    public static class PlatformerSfx
    {
        public static void PlayBlop(Vector3 position)
        {
            const int sampleRate = 22050;
            const float duration = 0.16f;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            AudioClip clip = AudioClip.Create("EnemyBlop", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - (t / duration);
                float frequency = Mathf.Lerp(185f, 78f, t / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.55f;
            }

            clip.SetData(data, 0);
            AudioSource.PlayClipAtPoint(clip, position, 0.8f);
        }
    }
}
