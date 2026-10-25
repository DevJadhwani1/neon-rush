// Module: Audio   Owner: Member 3   Purpose: Centralized audio playback and music management

using UnityEngine;
using System.Collections.Generic;

namespace NeonRush
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        private Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private AudioSource musicSource;
        private AudioSource sfxSource;
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            SetupAudioSources();
            GenerateAllClips();
        }

        void SetupAudioSources()
        {
            // Music source
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.4f;
            musicSource.priority = 0;
            
            // SFX source
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.volume = 0.7f;
            sfxSource.priority = 128;
        }

        void GenerateAllClips()
        {
            clips["engine"] = ProceduralAudio.CreateEngineLoop();
            clips["boost"] = ProceduralAudio.CreateBoostSound();
            clips["explosion"] = ProceduralAudio.CreateExplosion();
            clips["collect"] = ProceduralAudio.CreateCollectSound();
            
            Debug.Log("[AudioManager] Generated " + clips.Count + " procedural audio clips");
        }

        public void PlaySFX(string clipName)
        {
            if (clips.ContainsKey(clipName))
            {
                sfxSource.PlayOneShot(clips[clipName]);
            }
            else
            {
                Debug.LogWarning("[AudioManager] Clip not found: " + clipName);
            }
        }

        public void PlayMusic(string clipName)
        {
            if (clips.ContainsKey(clipName))
            {
                musicSource.clip = clips[clipName];
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void SetMusicVolume(float volume)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }

        public void SetSFXVolume(float volume)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }
}
