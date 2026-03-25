using SoulForge.Data;
using SoulForge.Player;
using SoulForge.UI;
using TMPro;
using UnityEngine;

namespace SoulForge.Rooms
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class WeaponPickup : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition weaponDefinition;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private SpriteRenderer frameRenderer;
        [SerializeField] private bool autoEquip = true;

        private void Awake()
        {
            if (frameRenderer == null)
            {
                frameRenderer = GetComponent<SpriteRenderer>();
            }

            RefreshLabel();
        }

        public void Configure(WeaponDefinition definition)
        {
            weaponDefinition = definition;
            RefreshLabel();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
            if (inventory == null || weaponDefinition == null)
            {
                return;
            }

            if (!inventory.AddWeapon(weaponDefinition, autoEquip))
            {
                return;
            }

            Destroy(gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (weaponDefinition == null)
            {
                return;
            }

            PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
            if (inventory == null || labelText == null)
            {
                return;
            }

            string colorHex = ColorUtility.ToHtmlStringRGB(WeaponRarityPalette.GetColor(weaponDefinition.Rarity));
            string suffix = inventory.Entries.Count >= inventory.Capacity ? "\n<size=60%>Full bag: swaps equipped</size>" : string.Empty;
            labelText.text = $"{weaponDefinition.DisplayName}\n<size=70%><color=#{colorHex}>{weaponDefinition.Rarity}</color></size>{suffix}";
        }

        private void RefreshLabel()
        {
            if (labelText != null)
            {
                string colorHex = weaponDefinition != null ? ColorUtility.ToHtmlStringRGB(WeaponRarityPalette.GetColor(weaponDefinition.Rarity)) : "FFFFFF";
                labelText.text = weaponDefinition != null
                    ? $"{weaponDefinition.DisplayName}\n<size=70%><color=#{colorHex}>{weaponDefinition.Rarity}</color></size>"
                    : "Pickup";
            }

            if (frameRenderer != null)
            {
                frameRenderer.color = weaponDefinition != null
                    ? WeaponRarityPalette.GetFrameColor(weaponDefinition.Rarity)
                    : Color.white;
            }
        }
    }
}
