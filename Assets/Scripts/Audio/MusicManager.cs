// Module: Music Manager   Owner: Member 3   Purpose: Background music system for races

using UnityEngine;

namespace NeonRush
{
    /// <summary>
    /// Background music manager for race tracks
    /// </summary>
    public class MusicManager : MonoBehaviour
    {
        [Header("Music Settings")]
        [SerializeField] private float musicVolume = 0.4f;
        [SerializeField] private bool playOnStart = true;

        private AudioSource musicSource;

        private void Awake()
        {
            // Singleton pattern
            MusicManager[] managers = FindObjectsByType<MusicManager>(FindObjectsSortMode.None);
            if (managers.Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            // Create audio source
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.spatialBlend = 0f; // 2D sound
            musicSource.volume = musicVolume;
            musicSource.clip = CreateRaceMusic();

            if (playOnStart)
            {
                musicSource.Play();
            }
        }

        public void Play()
        {
            if (musicSource != null && !musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }

        public void Stop()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        public void SetVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
        }

        // Generate procedural electronic race music
        private AudioClip CreateRaceMusic()
        {
            int sampleRate = 44100;
            int samples = sampleRate * 16; // 16 second loop
            AudioClip clip = AudioClip.Create("RaceMusic", samples, 1, sampleRate, false);

            float[] data = new float[samples];
            float bpm = 140f;
            float beatDuration = 60f / bpm;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;

                // Kick drum (4/4 beat)
                float beatPhase = (t % beatDuration) / beatDuration;
                float kick = Mathf.Exp(-beatPhase * 30f) * Mathf.Sin(2 * Mathf.PI * 60f * beatPhase);
                data[i] = kick * 0.4f;

                // Bass line (repeating pattern)
                float bassFreq = 110f; // A2
                float bassPattern = t % (beatDuration * 4);
                if (bassPattern < beatDuration) bassFreq = 110f;
                else if (bassPattern < beatDuration * 2) bassFreq = 146.83f; // D3
                else if (bassPattern < beatDuration * 3) bassFreq = 123.47f; // B2
                else bassFreq = 164.81f; // E3

                data[i] += Mathf.Sin(2 * Mathf.PI * bassFreq * t) * 0.2f;

                // Synth pad (ambient)
                data[i] += Mathf.Sin(2 * Mathf.PI * 220f * t) * 0.1f; // A3
                data[i] += Mathf.Sin(2 * Mathf.PI * 329.63f * t) * 0.08f; // E4

                // Hi-hat (16th notes)
                float hihatPhase = (t % (beatDuration / 4)) / (beatDuration / 4);
                float hihat = Mathf.Exp(-hihatPhase * 50f) * (Random.value * 2f - 1f);
                data[i] += hihat * 0.05f;

                // Clamp to prevent clipping
                data[i] = Mathf.Clamp(data[i], -1f, 1f);
            }

            clip.SetData(data, 0);
            return clip;
        }
    }
}
