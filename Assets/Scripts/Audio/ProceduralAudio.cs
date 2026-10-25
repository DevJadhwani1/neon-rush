// Module: Audio   Owner: Member 3   Purpose: Generate audio clips procedurally in code

using UnityEngine;

namespace NeonRush
{
    public class ProceduralAudio : MonoBehaviour
    {
        public static AudioClip CreateEngineLoop(float frequency = 220f, int sampleRate = 22050, float duration = 2f)
        {
            int sampleCount = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float wave = Mathf.Sin(2f * Mathf.PI * frequency * t);
                float harmonic = Mathf.Sin(2f * Mathf.PI * frequency * 2f * t) * 0.3f;
                samples[i] = (wave + harmonic) * 0.3f;
            }
            
            AudioClip clip = AudioClip.Create("EngineLoop", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip CreateBoostSound(int sampleRate = 22050, float duration = 0.8f)
        {
            int sampleCount = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float freq = Mathf.Lerp(200f, 800f, t); // Sweep up
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float envelope = 1f - t; // Fade out
                samples[i] = wave * envelope * 0.5f;
            }
            
            AudioClip clip = AudioClip.Create("BoostSound", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip CreateExplosion(int sampleRate = 22050, float duration = 1.2f)
        {
            int sampleCount = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float noise = Random.Range(-1f, 1f);
                float envelope = Mathf.Exp(-5f * t); // Sharp decay
                samples[i] = noise * envelope * 0.6f;
            }
            
            AudioClip clip = AudioClip.Create("Explosion", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip CreateCollectSound(int sampleRate = 22050)
        {
            float duration = 0.3f;
            int sampleCount = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float freq = 880f + (t * 440f); // Rising pitch
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float envelope = 1f - t;
                samples[i] = wave * envelope * 0.4f;
            }
            
            AudioClip clip = AudioClip.Create("CollectSound", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
