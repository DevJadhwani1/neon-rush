// Module: Modes   Owner: Members 1 & 2   Purpose: Base class for all game modes

using UnityEngine;

namespace NeonRush
{
    public abstract class GameModeBase : MonoBehaviour
    {
        public abstract string GetModeName();
        public abstract string GetModeDescription();
        public abstract void Initialize();
        public abstract void StartMode();
        public abstract void UpdateMode();
        public abstract bool IsComplete();
        public abstract void EndMode();
        public abstract int GetFinalScore();
        public abstract int GetCreditsEarned();
    }
}
