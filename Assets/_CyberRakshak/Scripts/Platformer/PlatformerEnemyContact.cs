using UnityEngine;

namespace CyberRakshak.Platformer
{
    /// <summary>Routes the child contact trigger to its owning SpaceMan.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class PlatformerEnemyContact : MonoBehaviour
    {
        PlatformerEnemy owner;

        public void Configure(PlatformerEnemy enemy)
        {
            owner = enemy;
        }

        void OnTriggerEnter(Collider other)
        {
            owner?.TryResolvePlayerContact(other);
        }

        void OnTriggerStay(Collider other)
        {
            owner?.TryResolvePlayerContact(other);
        }
    }
}
