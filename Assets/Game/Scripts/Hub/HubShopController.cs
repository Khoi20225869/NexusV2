using SoulForge.Bootstrap;
using SoulForge.Data;
using SoulForge.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulForge.Hub
{
    public sealed class HubShopController : MonoBehaviour
    {
        [System.Serializable]
        private struct ShopItemView
        {
            public Button Button;
            public Image Frame;
            public TMP_Text Label;
            public TMP_Text Price;
            public WeaponDefinition Weapon;
            public int Cost;
        }

        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text walletText;
        [SerializeField] private TMP_Text selectedText;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private Button closeButton;
        [SerializeField] private ShopItemView[] items;
        [SerializeField] private int startingCoins = 120;

        private int coins;
        public bool IsOpen => root != null ? root.activeSelf : gameObject.activeSelf;

        private void Awake()
        {
            if (root == null)
            {
                root = gameObject;
            }

            coins = startingCoins;

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HideShop);
                closeButton.onClick.AddListener(HideShop);
            }

            for (int i = 0; i < items.Length; i++)
            {
                int captured = i;
                if (items[i].Button == null)
                {
                    continue;
                }

                if (items[i].Frame == null)
                {
                    items[i].Frame = items[i].Button.GetComponent<Image>();
                }

                items[i].Button.onClick.RemoveAllListeners();
                items[i].Button.onClick.AddListener(() => Buy(captured));

                if (items[i].Label != null)
                {
                    string rarityColor = items[i].Weapon != null ? ColorUtility.ToHtmlStringRGB(WeaponRarityPalette.GetColor(items[i].Weapon.Rarity)) : "FFFFFF";
                    items[i].Label.text = items[i].Weapon != null
                        ? $"{items[i].Weapon.DisplayName}\n<size=70%><color=#{rarityColor}>{items[i].Weapon.Rarity}</color></size>"
                        : "Sold Out";
                }

                if (items[i].Price != null)
                {
                    items[i].Price.text = $"{GetCost(items[i])} C";
                }

                if (items[i].Frame != null && items[i].Weapon != null)
                {
                    items[i].Frame.color = WeaponRarityPalette.GetFillColor(items[i].Weapon.Rarity);
                }
            }

            Refresh();
            HideShop();
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HideShop);
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Button != null)
                {
                    items[i].Button.onClick.RemoveAllListeners();
                }
            }
        }

        private void Buy(int index)
        {
            if (index < 0 || index >= items.Length)
            {
                return;
            }

            ShopItemView item = items[index];
            int cost = GetCost(item);
            if (item.Weapon == null || coins < cost)
            {
                return;
            }

            coins -= cost;
            RunSessionState.SetShopWeapon(item.Weapon);
            SetHint($"Forged: {item.Weapon.DisplayName}");
            Refresh();
        }

        public void ShowShop()
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            SetHint("Forge a starting weapon, or close the altar.");
            Refresh();
        }

        public void HideShop()
        {
            SetHint("Approach the altar or click it to browse weapons.");

            if (root != null)
            {
                root.SetActive(false);
            }
        }

        public void ToggleShop()
        {
            if (IsOpen)
            {
                HideShop();
                return;
            }

            ShowShop();
        }

        private void Refresh()
        {
            if (walletText != null)
            {
                walletText.text = $"Crowns: {coins}";
            }

            if (selectedText != null)
            {
                selectedText.text = RunSessionState.ShopWeaponOverride != null
                    ? $"Shop Loadout: <color=#{ColorUtility.ToHtmlStringRGB(WeaponRarityPalette.GetColor(RunSessionState.ShopWeaponOverride.Rarity))}>{RunSessionState.ShopWeaponOverride.DisplayName} [{RunSessionState.ShopWeaponOverride.Rarity}]</color>"
                    : "Shop Loadout: Default";
            }

            if (hintText != null && string.IsNullOrWhiteSpace(hintText.text))
            {
                hintText.text = "Forge a starting weapon.";
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Button != null)
                {
                    items[i].Button.interactable = items[i].Weapon != null && coins >= GetCost(items[i]);
                }
            }
        }

        private void SetHint(string message)
        {
            if (hintText != null)
            {
                hintText.text = message;
            }
        }

        private static int GetCost(ShopItemView item)
        {
            if (item.Weapon != null && item.Weapon.ShopPrice > 0)
            {
                return item.Weapon.ShopPrice;
            }

            return Mathf.Max(0, item.Cost);
        }
    }
}
