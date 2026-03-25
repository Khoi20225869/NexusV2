using System.Collections.Generic;
using SoulForge.Data;
using SoulForge.UI;
using UnityEngine;

namespace SoulForge.Rooms
{
    public sealed class RewardSpawner : MonoBehaviour
    {
        [SerializeField] private List<RewardDefinition> possibleRewards = new();
        [SerializeField] private List<WeaponDefinition> chestWeapons = new();
        [SerializeField] private List<WeaponLootEntry> chestLootTable = new();
        [SerializeField] private RewardPanelPresenter rewardPanelPresenter;
        [SerializeField] private WeaponChest chestPrefab;
        [SerializeField] private Transform chestSpawnPoint;

        private WeaponChest spawnedChest;

        private RewardDefinition lastReward;

        public RewardDefinition LastReward => lastReward;
        public RoomController OwnerRoom { get; set; }

        private void Awake()
        {
            if (OwnerRoom == null)
            {
                OwnerRoom = GetComponent<RoomController>();
            }
        }

        public void ShowReward()
        {
            if (possibleRewards == null || possibleRewards.Count == 0)
            {
                return;
            }

            lastReward = possibleRewards[Random.Range(0, possibleRewards.Count)];
            rewardPanelPresenter?.Show(lastReward);
            SpawnChest();
        }

        public void HideReward()
        {
            rewardPanelPresenter?.Hide();
        }

        private void SpawnChest()
        {
            if (chestPrefab == null)
            {
                return;
            }

            if (spawnedChest != null)
            {
                Destroy(spawnedChest.gameObject);
            }

            Transform spawnPoint = chestSpawnPoint != null ? chestSpawnPoint : transform;
            spawnedChest = Instantiate(chestPrefab, spawnPoint.position, Quaternion.identity);
            spawnedChest.SetTier(OwnerRoom != null ? OwnerRoom.Tier : RoomTier.Tier1);
            if (chestLootTable != null && chestLootTable.Count > 0)
            {
                spawnedChest.Configure(FilterLootByTier(chestLootTable));
            }
            else
            {
                spawnedChest.Configure(FilterWeaponsByTier(chestWeapons));
            }
        }

        private System.Collections.Generic.List<WeaponLootEntry> FilterLootByTier(System.Collections.Generic.IReadOnlyList<WeaponLootEntry> source)
        {
            var filtered = new System.Collections.Generic.List<WeaponLootEntry>();
            RoomTier roomTier = OwnerRoom != null ? OwnerRoom.Tier : RoomTier.Tier1;

            for (int i = 0; i < source.Count; i++)
            {
                WeaponLootEntry entry = source[i];
                if (entry?.Weapon == null)
                {
                    continue;
                }

                if (entry.Weapon.MinRoomTier > roomTier || entry.Weapon.MaxRoomTier < roomTier)
                {
                    continue;
                }

                filtered.Add(entry);
            }

            if (IsBossRoom())
            {
                var bossFiltered = filtered.FindAll(entry => entry != null && entry.Weapon != null && entry.Weapon.Rarity >= WeaponRarity.Rare);
                if (bossFiltered.Count > 0)
                {
                    return bossFiltered;
                }
            }

            return filtered.Count > 0 ? filtered : new System.Collections.Generic.List<WeaponLootEntry>(source);
        }

        private System.Collections.Generic.List<WeaponDefinition> FilterWeaponsByTier(System.Collections.Generic.IReadOnlyList<WeaponDefinition> source)
        {
            var filtered = new System.Collections.Generic.List<WeaponDefinition>();
            RoomTier roomTier = OwnerRoom != null ? OwnerRoom.Tier : RoomTier.Tier1;

            for (int i = 0; i < source.Count; i++)
            {
                WeaponDefinition weapon = source[i];
                if (weapon == null)
                {
                    continue;
                }

                if (weapon.MinRoomTier > roomTier || weapon.MaxRoomTier < roomTier)
                {
                    continue;
                }

                filtered.Add(weapon);
            }

            if (IsBossRoom())
            {
                var bossFiltered = filtered.FindAll(weapon => weapon != null && weapon.Rarity >= WeaponRarity.Rare);
                if (bossFiltered.Count > 0)
                {
                    return bossFiltered;
                }
            }

            return filtered.Count > 0 ? filtered : new System.Collections.Generic.List<WeaponDefinition>(source);
        }

        private bool IsBossRoom()
        {
            return OwnerRoom != null && OwnerRoom.Tier == RoomTier.Tier3;
        }
    }
}
