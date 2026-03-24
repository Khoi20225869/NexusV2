using SoulForge.Data;

namespace SoulForge.Bootstrap
{
    public static class RunSessionState
    {
        public static HeroDefinition SelectedHero { get; private set; }
        public static WeaponDefinition ShopWeaponOverride { get; private set; }
        public static HeroAppearanceSelection AppearanceSelection { get; private set; } = new HeroAppearanceSelection();

        public static bool HasHeroSelection => SelectedHero != null;

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

        public static void Clear()
        {
            SelectedHero = null;
            ShopWeaponOverride = null;
            AppearanceSelection = new HeroAppearanceSelection();
        }
    }
}
