using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    // Audio cues silenced to prevent harsh/annoying buzzing sounds
    public void PlayAntiGravityActive() { }
    public void PlayHazardHit() { }
    public void PlayEnergyPickup() { }
    public void PlayVictorySound() { }
    public void PlayMiniTurbo() { }
    public void PlayShardCollect() { }
    public void PlayCountdownBeep(bool isGo = false) { }
}
