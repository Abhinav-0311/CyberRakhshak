using UnityEngine;

namespace MazeGenerator
{
    /// <summary>
    /// Wall module metrics from a live bounds probe (import scale/rotation applied).
    /// Layout atom comes from the primary wall; secondary walls fit that envelope.
    /// </summary>
    public readonly struct MazeWallPrefabMetrics
    {
        const float ElongationRatio = 1.35f;

        public readonly float Length;
        public readonly float Height;
        public readonly float Thickness;
        public readonly Quaternion LengthAlignYaw;
        public readonly Vector3 PivotToBoundsCenter;

        /// <summary>
        /// Long thin module (fence): edge ribbon. Near-cube: block / voxel grid.
        /// </summary>
        public bool IsElongated => Length > Thickness * ElongationRatio;

        /// <summary>
        /// Atom size used for layout: ribbon edge length, or one block cell.
        /// </summary>
        public float ModuleSize => Length;

        public Vector3 Envelope => new Vector3(Thickness, Height, Length);

        public MazeWallPrefabMetrics(
            float length,
            float height,
            float thickness,
            Quaternion lengthAlignYaw,
            Vector3 pivotToBoundsCenter)
        {
            Length = Mathf.Max(0.05f, length);
            Height = Mathf.Max(0.05f, height);
            Thickness = Mathf.Max(0.05f, thickness);
            LengthAlignYaw = lengthAlignYaw;
            PivotToBoundsCenter = pivotToBoundsCenter;
        }
    }

    /// <summary>
    /// Placement helpers for the general wall model.
    /// Primary wall defines CellSize / envelope; secondaries scale to match that envelope.
    /// </summary>
    public static class MazePrefabTransform
    {
        const float Epsilon = 1e-4f;
        const float FitEpsilon = 0.02f;

        public static bool TryMeasureWall(GameObject wallPrefab, out MazeWallPrefabMetrics metrics)
        {
            metrics = default;
            if (wallPrefab == null)
            {
                return false;
            }

            var probe = Object.Instantiate(wallPrefab);
            probe.name = "__MazeWallProbe";
            probe.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                var t = probe.transform;
                t.SetPositionAndRotation(Vector3.zero, wallPrefab.transform.localRotation);
                t.localScale = wallPrefab.transform.localScale;

                var size0 = EncapsulateRendererBounds(probe).size;
                var lengthAlign = size0.x > size0.z + Epsilon
                    ? Quaternion.Euler(0f, 90f, 0f)
                    : Quaternion.identity;

                t.SetPositionAndRotation(Vector3.zero, lengthAlign * wallPrefab.transform.localRotation);
                t.localScale = wallPrefab.transform.localScale;

                var bounds = EncapsulateRendererBounds(probe);
                var size = bounds.size;
                var length = Mathf.Max(size.z, Epsilon);
                var thickness = Mathf.Max(size.x, Epsilon);
                var height = Mathf.Max(size.y, Epsilon);
                var pivotToCenter = bounds.center - t.position;

                metrics = new MazeWallPrefabMetrics(length, height, thickness, lengthAlign, pivotToCenter);
                return true;
            }
            finally
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(probe);
                }
                else
                {
                    Object.DestroyImmediate(probe);
                }
            }
        }

        public static void ApplyBuildMetricsToParams(MazePrefabConfig prefabs, MazeGenerationParams parameters)
        {
            if (prefabs == null || parameters == null)
            {
                return;
            }

            var wall = ResolvePrimaryWallPrefab(prefabs);
            if (!TryMeasureWall(wall, out var metrics))
            {
                parameters.CellSize = MazeGenerationParams.DefaultCellSize;
                parameters.WallHeight = MazeGenerationParams.DefaultWallHeight;
                parameters.WallThickness = MazeGenerationParams.DefaultWallThickness;
                return;
            }

            // Logical spacing between maze cell centers.
            // Ribbon: one module per edge → step = L. Block: rooms on odd voxels → step = 2L.
            parameters.CellSize = metrics.IsElongated
                ? metrics.ModuleSize
                : metrics.ModuleSize * 2f;
            parameters.WallHeight = metrics.Height;
            parameters.WallThickness = metrics.Thickness;
        }

        /// <summary>
        /// Primary wall when set; otherwise first available secondary (layout fallback).
        /// </summary>
        public static GameObject ResolvePrimaryWallPrefab(MazePrefabConfig prefabs)
        {
            if (prefabs == null)
            {
                return null;
            }

            if (prefabs.WallPrefab != null)
            {
                return prefabs.WallPrefab;
            }

            return prefabs.SecondaryWallPrefab1 != null
                ? prefabs.SecondaryWallPrefab1
                : prefabs.SecondaryWallPrefab2;
        }

        /// <summary>
        /// Orients a wall instance and scales it so its AABB matches the primary envelope.
        /// Primary (same size) keeps authored scale; secondaries are fitted.
        /// </summary>
        public static void ApplyWallTransform(
            Transform instance,
            Transform prefabRoot,
            Quaternion pieceRotation,
            MazeWallPrefabMetrics instanceMetrics,
            MazeWallPrefabMetrics primaryMetrics)
        {
            ApplyFitTransform(
                instance,
                prefabRoot,
                pieceRotation,
                instanceMetrics,
                primaryMetrics.Envelope);
        }

        /// <summary>
        /// Clear XZ footprint of a logical maze cell from primary wall metrics.
        /// Ribbon: L − T. Block: one carved module L × L.
        /// </summary>
        public static Vector3 ClearCellEnvelope(
            MazeWallPrefabMetrics primary,
            float cellSize,
            float heightFraction = 0.25f)
        {
            var footprint = primary.IsElongated
                ? Mathf.Max(0.05f, cellSize - primary.Thickness)
                : primary.ModuleSize;
            var height = Mathf.Max(0.05f, primary.Height * Mathf.Clamp01(heightFraction));
            // Piece-space envelope: X=thickness/width, Y=height, Z=length/depth.
            return new Vector3(footprint, height, footprint);
        }

        /// <summary>
        /// Orients an instance and scales its AABB to a target piece-space envelope.
        /// </summary>
        public static void ApplyFitTransform(
            Transform instance,
            Transform prefabRoot,
            Quaternion pieceRotation,
            MazeWallPrefabMetrics instanceMetrics,
            Vector3 targetEnvelope)
        {
            var localRot = instanceMetrics.LengthAlignYaw * prefabRoot.localRotation;
            instance.localRotation = pieceRotation * localRot;
            instance.localScale = FitScaleToEnvelope(
                prefabRoot.localScale,
                instanceMetrics.Envelope,
                targetEnvelope,
                localRot);
        }

        /// <summary>
        /// Snaps renderer AABB center onto a target XZ point, bottom to y = 0.
        /// </summary>
        public static void PlaceCentered(Transform instance, Vector3 targetCenterLocal)
        {
            instance.localPosition = targetCenterLocal;
            if (!TryGetRendererBounds(instance.gameObject, out var bounds))
            {
                SnapBottomToY(instance, 0f);
                return;
            }

            var deltaWorld = bounds.center - instance.position;
            var deltaLocal = instance.parent != null
                ? instance.parent.InverseTransformVector(deltaWorld)
                : deltaWorld;
            instance.localPosition = targetCenterLocal - new Vector3(deltaLocal.x, 0f, deltaLocal.z);
            SnapBottomToY(instance, 0f);
        }

        public static void SnapBottomToY(Transform instance, float floorY)
        {
            if (!TryGetRendererBounds(instance.gameObject, out var bounds))
            {
                return;
            }

            instance.position += Vector3.up * (floorY - bounds.min.y);
        }

        static Vector3 FitScaleToEnvelope(
            Vector3 authoredScale,
            Vector3 authoredEnvelope,
            Vector3 targetEnvelope,
            Quaternion localRotation)
        {
            var ratio = new Vector3(
                targetEnvelope.x / Mathf.Max(authoredEnvelope.x, Epsilon),
                targetEnvelope.y / Mathf.Max(authoredEnvelope.y, Epsilon),
                targetEnvelope.z / Mathf.Max(authoredEnvelope.z, Epsilon));

            if (NearlyUniformOne(ratio))
            {
                return authoredScale;
            }

            var localRatio = PieceScaleToPrefabLocal(ratio, localRotation);
            return Vector3.Scale(authoredScale, localRatio);
        }

        static bool NearlyUniformOne(Vector3 ratio)
        {
            return Mathf.Abs(ratio.x - 1f) <= FitEpsilon
                   && Mathf.Abs(ratio.y - 1f) <= FitEpsilon
                   && Mathf.Abs(ratio.z - 1f) <= FitEpsilon;
        }

        static Vector3 PieceScaleToPrefabLocal(Vector3 pieceScale, Quaternion localRotation)
        {
            var r = Matrix4x4.Rotate(localRotation);
            return new Vector3(
                AbsDotColumn(r, 0, pieceScale),
                AbsDotColumn(r, 1, pieceScale),
                AbsDotColumn(r, 2, pieceScale));
        }

        static float AbsDotColumn(Matrix4x4 r, int column, Vector3 v)
        {
            return Mathf.Abs(r[0, column]) * v.x
                   + Mathf.Abs(r[1, column]) * v.y
                   + Mathf.Abs(r[2, column]) * v.z;
        }

        static bool TryGetRendererBounds(GameObject go, out Bounds bounds)
        {
            bounds = default;
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers == null || renderers.Length == 0)
            {
                return false;
            }

            var any = false;
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    continue;
                }

                if (!any)
                {
                    bounds = renderers[i].bounds;
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            return any;
        }

        static Bounds EncapsulateRendererBounds(GameObject go)
        {
            if (TryGetRendererBounds(go, out var bounds))
            {
                return bounds;
            }

            return new Bounds(go.transform.position, Vector3.one);
        }
    }
}
