using System;
using System.Collections.Generic;
using PCG.Terrain.Common.Extensions;
using PCG.Terrain.Common.Grid;
using PCG.Terrain.Common.Jobs;
using PCG.Terrain.Common.Memory;
using PCG.Terrain.Core.Components;
using PCG.Terrain.Settings;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace PCG.Terrain.Core.Systems
{
    public sealed class CreateSystem : BaseSystem
    {
        private RenderMesh _renderMesh;
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<int> _triangles = new List<int>();

        private EntityArchetype _entityArchetypeNoise;
        private EntityArchetype _entityArchetypeMesh;

        #region Jobs

        [BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
        private struct MeshCalculationGrid : IJobParallelForGrid, IDisposable
        {
            [WriteOnly] public NativeArray<float3> Vertices;

            [NativeDisableParallelForRestriction, WriteOnly]
            public NativeArray<int> Triangles;

            private readonly float _stepSize;
            private readonly int _resolution;
            public int GridSize { get; }

            public MeshCalculationGrid(int resolution, float stepSize, int verticesCount, int trianglesCount)
            {
                _resolution = resolution;
                _stepSize = stepSize;
                GridSize = verticesCount;
                Vertices = new NativeArray<float3>(verticesCount, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);
                Triangles = new NativeArray<int>(trianglesCount, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);
            }

            public void Execute(int index)
            {
                MeshCreator.GridData(ref Vertices, ref Triangles, index, _resolution, _stepSize);
            }

            public void Dispose()
            {
                Vertices.Dispose();
                Triangles.Dispose();
            }
        }

        #endregion

        public CreateSystem(ITerrainSettings terrainSettings) : base(terrainSettings)
        {
            _renderMesh = terrainSettings.MeshSettings.RenderMesh;
            _renderMesh.material.SetTexture(Settings.TerrainSettings.GradientHeightTexture,
                terrainSettings.GradientHeight());
        }

        protected override void OnCreateManager()
        {
            base.OnCreateManager();
            _entityArchetypeNoise = EntityManager.CreateArchetype(
                typeof(NoiseCalculation),
                ComponentType.ChunkComponent<ArchetypeChunkNoiseMetaInfo>(),
                ComponentType.ChunkComponent<ArchetypeChunkCalculationIndicator>()
            );
            _entityArchetypeMesh = EntityManager.CreateArchetype(
                typeof(Position),
                typeof(RenderMesh),
                typeof(RenderBounds)
            );
        }

        protected override unsafe void OnUpdate()
        {
            using (var entities = new NativeArray<Entity>(
                TerrainSettings.ChunkCount * TerrainSettings.ArrayChunk,
                Allocator.Temp
            ))
            {
                EntityManager.CreateEntity(_entityArchetypeNoise, entities);
            }

            var meshCalculationGrid = new MeshCalculationGrid
            (
                TerrainSettings.Resolution,
                1f / TerrainSettings.Resolution,
                TerrainSettings.TotalVertices,
                TerrainSettings.TotalTriangles
            );
            meshCalculationGrid.Schedule(64).Complete();
            _vertices.NativeInject(meshCalculationGrid.Vertices);
            _triangles.NativeInject(meshCalculationGrid.Triangles);

            using (var entities = new NativeArray<Entity>(TerrainSettings.ChunkCount, Allocator.Temp))
            {
                EntityManager.CreateEntity(_entityArchetypeMesh, entities);
                for (var i = 0; i < TerrainSettings.ChunkCount; i++)
                {
                    var mesh = new Mesh();
                    mesh.SetVertices(_vertices);
                    mesh.SetTriangles(_triangles, 0);
                    mesh.SetNormals(_vertices);
                    mesh.MarkDynamic();
                    _renderMesh.mesh = mesh;
                    PostUpdateCommands.SetSharedComponent(entities[i], _renderMesh);
                    PostUpdateCommands.SetComponent(entities[i], new RenderBounds
                    {
                        Value = new AABB
                        {
                            Center = float3.zero,
                            Extents = math.float3(0.5f, 1f, 0.5f)
                        }
                    });
                }
            }

            var entity = EntityManager.CreateEntity(EntityManager.CreateArchetype(typeof(VerticesData)));
            var verticesData = EntityManager.GetBuffer<VerticesData>(entity);
            verticesData.ResizeUninitialized(TerrainSettings.TotalVertices);
            verticesData.InjectData(meshCalculationGrid.Vertices.GetUnsafeReadOnlyPtr(), TerrainSettings.TotalVertices);

            meshCalculationGrid.Dispose();
            _triangles.Clear();
            _vertices.Clear();
        }
    }
}