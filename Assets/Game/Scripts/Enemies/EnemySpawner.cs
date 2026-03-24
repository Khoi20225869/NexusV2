using System.Collections.Generic;
using SoulForge.Data;
using SoulForge.Player;
using SoulForge.Rooms;
using UnityEngine;

namespace SoulForge.Enemies
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [System.Serializable]
        private struct SpawnEntry
        {
            public string SpawnId;
            public EnemyController Prefab;
            public EnemyDefinition Definition;
            public Transform SpawnPoint;
        }

        [SerializeField] private List<SpawnEntry> spawnEntries = new();
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private RoomController ownerRoom;

        private void Start()
        {
            if (!spawnOnStart)
            {
                return;
            }

            SpawnAll();
        }

        public void SpawnAll()
        {
            PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
            if (player == null)
            {
                return;
            }

            for (int i = 0; i < spawnEntries.Count; i++)
            {
                SpawnEntry entry = spawnEntries[i];
                if (entry.Prefab == null || entry.SpawnPoint == null)
                {
                    continue;
                }

                EnemyController enemy = Instantiate(entry.Prefab, entry.SpawnPoint.position, Quaternion.identity);
                enemy.Initialize(entry.Definition, player, ownerRoom);
            }
        }

        public bool SpawnById(string spawnId)
        {
            if (string.IsNullOrWhiteSpace(spawnId))
            {
                return false;
            }

            PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
            if (player == null)
            {
                return false;
            }

            for (int i = 0; i < spawnEntries.Count; i++)
            {
                SpawnEntry entry = spawnEntries[i];
                if (entry.SpawnId != spawnId || entry.Prefab == null || entry.SpawnPoint == null)
                {
                    continue;
                }

                EnemyController enemy = Instantiate(entry.Prefab, entry.SpawnPoint.position, Quaternion.identity);
                enemy.Initialize(entry.Definition, player, ownerRoom);
                return true;
            }

            return false;
        }
    }
}
