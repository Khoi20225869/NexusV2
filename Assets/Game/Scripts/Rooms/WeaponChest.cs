using System.Collections.Generic;
using SoulForge.Data;
using SoulForge.Player;
using TMPro;
using UnityEngine;

namespace SoulForge.Rooms
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class WeaponChest : MonoBehaviour
    {
        [SerializeField] private WeaponPickup pickupPrefab;
        [SerializeField] private Transform dropPoint;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private SpriteRenderer chestRenderer;
        [SerializeField] private Color tier1ClosedColor = new(0.68f, 0.46f, 0.16f, 1f);
        [SerializeField] private Color tier2ClosedColor = new(0.26f, 0.62f, 0.34f, 1f);
        [SerializeField] private Color tier3ClosedColor = new(0.62f, 0.24f, 0.72f, 1f);
        [SerializeField] private Color openedTint = new(0.42f, 0.32f, 0.18f, 1f);
        [SerializeField] private List<WeaponLootEntry> lootTable = new();

        private readonly List<WeaponDefinition> weaponPool = new();
        private PlayerInventory nearbyInventory;
        private bool opened;
        private RoomTier chestTier = RoomTier.Tier1;

        private void Awake()
        {
            SetPrompt(false, "Press E");
            UpdateVisual();
        }

        private void Update()
        {
            if (opened || nearbyInventory == null)
            {
                return;
            }

            if (Input.GetKeyDown(interactKey))
            {
                OpenChest();
            }
        }

        public void Configure(IEnumerable<WeaponDefinition> weapons)
        {
            weaponPool.Clear();
            lootTable.Clear();
            if (weapons == null)
            {
                return;
            }

            foreach (WeaponDefinition weapon in weapons)
            {
                if (weapon != null)
                {
                    weaponPool.Add(weapon);
                    lootTable.Add(new WeaponLootEntry { Weapon = weapon, Weight = Mathf.Max(1, weapon.ChestWeight) });
                }
            }
        }

        public void Configure(IReadOnlyList<WeaponLootEntry> entries)
        {
            weaponPool.Clear();
            lootTable.Clear();

            if (entries == null)
            {
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                WeaponLootEntry entry = entries[i];
                if (entry?.Weapon == null)
                {
                    continue;
                }

                weaponPool.Add(entry.Weapon);
                lootTable.Add(new WeaponLootEntry
                {
                    Weapon = entry.Weapon,
                    Weight = Mathf.Max(1, entry.Weight)
                });
            }
        }

        public void SetTier(RoomTier tier)
        {
            chestTier = tier;
            UpdateVisual();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            nearbyInventory = other.GetComponentInParent<PlayerInventory>();
            if (nearbyInventory != null && !opened)
            {
                SetPrompt(true, "Press E to open chest");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (nearbyInventory == null || other.GetComponentInParent<PlayerInventory>() != nearbyInventory)
            {
                return;
            }

            nearbyInventory = null;
            SetPrompt(false, string.Empty);
        }

        private void OpenChest()
        {
            WeaponDefinition selectedWeapon = PickWeapon();
            if (pickupPrefab == null || selectedWeapon == null)
            {
                return;
            }

            opened = true;
            Transform spawnPoint = dropPoint != null ? dropPoint : transform;
            WeaponPickup pickup = Instantiate(pickupPrefab, spawnPoint.position, Quaternion.identity);
            pickup.Configure(selectedWeapon);
            SetPrompt(true, $"{selectedWeapon.DisplayName} [{selectedWeapon.Rarity}]");
            if (promptText != null)
            {
                promptText.color = selectedWeapon.AccentColor;
            }
            UpdateVisual();
        }

        private WeaponDefinition PickWeapon()
        {
            if (lootTable.Count > 0)
            {
                int totalWeight = 0;
                for (int i = 0; i < lootTable.Count; i++)
                {
                    if (lootTable[i]?.Weapon == null)
                    {
                        continue;
                    }

                    totalWeight += Mathf.Max(1, lootTable[i].Weight);
                }

                if (totalWeight <= 0)
                {
                    return null;
                }

                int roll = Random.Range(0, totalWeight);
                int cumulative = 0;
                for (int i = 0; i < lootTable.Count; i++)
                {
                    WeaponLootEntry entry = lootTable[i];
                    if (entry?.Weapon == null)
                    {
                        continue;
                    }

                    cumulative += Mathf.Max(1, entry.Weight);
                    if (roll < cumulative)
                    {
                        return entry.Weapon;
                    }
                }
            }

            return weaponPool.Count > 0 ? weaponPool[Random.Range(0, weaponPool.Count)] : null;
        }

        private void UpdateVisual()
        {
            if (chestRenderer != null)
            {
                chestRenderer.color = opened ? GetOpenedColor() : GetClosedColor();
            }
        }

        private Color GetClosedColor()
        {
            return chestTier switch
            {
                RoomTier.Tier2 => tier2ClosedColor,
                RoomTier.Tier3 => tier3ClosedColor,
                _ => tier1ClosedColor
            };
        }

        private Color GetOpenedColor()
        {
            return Color.Lerp(GetClosedColor(), openedTint, 0.65f);
        }

        private void SetPrompt(bool visible, string text)
        {
            if (promptText == null)
            {
                return;
            }

            promptText.gameObject.SetActive(visible);
            promptText.text = text;
            if (!visible)
            {
                promptText.color = Color.white;
            }
        }
    }
}
