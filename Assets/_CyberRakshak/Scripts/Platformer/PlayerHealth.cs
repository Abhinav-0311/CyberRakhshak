using UnityEngine;
using UnityEngine.SceneManagement;

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
        private bool restarting;
        private float pendingBurnDamage;

        private void Update()
        {
            if (!restarting && transform.position.y < -8f)
            {
                RestartLevel();
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            hit.collider.GetComponentInParent<PlatformerEnemy>()?.TryResolvePlayerContact(GetComponent<CharacterController>());
        }

        public bool TakeHit(int damage)
        {
            if (damage <= 0 || IsDefeated || restarting || Time.time < nextDamageTime)
            {
                return false;
            }

            nextDamageTime = Time.time + contactInvulnerabilitySeconds;
            ApplyDamage(damage);
            return true;
        }

        /// <summary>Continuous fire damage is independent of enemy-contact invulnerability.</summary>
        public void TakeBurnDamage(float damage)
        {
            if (damage <= 0f || float.IsNaN(damage) || float.IsInfinity(damage) || IsDefeated || restarting) return;
            pendingBurnDamage += damage;
            int wholeDamage = Mathf.FloorToInt(Mathf.Min(pendingBurnDamage, CurrentHealth));
            if (wholeDamage == 0) return;
            pendingBurnDamage -= wholeDamage;
            ApplyDamage(wholeDamage);
        }

        private void ApplyDamage(int damage)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            PlatformerHud.Instance?.SetHealth(CurrentHealth, MaxHealth);
            if (CurrentHealth == 0)
            {
                RestartLevel();
            }
        }

        private void RestartLevel()
        {
            if (restarting)
            {
                return;
            }

            restarting = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
