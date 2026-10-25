// Module: Ship Audio   Owner: Member 3   Purpose: Engine, boost, and impact sounds for ships

using UnityEngine;

namespace NeonRush
{
    /// <summary>
    /// Audio system for ship engine, boost, and collision sounds
    /// </summary>
    [RequireComponent(typeof(ShipController))]
    public class ShipAudio : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource engineSource;
        [SerializeField] private AudioSource boostSource;
        [SerializeField] private AudioSource impactSource;

        [Header("Engine Settings")]
        [SerializeField] private float minEnginePitch = 0.8f;
        [SerializeField] private float maxEnginePitch = 2.0f;
        [SerializeField] private float engineVolume = 0.5f;

        [Header("Boost Settings")]
        [SerializeField] private float boostVolume = 0.7f;
        [SerializeField] private float boostPitch = 1.2f;

        [Header("Impact Settings")]
        [SerializeField] private float impactVolume = 0.6f;
        [SerializeField] private float minImpactVelocity = 5f;

        private ShipController shipController;
        private Rigidbody rb;

        private void Awake()
        {
            shipController = GetComponent<ShipController>();
            rb = GetComponent<Rigidbody>();

            // Create audio sources if not assigned
            if (engineSource == null)
            {
                engineSource = gameObject.AddComponent<AudioSource>();
                engineSource.loop = true;
                engineSource.spatialBlend = 0.8f; // 3D sound
                engineSource.minDistance = 5f;
                engineSource.maxDistance = 50f;
                engineSource.volume = engineVolume;
                engineSource.clip = CreateEngineSound();
                engineSource.Play();
            }

            if (boostSource == null)
            {
                boostSource = gameObject.AddComponent<AudioSource>();
                boostSource.loop = false;
                boostSource.spatialBlend = 0.8f;
                boostSource.minDistance = 5f;
                boostSource.maxDistance = 50f;
                boostSource.volume = boostVolume;
                boostSource.pitch = boostPitch;
                boostSource.clip = CreateBoostSound();
            }

            if (impactSource == null)
            {
                impactSource = gameObject.AddComponent<AudioSource>();
                impactSource.loop = false;
                impactSource.spatialBlend = 1.0f;
                impactSource.minDistance = 3f;
                impactSource.maxDistance = 30f;
                impactSource.volume = impactVolume;
                impactSource.clip = CreateImpactSound();
            }
        }

        private void Update()
        {
            UpdateEngineSound();
        }

        private void UpdateEngineSound()
        {
            if (engineSource == null || rb == null) return;

            // Pitch based on speed
            float speed = rb.linearVelocity.magnitude;
            float maxSpeed = 100f;
            float pitchT = Mathf.Clamp01(speed / maxSpeed);
            engineSource.pitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, pitchT);

            // Volume based on speed (softer when stopped)
            float volumeT = Mathf.Clamp01(speed / 10f);
            engineSource.volume = Mathf.Lerp(engineVolume * 0.3f, engineVolume, volumeT);
        }

        public void PlayBoostSound()
        {
            if (boostSource != null && !boostSource.isPlaying)
            {
                boostSource.Play();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (impactSource == null) return;

            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed > minImpactVelocity)
            {
                float volumeT = Mathf.Clamp01((impactSpeed - minImpactVelocity) / 20f);
                impactSource.volume = impactVolume * volumeT;
                impactSource.Play();
            }
        }

        // Generate procedural engine sound (low rumble)
        private AudioClip CreateEngineSound()
        {
            int sampleRate = 44100;
            int samples = sampleRate * 2; // 2 second loop
            AudioClip clip = AudioClip.Create("EngineSound", samples, 1, sampleRate, false);

            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                // Low frequency rumble (60-120 Hz)
                float t = (float)i / sampleRate;
                data[i] = Mathf.Sin(2 * Mathf.PI * 80f * t) * 0.3f;
                data[i] += Mathf.Sin(2 * Mathf.PI * 120f * t) * 0.2f;
                data[i] += Mathf.Sin(2 * Mathf.PI * 60f * t) * 0.15f;
                
                // Add some noise
                data[i] += (Random.value * 2f - 1f) * 0.05f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        // Generate procedural boost sound (high whoosh)
        private AudioClip CreateBoostSound()
        {
            int sampleRate = 44100;
            int samples = sampleRate; // 1 second
            AudioClip clip = AudioClip.Create("BoostSound", samples, 1, sampleRate, false);

            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 3f); // Decay envelope

                // Sweep from 200Hz to 800Hz
                float freq = Mathf.Lerp(200f, 800f, t);
                data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope * 0.5f;
                
                // Add noise
                data[i] += (Random.value * 2f - 1f) * envelope * 0.2f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        // Generate procedural impact sound (sharp thud)
        private AudioClip CreateImpactSound()
        {
            int sampleRate = 44100;
            int samples = sampleRate / 4; // 0.25 seconds
            AudioClip clip = AudioClip.Create("ImpactSound", samples, 1, sampleRate, false);

            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 20f); // Fast decay

                // Low thud (100-300 Hz)
                data[i] = Mathf.Sin(2 * Mathf.PI * 200f * t) * envelope;
                data[i] += Mathf.Sin(2 * Mathf.PI * 150f * t) * envelope * 0.5f;
                
                // Metallic ring
                data[i] += Mathf.Sin(2 * Mathf.PI * 2000f * t) * envelope * 0.3f;
            }

            clip.SetData(data, 0);
            return clip;
        }
    }
}
