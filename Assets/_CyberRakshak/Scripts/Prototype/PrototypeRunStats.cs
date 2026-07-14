using UnityEngine;

namespace CyberRakshak.Prototype
{
    public sealed class PrototypeRunStats : MonoBehaviour
    {
        public int WrongInteractions { get; private set; }
        public int FirewallHits { get; private set; }

        public void RecordWrongInteraction()
        {
            WrongInteractions++;
        }

        public void RecordFirewallHit()
        {
            FirewallHits++;
        }

        public string GetPrototypeRating()
        {
            int penalties = WrongInteractions + FirewallHits;

            if (penalties == 0)
            {
                return "Flawless";
            }

            if (penalties <= 2)
            {
                return "Feasible";
            }

            if (penalties <= 4)
            {
                return "Acceptable";
            }

            return "Back to Grad School";
        }
    }
}

