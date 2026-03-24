using System.Text;
using SoulForge.Data;
using SoulForge.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulForge.UI
{
    public sealed class InventoryPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private TMP_Text heroText;
        [SerializeField] private TMP_Text inventoryText;
        [SerializeField] private TMP_Text equippedText;
        [SerializeField] private Button previousWeaponButton;
        [SerializeField] private Button nextWeaponButton;
        [SerializeField] private Image panelBackground;
        [SerializeField] private Outline panelOutline;

        private void Awake()
        {
            if (playerInventory == null)
            {
                playerInventory = FindFirstObjectByType<PlayerInventory>();
            }

            if (panelBackground == null)
            {
                panelBackground = GetComponent<Image>();
            }

            if (panelOutline == null)
            {
                panelOutline = GetComponent<Outline>();
            }
        }

        private void OnEnable()
        {
            if (playerInventory != null)
            {
                playerInventory.InventoryChanged += Refresh;
            }

            if (previousWeaponButton != null)
            {
                previousWeaponButton.onClick.AddListener(OnPreviousClicked);
            }

            if (nextWeaponButton != null)
            {
                nextWeaponButton.onClick.AddListener(OnNextClicked);
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (playerInventory != null)
            {
                playerInventory.InventoryChanged -= Refresh;
            }

            if (previousWeaponButton != null)
            {
                previousWeaponButton.onClick.RemoveListener(OnPreviousClicked);
            }

            if (nextWeaponButton != null)
            {
                nextWeaponButton.onClick.RemoveListener(OnNextClicked);
            }
        }

        public void Refresh()
        {
            if (heroText != null)
            {
                heroText.text = playerInventory != null && playerInventory.CurrentHero != null
                    ? $"Hero: {playerInventory.CurrentHero.DisplayName}"
                    : "Hero: None";
            }

            if (inventoryText == null)
            {
                return;
            }

            if (playerInventory == null)
            {
                inventoryText.text = "Inventory unavailable";
                return;
            }

            StringBuilder builder = new();
            builder.AppendLine($"Bag {playerInventory.Entries.Count}/{playerInventory.Capacity}");
            builder.AppendLine("Q/E or buttons to switch");

            for (int i = 0; i < playerInventory.Entries.Count; i++)
            {
                PlayerInventory.InventoryEntry entry = playerInventory.Entries[i];
                string marker = i == playerInventory.EquippedWeaponEntryIndex ? ">" : " ";
                string rarity = entry.Equippable ? $" [{entry.Rarity}]" : string.Empty;
                builder.AppendLine($"{marker} {i + 1}. {entry.DisplayName}{rarity} x{entry.Quantity}");
            }

            if (playerInventory.Entries.Count == 0)
            {
                builder.Append("Empty");
            }

            inventoryText.text = builder.ToString();

            if (equippedText != null)
            {
                bool hasEquipped = playerInventory.EquippedWeaponEntryIndex >= 0 &&
                    playerInventory.EquippedWeaponEntryIndex < playerInventory.Entries.Count;
                WeaponRarity rarity = hasEquipped ? playerInventory.Entries[playerInventory.EquippedWeaponEntryIndex].Rarity : WeaponRarity.Common;
                string colorHex = ColorUtility.ToHtmlStringRGB(WeaponRarityPalette.GetColor(rarity));
                equippedText.text = hasEquipped
                    ? $"Equipped: <color=#{colorHex}>{playerInventory.Entries[playerInventory.EquippedWeaponEntryIndex].DisplayName} [{rarity}]</color>"
                    : "Equipped: None";
                ApplyFrame(rarity, hasEquipped);
            }
        }

        private void OnPreviousClicked()
        {
            playerInventory?.CycleWeapon(-1);
        }

        private void OnNextClicked()
        {
            playerInventory?.CycleWeapon(1);
        }

        private void ApplyFrame(WeaponRarity rarity, bool active)
        {
            if (panelBackground != null)
            {
                panelBackground.color = active
                    ? WeaponRarityPalette.GetFillColor(rarity)
                    : new Color(0f, 0f, 0f, 0.65f);
            }

            if (panelOutline != null)
            {
                panelOutline.effectColor = active
                    ? WeaponRarityPalette.GetFrameColor(rarity)
                    : new Color(1f, 1f, 1f, 0.16f);
            }
        }
    }
}
