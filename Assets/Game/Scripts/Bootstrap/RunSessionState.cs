using SoulForge.Data;
using UnityEngine;

namespace SoulForge.Bootstrap
{
    public static class RunSessionState
    {
        private const string ProfileCreatedKey = "SoulForge.Profile.Created";
        private const string HeroIdKey = "SoulForge.Profile.HeroId";
        private const string HairKey = "SoulForge.Profile.Appearance.Hair";
        private const string FaceHairKey = "SoulForge.Profile.Appearance.FaceHair";
        private const string HelmetKey = "SoulForge.Profile.Appearance.Helmet";
        private const string ArmorKey = "SoulForge.Profile.Appearance.Armor";
        private const string PantKey = "SoulForge.Profile.Appearance.Pant";
        private const string WeaponKey = "SoulForge.Profile.Appearance.Weapon";
        private const string BackKey = "SoulForge.Profile.Appearance.Back";
        private const string EyeColorKey = "SoulForge.Profile.Appearance.EyeColor";
        private const string HairColorKey = "SoulForge.Profile.Appearance.HairColor";
        private const string ClothColorKey = "SoulForge.Profile.Appearance.Cloth";

        public static HeroDefinition SelectedHero { get; private set; }
        public static WeaponDefinition ShopWeaponOverride { get; private set; }
        public static HeroAppearanceSelection AppearanceSelection { get; private set; } = new HeroAppearanceSelection();

        public static bool HasHeroSelection => SelectedHero != null;
        public static bool HasSavedProfile => PlayerPrefs.GetInt(ProfileCreatedKey, 0) == 1;

        public static void SelectHero(HeroDefinition heroDefinition)
        {
            SelectedHero = heroDefinition;
        }

        public static void SetShopWeapon(WeaponDefinition weaponDefinition)
        {
            ShopWeaponOverride = weaponDefinition;
        }

        public static void SetAppearance(HeroAppearanceSelection selection)
        {
            AppearanceSelection = selection != null ? selection.Clone() : new HeroAppearanceSelection();
        }

        public static void SaveProfile(HeroDefinition heroDefinition, HeroAppearanceSelection selection)
        {
            if (heroDefinition == null)
            {
                return;
            }

            SelectHero(heroDefinition);
            SetAppearance(selection);

            PlayerPrefs.SetInt(ProfileCreatedKey, 1);
            PlayerPrefs.SetString(HeroIdKey, heroDefinition.HeroId);
            PlayerPrefs.SetInt(HairKey, AppearanceSelection.HairStyleIndex);
            PlayerPrefs.SetInt(FaceHairKey, AppearanceSelection.FaceHairIndex);
            PlayerPrefs.SetInt(HelmetKey, AppearanceSelection.HelmetIndex);
            PlayerPrefs.SetInt(ArmorKey, AppearanceSelection.ArmorIndex);
            PlayerPrefs.SetInt(PantKey, AppearanceSelection.PantIndex);
            PlayerPrefs.SetInt(WeaponKey, AppearanceSelection.WeaponIndex);
            PlayerPrefs.SetInt(BackKey, AppearanceSelection.BackIndex);
            PlayerPrefs.SetInt(EyeColorKey, AppearanceSelection.EyeColorIndex);
            PlayerPrefs.SetInt(HairColorKey, AppearanceSelection.HairColorIndex);
            PlayerPrefs.SetInt(ClothColorKey, AppearanceSelection.ClothColorIndex);
            PlayerPrefs.Save();
        }

        public static bool TryRestoreSavedProfile(HeroRosterDefinition roster = null, HeroDefinition fallbackHero = null)
        {
            if (!HasSavedProfile)
            {
                return false;
            }

            HeroDefinition resolvedHero = ResolveSavedHero(roster, fallbackHero);
            if (resolvedHero == null)
            {
                return false;
            }

            SelectedHero = resolvedHero;
            AppearanceSelection = LoadSavedAppearance();
            return true;
        }

        public static void Clear()
        {
            SelectedHero = null;
            ShopWeaponOverride = null;
            AppearanceSelection = new HeroAppearanceSelection();
        }

        private static HeroDefinition ResolveSavedHero(HeroRosterDefinition roster, HeroDefinition fallbackHero)
        {
            string savedHeroId = PlayerPrefs.GetString(HeroIdKey, string.Empty);
            if (roster != null && !string.IsNullOrWhiteSpace(savedHeroId))
            {
                for (int i = 0; i < roster.Heroes.Count; i++)
                {
                    HeroDefinition hero = roster.Heroes[i];
                    if (hero != null && hero.HeroId == savedHeroId)
                    {
                        return hero;
                    }
                }
            }

            return fallbackHero;
        }

        private static HeroAppearanceSelection LoadSavedAppearance()
        {
            return new HeroAppearanceSelection
            {
                HairStyleIndex = PlayerPrefs.GetInt(HairKey, 0),
                FaceHairIndex = PlayerPrefs.GetInt(FaceHairKey, 0),
                HelmetIndex = PlayerPrefs.GetInt(HelmetKey, 0),
                ArmorIndex = PlayerPrefs.GetInt(ArmorKey, 0),
                PantIndex = PlayerPrefs.GetInt(PantKey, 0),
                WeaponIndex = PlayerPrefs.GetInt(WeaponKey, 0),
                BackIndex = PlayerPrefs.GetInt(BackKey, 0),
                EyeColorIndex = PlayerPrefs.GetInt(EyeColorKey, 0),
                HairColorIndex = PlayerPrefs.GetInt(HairColorKey, 0),
                ClothColorIndex = PlayerPrefs.GetInt(ClothColorKey, 0)
            };
        }
    }
}
