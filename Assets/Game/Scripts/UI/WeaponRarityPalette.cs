using SoulForge.Data;
using UnityEngine;

namespace SoulForge.UI
{
    public static class WeaponRarityPalette
    {
        public static Color GetColor(WeaponRarity rarity)
        {
            return rarity switch
            {
                WeaponRarity.Common => new Color(0.74f, 0.84f, 0.92f, 1f),
                WeaponRarity.Uncommon => new Color(0.46f, 1f, 0.58f, 1f),
                WeaponRarity.Rare => new Color(0.48f, 0.82f, 1f, 1f),
                WeaponRarity.Epic => new Color(1f, 0.58f, 0.95f, 1f),
                _ => Color.white
            };
        }

        public static Color GetFrameColor(WeaponRarity rarity)
        {
            Color color = GetColor(rarity);
            color.a = 0.9f;
            return color;
        }

        public static Color GetFillColor(WeaponRarity rarity)
        {
            Color color = GetColor(rarity) * 0.26f;
            color.a = 0.88f;
            return color;
        }
    }
}
