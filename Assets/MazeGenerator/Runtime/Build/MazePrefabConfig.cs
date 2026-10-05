using System;
using UnityEngine;

namespace MazeGenerator
{
    /// <summary>
    /// Prefab catalogue used by <see cref="PrefabMazeBuilder"/> and debug solve visualization.
    /// Wall variants are chosen per wall segment using percentages that always sum to 100.
    /// </summary>
    [Serializable]
    public sealed class MazePrefabConfig
    {
        [Header("Walls")]
        [Tooltip("Primary wall. Defines maze spacing and the size envelope; secondary walls are scaled to match.")]
        public GameObject WallPrefab;
        [Range(0, 100), Tooltip("Share of the primary wall prefab. The three wall % always sum to 100.")]
        public int WallPercent = 100;

        public GameObject SecondaryWallPrefab1;
        [Range(0, 100), Tooltip("Share of secondary wall prefab 1. Fitted to the primary wall size. The three wall % always sum to 100.")]
        public int SecondaryWall1Percent = 0;

        public GameObject SecondaryWallPrefab2;
        [Range(0, 100), Tooltip("Share of secondary wall prefab 2. Fitted to the primary wall size. The three wall % always sum to 100.")]
        public int SecondaryWall2Percent = 0;

        [Header("Debug Solve (A*)")]
        [Tooltip("Marker on the solution path. Scaled to the clear cell space from the primary wall (L−T ribbon, or L×L block).")]
        public GameObject DebugSolutionPrefab;

        [SerializeField, HideInInspector] int _lastWallPercent = 100;
        [SerializeField, HideInInspector] int _lastSecondaryWall1Percent;
        [SerializeField, HideInInspector] int _lastSecondaryWall2Percent;

        /// <summary>
        /// Returns an error message when required build prefabs are missing; otherwise null.
        /// </summary>
        public string ValidateRequired()
        {
            if (WallPrefab == null && SecondaryWallPrefab1 == null && SecondaryWallPrefab2 == null)
            {
                return "At least one Wall Prefab is required.";
            }

            return null;
        }

        /// <summary>
        /// Clamps and rebalances wall percentages so they always sum to 100.
        /// Call from a MonoBehaviour <c>OnValidate</c>.
        /// </summary>
        public void Validate()
        {
            WallPercent = Mathf.Clamp(WallPercent, 0, 100);
            SecondaryWall1Percent = Mathf.Clamp(SecondaryWall1Percent, 0, 100);
            SecondaryWall2Percent = Mathf.Clamp(SecondaryWall2Percent, 0, 100);

            var changed = -1;
            if (WallPercent != _lastWallPercent)
            {
                changed = 0;
            }
            else if (SecondaryWall1Percent != _lastSecondaryWall1Percent)
            {
                changed = 1;
            }
            else if (SecondaryWall2Percent != _lastSecondaryWall2Percent)
            {
                changed = 2;
            }

            if (changed >= 0)
            {
                Rebalance(changed);
            }
            else if (WallPercent + SecondaryWall1Percent + SecondaryWall2Percent != 100)
            {
                NormalizeTo100();
            }

            _lastWallPercent = WallPercent;
            _lastSecondaryWall1Percent = SecondaryWall1Percent;
            _lastSecondaryWall2Percent = SecondaryWall2Percent;
        }

        /// <summary>
        /// Picks a wall prefab using the configured percentages (null entries are ignored).
        /// </summary>
        public GameObject PickWallPrefab(MazeRandom random)
        {
            var w0 = WallPrefab != null ? Mathf.Max(0, WallPercent) : 0;
            var w1 = SecondaryWallPrefab1 != null ? Mathf.Max(0, SecondaryWall1Percent) : 0;
            var w2 = SecondaryWallPrefab2 != null ? Mathf.Max(0, SecondaryWall2Percent) : 0;
            var total = w0 + w1 + w2;

            if (total <= 0)
            {
                return WallPrefab != null
                    ? WallPrefab
                    : SecondaryWallPrefab1 != null
                        ? SecondaryWallPrefab1
                        : SecondaryWallPrefab2;
            }

            var roll = (random != null ? random.NextFloat() : 0f) * total;
            if (roll < w0)
            {
                return WallPrefab;
            }

            roll -= w0;
            if (roll < w1)
            {
                return SecondaryWallPrefab1;
            }

            return SecondaryWallPrefab2 != null ? SecondaryWallPrefab2 : WallPrefab;
        }

        void Rebalance(int changedIndex)
        {
            var values = new[] { WallPercent, SecondaryWall1Percent, SecondaryWall2Percent };
            var last = new[] { _lastWallPercent, _lastSecondaryWall1Percent, _lastSecondaryWall2Percent };

            values[changedIndex] = Mathf.Clamp(values[changedIndex], 0, 100);
            var remaining = 100 - values[changedIndex];

            var a = (changedIndex + 1) % 3;
            var b = (changedIndex + 2) % 3;
            var lastSum = last[a] + last[b];

            if (lastSum <= 0)
            {
                values[a] = remaining;
                values[b] = 0;
            }
            else
            {
                values[a] = Mathf.RoundToInt(remaining * (last[a] / (float)lastSum));
                values[a] = Mathf.Clamp(values[a], 0, remaining);
                values[b] = remaining - values[a];
            }

            WallPercent = values[0];
            SecondaryWall1Percent = values[1];
            SecondaryWall2Percent = values[2];
        }

        void NormalizeTo100()
        {
            var total = WallPercent + SecondaryWall1Percent + SecondaryWall2Percent;
            if (total <= 0)
            {
                WallPercent = 100;
                SecondaryWall1Percent = 0;
                SecondaryWall2Percent = 0;
                return;
            }

            var w0 = Mathf.RoundToInt(100f * WallPercent / total);
            var w1 = Mathf.RoundToInt(100f * SecondaryWall1Percent / total);
            w0 = Mathf.Clamp(w0, 0, 100);
            w1 = Mathf.Clamp(w1, 0, 100 - w0);
            var w2 = 100 - w0 - w1;

            WallPercent = w0;
            SecondaryWall1Percent = w1;
            SecondaryWall2Percent = w2;
        }
    }
}
