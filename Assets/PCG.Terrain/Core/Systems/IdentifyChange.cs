using System;
using JetBrains.Annotations;
using PCG.Terrain.Core.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Assertions;

namespace PCG.Terrain.Core.Systems
{
    public sealed class IdentifyChange
    {
        public delegate int2 OnChangedPosition(float2 followTargetPosition);

        public static readonly EntityArchetypeQuery EntityArchetypeQuery = new EntityArchetypeQuery
        {
            All = new[]
            {
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<FollowTarget>()
            }
        };

        private readonly int _changeThreshold;
        private readonly EntityManager _entityManager;
        private readonly ComponentGroup _componentGroup;

        private int _followTargetOrderVersion = -1;
        private Entity _followTargetEntity = Entity.Null;
        public int2 CentroidPosition { get; private set; }

        public IdentifyChange([NotNull] EntityManager entityManager, [NotNull] ComponentGroup componentGroup,
            int changeThreshold, int2 centroidPosition)
        {
#if DEBUG
            // ReSharper disable once JoinNullCheckWithUsage
            if (entityManager == null) throw new ArgumentNullException(nameof(entityManager));
            // ReSharper disable once JoinNullCheckWithUsage
            if (componentGroup == null) throw new ArgumentNullException(nameof(componentGroup));
#endif
            _entityManager = entityManager;
            _componentGroup = componentGroup;
            _changeThreshold = changeThreshold;
            CentroidPosition = centroidPosition;
        }

        public void HandleChangeFollowTargetPosition([NotNull] OnChangedPosition onChangedPosition)
        {
#if DEBUG
            if (onChangedPosition == null) throw new ArgumentNullException(nameof(onChangedPosition));
#endif
            UpdateFollowTargetEntity();
            if (CheckNewPositionOfFollowTarget(out var followTargetPosition))
            {
                CentroidPosition = onChangedPosition.Invoke(followTargetPosition);
            }
        }

        private void UpdateFollowTargetEntity()
        {
            var followTargetOrderVersion = _entityManager.GetComponentOrderVersion<FollowTarget>();
            if (followTargetOrderVersion == _followTargetOrderVersion)
            {
                return;
            }

            var followTargetChunks = _componentGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            if (followTargetChunks.Length > 0)
            {
                Assert.IsTrue(
                    followTargetChunks.Length == 1 && followTargetChunks[0].Count == 1,
                    $"Only one followed target is supported. Make sure component {typeof(FollowTarget)} is only in one Entity."
                );
                _followTargetEntity =
                    followTargetChunks[0].GetNativeArray(_entityManager.GetArchetypeChunkEntityType())[0];
            }
            else
            {
                _followTargetEntity = Entity.Null;
            }

            followTargetChunks.Dispose();

            _followTargetOrderVersion = followTargetOrderVersion;
        }

        private bool CheckNewPositionOfFollowTarget(out float2 followTargetPosition)
        {
            followTargetPosition = math.float2(0f);
            if (_followTargetEntity.Equals(Entity.Null))
            {
                return false;
            }

            var position = _entityManager.GetComponentData<Position>(_followTargetEntity).Value;
            followTargetPosition.x = position.x;
            followTargetPosition.y = position.z;

            return CheckOverlapOfChangeThreshold(followTargetPosition);
        }

        private bool CheckOverlapOfChangeThreshold(float2 followTargetPosition)
        {
            return math.any(math.abs(followTargetPosition - CentroidPosition) > _changeThreshold + 0.5f);
        }
    }
}