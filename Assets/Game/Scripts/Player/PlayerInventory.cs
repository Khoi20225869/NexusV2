using System;
using System.Collections.Generic;
using SoulForge.Data;
using UnityEngine;

namespace SoulForge.Player
{
    public sealed class PlayerInventory : MonoBehaviour
    {
        [Serializable]
        public struct InventoryEntry
        {
            public string ItemId;
            public string DisplayName;
            public int Quantity;
            public WeaponDefinition WeaponDefinition;
            public bool Equippable;
            public WeaponRarity Rarity;
        }

        [SerializeField] private PlayerWeaponController playerWeaponController;

        private readonly List<InventoryEntry> entries = new();
        private int capacity = 8;
        private HeroDefinition currentHero;
        private int equippedWeaponEntryIndex = -1;

        public event Action InventoryChanged;
        public event Action<string> InventoryNotice;

        public IReadOnlyList<InventoryEntry> Entries => entries;
        public HeroDefinition CurrentHero => currentHero;
        public int Capacity => capacity;
        public int EquippedWeaponEntryIndex => equippedWeaponEntryIndex;

        private void Awake()
        {
            if (playerWeaponController == null)
            {
                playerWeaponController = GetComponent<PlayerWeaponController>();
            }
        }

        public void InitializeHero(HeroDefinition heroDefinition)
        {
            currentHero = heroDefinition;
            capacity = heroDefinition != null ? Mathf.Max(1, heroDefinition.InventoryCapacity) : 8;
            entries.Clear();
            equippedWeaponEntryIndex = -1;

            WeaponDefinition startingWeapon = SoulForge.Bootstrap.RunSessionState.ShopWeaponOverride != null
                ? SoulForge.Bootstrap.RunSessionState.ShopWeaponOverride
                : heroDefinition != null ? heroDefinition.StartingWeapon : null;

            if (startingWeapon != null)
            {
                AddWeapon(startingWeapon, equipImmediately: true);
            }
            else
            {
                InventoryChanged?.Invoke();
            }
        }

        public bool AddWeapon(WeaponDefinition weaponDefinition, bool equipImmediately = true)
        {
            if (weaponDefinition == null)
            {
                return false;
            }

            if (entries.Count >= capacity)
            {
                if (!TryReplaceWeapon(weaponDefinition, equipImmediately))
                {
                    InventoryNotice?.Invoke("Bag full");
                    return false;
                }

                return true;
            }

            entries.Add(new InventoryEntry
            {
                ItemId = weaponDefinition.WeaponId,
                DisplayName = weaponDefinition.DisplayName,
                Quantity = 1,
                WeaponDefinition = weaponDefinition,
                Equippable = true,
                Rarity = weaponDefinition.Rarity
            });

            if (equipImmediately)
            {
                EquipWeaponAtEntryIndex(entries.Count - 1);
            }

            InventoryChanged?.Invoke();
            InventoryNotice?.Invoke($"Picked up {weaponDefinition.DisplayName}");
            return true;
        }

        public bool AddReward(RewardDefinition rewardDefinition)
        {
            if (rewardDefinition == null)
            {
                return false;
            }

            bool added = TryAddOrStack(rewardDefinition.RewardId, rewardDefinition.DisplayName);
            if (added)
            {
                InventoryChanged?.Invoke();
            }

            return added;
        }

        public bool CycleWeapon(int direction)
        {
            if (entries.Count == 0)
            {
                return false;
            }

            int startIndex = equippedWeaponEntryIndex >= 0 ? equippedWeaponEntryIndex : 0;
            for (int offset = 1; offset <= entries.Count; offset++)
            {
                int index = (startIndex + offset * (direction >= 0 ? 1 : -1) + entries.Count) % entries.Count;
                if (!entries[index].Equippable || entries[index].WeaponDefinition == null)
                {
                    continue;
                }

                EquipWeaponAtEntryIndex(index);
                return true;
            }

            return false;
        }

        public bool EquipWeaponAtEntryIndex(int entryIndex)
        {
            if (entryIndex < 0 || entryIndex >= entries.Count)
            {
                return false;
            }

            InventoryEntry entry = entries[entryIndex];
            if (!entry.Equippable || entry.WeaponDefinition == null)
            {
                return false;
            }

            equippedWeaponEntryIndex = entryIndex;
            playerWeaponController?.Equip(entry.WeaponDefinition);
            InventoryChanged?.Invoke();
            return true;
        }

        private bool TryAddOrStack(string itemId, string displayName)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (!string.Equals(entries[i].ItemId, itemId, StringComparison.Ordinal) || entries[i].Equippable)
                {
                    continue;
                }

                InventoryEntry updated = entries[i];
                updated.Quantity += 1;
                entries[i] = updated;
                return true;
            }

            if (entries.Count >= capacity)
            {
                return false;
            }

            entries.Add(new InventoryEntry
            {
                ItemId = itemId,
                DisplayName = displayName,
                Quantity = 1,
                WeaponDefinition = null,
                Equippable = false,
                Rarity = WeaponRarity.Common
            });
            return true;
        }

        private bool TryReplaceWeapon(WeaponDefinition weaponDefinition, bool equipImmediately)
        {
            int replaceIndex = GetReplacementWeaponIndex(weaponDefinition);
            if (replaceIndex < 0)
            {
                return false;
            }

            entries[replaceIndex] = new InventoryEntry
            {
                ItemId = weaponDefinition.WeaponId,
                DisplayName = weaponDefinition.DisplayName,
                Quantity = 1,
                WeaponDefinition = weaponDefinition,
                Equippable = true,
                Rarity = weaponDefinition.Rarity
            };

            if (equipImmediately || replaceIndex == equippedWeaponEntryIndex)
            {
                EquipWeaponAtEntryIndex(replaceIndex);
            }
            else
            {
                InventoryChanged?.Invoke();
            }

            InventoryNotice?.Invoke($"Swapped in {weaponDefinition.DisplayName}");
            return true;
        }

        private int GetReplacementWeaponIndex(WeaponDefinition incomingWeapon)
        {
            if (entries.Count == 0)
            {
                return -1;
            }

            if (equippedWeaponEntryIndex >= 0 && equippedWeaponEntryIndex < entries.Count)
            {
                return equippedWeaponEntryIndex;
            }

            int lowestRarityIndex = -1;
            WeaponRarity lowestRarity = WeaponRarity.Epic;
            for (int i = 0; i < entries.Count; i++)
            {
                if (!entries[i].Equippable || entries[i].WeaponDefinition == null)
                {
                    continue;
                }

                if (lowestRarityIndex < 0 || entries[i].Rarity < lowestRarity)
                {
                    lowestRarityIndex = i;
                    lowestRarity = entries[i].Rarity;
                }
            }

            if (lowestRarityIndex >= 0 && incomingWeapon != null && incomingWeapon.Rarity >= lowestRarity)
            {
                return lowestRarityIndex;
            }

            return lowestRarityIndex;
        }
    }
}
