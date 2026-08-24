using UnityEngine;

namespace CyberRakshak.Platformer
{
    /// <summary>Owns the player's small, review-friendly health contract for the firewall encounter.</summary>
    public sealed class PlayerHealth : MonoBehaviour
    {
        public const int MaxHealth = 100;

        [SerializeField] private float contactInvulnerabilitySeconds = 0.8f;

        public int CurrentHealth { get; private set; } = MaxHealth;
        public float NormalizedHealth => CurrentHealth / (float)MaxHealth;
        public bool IsDefeated => CurrentHealth <= 0;

        private float nextDamageTime;

        public bool TakeHit(int damage)
        {
            if (IsDefeated || Time.time < nextDamageTime)
            {
                return false;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            nextDamageTime = Time.time + contactInvulnerabilitySeconds;
            PlatformerHud.Instance?.SetHealth(CurrentHealth, MaxHealth);
            Debug.Log($"Firewall contact: -{damage} HP. Remaining health: {CurrentHealth}.");
            return true;
        }
    }
}
