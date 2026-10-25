// Module: FX   Owner: Member 3   Purpose: Procedural particle effects library

using UnityEngine;

namespace NeonRush
{
    public class VFXLibrary : MonoBehaviour
    {
        public static GameObject CreateBoostTrail(Transform parent)
        {
            GameObject trail = new GameObject("BoostTrail");
            trail.transform.SetParent(parent, false);
            trail.transform.localPosition = new Vector3(0f, 0.2f, -2f);
            
            ParticleSystem ps = trail.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.8f;
            main.startSpeed = 2f;
            main.startSize = 0.5f;
            main.startColor = new Color(0f, 0.9f, 1f);
            main.maxParticles = 100;
            
            var emission = ps.emission;
            emission.rateOverTime = 50;
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.1f;
            
            return trail;
        }

        public static GameObject CreateExplosion(Vector3 position)
        {
            GameObject explosion = new GameObject("Explosion");
            explosion.transform.position = position;
            
            ParticleSystem ps = explosion.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1f;
            main.startSpeed = 8f;
            main.startSize = 0.5f;
            main.startColor = new Color(1f, 0.5f, 0f);
            main.maxParticles = 100;
            
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 100) });
            
            ps.Play();
            Object.Destroy(explosion, 2f);
            
            return explosion;
        }

        public static GameObject CreateSpeedLines(Transform parent)
        {
            GameObject lines = new GameObject("SpeedLines");
            lines.transform.SetParent(parent, false);
            lines.transform.localPosition = new Vector3(0f, 0f, 5f);
            
            ParticleSystem ps = lines.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.3f;
            main.startSpeed = 30f;
            main.startSize = 0.1f;
            main.startColor = new Color(1f, 1f, 1f, 0.5f);
            main.maxParticles = 50;
            
            var emission = ps.emission;
            emission.rateOverTime = 100;
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(20f, 10f, 1f);
            
            return lines;
        }

        public static GameObject CreateCollectEffect(Vector3 position)
        {
            GameObject effect = new GameObject("CollectEffect");
            effect.transform.position = position;
            
            ParticleSystem ps = effect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 3f;
            main.startSize = 0.3f;
            main.startColor = Color.yellow;
            main.maxParticles = 20;
            
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });
            
            ps.Play();
            Object.Destroy(effect, 1f);
            
            return effect;
        }
    }
}
