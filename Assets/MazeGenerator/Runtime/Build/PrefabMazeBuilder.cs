using System.Collections.Generic;
using UnityEngine;

namespace MazeGenerator
{
    /// <summary>
    /// Builds wall geometry from the final maze grid.
    /// Primary wall defines layout + size envelope; secondary walls are fitted to that envelope.
    /// Elongated primary → ribbon edges; near-cube primary → voxel blocks.
    /// </summary>
    public sealed class PrefabMazeBuilder
    {
        struct Edge
        {
            public int X;
            public int Z;
            public int OwnerX;
            public int OwnerY;
            public Vector3 Position;
            public Quaternion Rotation;
        }

        readonly Transform _root;
        readonly MazePrefabConfig _prefabs;
        readonly MazeGenerationParams _parameters;
        readonly MazeRandom _wallRandom;
        readonly Dictionary<long, Transform> _chunks = new Dictionary<long, Transform>();
        readonly Dictionary<int, MazeWallPrefabMetrics> _metricsCache = new Dictionary<int, MazeWallPrefabMetrics>();

        MazeWallPrefabMetrics _primaryMetrics;

        public PrefabMazeBuilder(Transform root, MazePrefabConfig prefabs, MazeGenerationParams parameters)
        {
            _root = root;
            _prefabs = prefabs;
            _parameters = parameters;
            _wallRandom = new MazeRandom(parameters != null ? parameters.Seed : 0);
        }

        public void Clear()
        {
            MazeEditorSafeDestroy.DestroyChildren(_root);
            _chunks.Clear();
            _metricsCache.Clear();
        }

        public void Build(MazeGrid grid)
        {
            Clear();
            if (grid == null || _prefabs == null)
            {
                return;
            }

            var primaryWall = MazePrefabTransform.ResolvePrimaryWallPrefab(_prefabs);
            if (!TryGetMetrics(primaryWall, out _primaryMetrics))
            {
                return;
            }

            if (_primaryMetrics.IsElongated)
            {
                BuildRibbon(grid);
            }
            else
            {
                BuildBlocks(grid);
            }
        }

        void BuildRibbon(MazeGrid grid)
        {
            var cellSize = _parameters.CellSize;
            var edges = new List<Edge>();
            var edgeSet = new HashSet<long>();

            foreach (var cell in grid.ActiveCells())
            {
                var center = CellCenter(cell.X, cell.Y, cellSize);

                if (cell.HasWall(WallFlags.East))
                {
                    TryAddEdge(
                        edges,
                        edgeSet,
                        cell.X + 1,
                        cell.Y,
                        cell.X,
                        cell.Y,
                        horizontal: false,
                        center + Vector3.right * (cellSize * 0.5f),
                        Quaternion.identity);
                }

                if (cell.HasWall(WallFlags.North))
                {
                    TryAddEdge(
                        edges,
                        edgeSet,
                        cell.X,
                        cell.Y + 1,
                        cell.X,
                        cell.Y,
                        horizontal: true,
                        center + Vector3.forward * (cellSize * 0.5f),
                        Quaternion.Euler(0f, 90f, 0f));
                }

                if (cell.HasWall(WallFlags.West) && !grid.IsActive(cell.X - 1, cell.Y))
                {
                    TryAddEdge(
                        edges,
                        edgeSet,
                        cell.X,
                        cell.Y,
                        cell.X,
                        cell.Y,
                        horizontal: false,
                        center + Vector3.left * (cellSize * 0.5f),
                        Quaternion.identity);
                }

                if (cell.HasWall(WallFlags.South) && !grid.IsActive(cell.X, cell.Y - 1))
                {
                    TryAddEdge(
                        edges,
                        edgeSet,
                        cell.X,
                        cell.Y,
                        cell.X,
                        cell.Y,
                        horizontal: true,
                        center + Vector3.back * (cellSize * 0.5f),
                        Quaternion.Euler(0f, 90f, 0f));
                }
            }

            foreach (var edge in edges)
            {
                SpawnWall(edge.OwnerX, edge.OwnerY, edge.Position, edge.Rotation);
            }
        }

        void BuildBlocks(MazeGrid grid)
        {
            var s = _primaryMetrics.ModuleSize;
            var bw = grid.Width * 2 + 1;
            var bh = grid.Height * 2 + 1;
            var solid = new bool[bw, bh];
            for (var i = 0; i < bw; i++)
            {
                for (var j = 0; j < bh; j++)
                {
                    solid[i, j] = true;
                }
            }

            foreach (var cell in grid.ActiveCells())
            {
                var ix = cell.X * 2 + 1;
                var iz = cell.Y * 2 + 1;
                solid[ix, iz] = false;

                if (!cell.HasWall(WallFlags.East) && ix + 1 < bw)
                {
                    solid[ix + 1, iz] = false;
                }

                if (!cell.HasWall(WallFlags.North) && iz + 1 < bh)
                {
                    solid[ix, iz + 1] = false;
                }

                if (!cell.HasWall(WallFlags.West) && ix - 1 >= 0)
                {
                    solid[ix - 1, iz] = false;
                }

                if (!cell.HasWall(WallFlags.South) && iz - 1 >= 0)
                {
                    solid[ix, iz - 1] = false;
                }
            }

            // Drop fill outside the active shape (circle / mask); keep only voxels
            // that belong to the 3×3 neighborhood of at least one active cell.
            for (var i = 0; i < bw; i++)
            {
                for (var j = 0; j < bh; j++)
                {
                    if (!solid[i, j] || VoxelTouchesActiveCell(i, j, grid))
                    {
                        continue;
                    }

                    solid[i, j] = false;
                }
            }

            for (var i = 0; i < bw; i++)
            {
                for (var j = 0; j < bh; j++)
                {
                    if (!solid[i, j])
                    {
                        continue;
                    }

                    SpawnWall(
                        i,
                        j,
                        new Vector3(i * s, 0f, j * s),
                        Quaternion.identity);
                }
            }
        }

        /// <summary>
        /// True when voxel (i,j) lies in the 3×3 block neighborhood of an active maze cell.
        /// </summary>
        static bool VoxelTouchesActiveCell(int i, int j, MazeGrid grid)
        {
            var xMin = Mathf.Max(0, (i - 2) / 2);
            var xMax = Mathf.Min(grid.Width - 1, i / 2);
            var yMin = Mathf.Max(0, (j - 2) / 2);
            var yMax = Mathf.Min(grid.Height - 1, j / 2);

            for (var x = xMin; x <= xMax; x++)
            {
                for (var y = yMin; y <= yMax; y++)
                {
                    if (!grid.IsActive(x, y))
                    {
                        continue;
                    }

                    var ix = x * 2;
                    var iz = y * 2;
                    if (i >= ix && i <= ix + 2 && j >= iz && j <= iz + 2)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static void TryAddEdge(
            List<Edge> edges,
            HashSet<long> edgeSet,
            int x,
            int z,
            int ownerX,
            int ownerY,
            bool horizontal,
            Vector3 position,
            Quaternion rotation)
        {
            var key = EdgeKey(x, z, horizontal);
            if (!edgeSet.Add(key))
            {
                return;
            }

            edges.Add(new Edge
            {
                X = x,
                Z = z,
                OwnerX = ownerX,
                OwnerY = ownerY,
                Position = position,
                Rotation = rotation
            });
        }

        void SpawnWall(int chunkX, int chunkY, Vector3 position, Quaternion pieceRotation)
        {
            var prefab = _prefabs.PickWallPrefab(_wallRandom);
            if (prefab == null || _root == null || !TryGetMetrics(prefab, out var instanceMetrics))
            {
                return;
            }

            var parent = GetChunkParent(chunkX, chunkY);
            var instance = Object.Instantiate(prefab, parent);
            MazePrefabTransform.ApplyWallTransform(
                instance.transform,
                prefab.transform,
                pieceRotation,
                instanceMetrics,
                _primaryMetrics);
            MazePrefabTransform.PlaceCentered(instance.transform, position);

            if (_parameters.MarkBuildStatic)
            {
                ApplyStaticFlags(instance);
            }
        }

        bool TryGetMetrics(GameObject prefab, out MazeWallPrefabMetrics metrics)
        {
            metrics = default;
            if (prefab == null)
            {
                return false;
            }

            var id = prefab.GetInstanceID();
            if (_metricsCache.TryGetValue(id, out metrics))
            {
                return true;
            }

            if (!MazePrefabTransform.TryMeasureWall(prefab, out metrics))
            {
                return false;
            }

            _metricsCache[id] = metrics;
            return true;
        }

        Transform GetChunkParent(int cellX, int cellY)
        {
            var size = Mathf.Max(1, _parameters.BuildChunkSize);
            var cx = cellX >= 0 ? cellX / size : (cellX - size + 1) / size;
            var cy = cellY >= 0 ? cellY / size : (cellY - size + 1) / size;
            var key = ((long)cx << 32) ^ (uint)cy;
            if (_chunks.TryGetValue(key, out var existing))
            {
                return existing;
            }

            var go = new GameObject($"Chunk_{cx}_{cy}");
            go.transform.SetParent(_root, false);
            if (_parameters.MarkBuildStatic)
            {
                ApplyStaticFlags(go);
            }

            _chunks[key] = go.transform;
            return go.transform;
        }

        static void ApplyStaticFlags(GameObject go)
        {
            go.isStatic = true;
        }

        static Vector3 CellCenter(int x, int y, float cellSize)
        {
            return new Vector3(x * cellSize + cellSize * 0.5f, 0f, y * cellSize + cellSize * 0.5f);
        }

        static long EdgeKey(int x, int z, bool horizontal)
        {
            unchecked
            {
                var ux = (uint)(x + 512);
                var uz = (uint)(z + 512);
                var h = horizontal ? 1u : 0u;
                return ((long)h << 40) | ((long)ux << 20) | uz;
            }
        }
    }
}
