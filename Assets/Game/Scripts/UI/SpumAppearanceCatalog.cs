using System;
using UnityEngine;

namespace SoulForge.UI
{
    public static class SpumAppearanceCatalog
    {
        public static readonly string[] HairStyles =
        {
            string.Empty,
            "SPUM/SPUM_Sprites/Items/0_Hair/Hair_1",
            "SPUM/SPUM_Sprites/Items/0_Hair/Hair_2",
            "SPUM/SPUM_Sprites/Items/0_Hair/Hair_3"
        };

        public static readonly string[] FaceHairs =
        {
            string.Empty,
            "SPUM/SPUM_Sprites/Items/1_FaceHair/FaceHair_1",
            "SPUM/SPUM_Sprites/Items/1_FaceHair/FaceHair_2"
        };

        public static readonly string[] Helmets =
        {
            string.Empty,
            "SPUM/SPUM_Sprites/Items/4_Helmet/Helmet_1",
            "SPUM/SPUM_Sprites/Items/4_Helmet/Helmet_2",
            "SPUM/SPUM_Sprites/Items/4_Helmet/Helmet_3"
        };

        public static readonly string[] BackItems =
        {
            string.Empty,
            "SPUM/SPUM_Sprites/Items/7_Back/Back_3",
            "SPUM/SPUM_Sprites/Items/7_Back/BowBack_1"
        };

        public static readonly string[] Weapons =
        {
            string.Empty,
            "SPUM/SPUM_Sprites/Items/6_Weapons/Sword_1",
            "SPUM/SPUM_Sprites/Items/6_Weapons/Bow_1",
            "SPUM/SPUM_Sprites/Items/6_Weapons/Ward_1",
            "SPUM/SPUM_Sprites/Items/6_Weapons/Axe_1"
        };

        public static readonly string[] Armors =
        {
            string.Empty,
            "SPUM/SPUM_Sprites/Items/5_Armor/Armor_1",
            "SPUM/SPUM_Sprites/Items/5_Armor/Armor_2",
            "SPUM/SPUM_Sprites/Items/5_Armor/Armor_3"
        };

        public static readonly string[] Pants =
        {
            string.Empty,
            "SPUM/SPUM_Sprites/Items/3_Pant/Foot_1",
            "SPUM/SPUM_Sprites/Items/3_Pant/Foot_2",
            "SPUM/SPUM_Sprites/Items/3_Pant/Foot_3"
        };

        public static readonly Color[] EyeColors =
        {
            new(0.36f, 0.85f, 1f, 1f),
            new(0.38f, 1f, 0.48f, 1f),
            new(1f, 0.45f, 0.86f, 1f),
            new(1f, 0.82f, 0.32f, 1f)
        };

        public static readonly Color[] HairColors =
        {
            new(0.78f, 0.72f, 0.28f, 1f),
            new(0.18f, 0.7f, 0.28f, 1f),
            new(0.64f, 0.3f, 1f, 1f),
            new(0.92f, 0.38f, 0.24f, 1f)
        };

        public static readonly Color[] ClothColors =
        {
            new(0.92f, 0.92f, 1f, 1f),
            new(0.82f, 1f, 0.82f, 1f),
            new(0.92f, 0.84f, 1f, 1f),
            new(1f, 0.9f, 0.72f, 1f)
        };

        public static string GetHairLabel(int index) => GetSpriteLabel(HairStyles, index, "None", "Hair ");
        public static string GetFaceHairLabel(int index) => GetSpriteLabel(FaceHairs, index, "None", "FaceHair ");
        public static string GetHelmetLabel(int index) => GetSpriteLabel(Helmets, index, "None", "Helmet ");
        public static string GetArmorLabel(int index) => GetSpriteLabel(Armors, index, "None", "Armor ");
        public static string GetPantLabel(int index) => GetSpriteLabel(Pants, index, "None", "Pant ");
        public static string GetWeaponLabel(int index) => GetSpriteLabel(Weapons, index, "None", "Weapon ");
        public static string GetBackLabel(int index) => GetSpriteLabel(BackItems, index, "None", "Back ");
        public static string GetEyeColorLabel(int index) => GetColorLabel(index, "Eyes");
        public static string GetHairColorLabel(int index) => GetColorLabel(index, "Hair");
        public static string GetClothColorLabel(int index) => GetColorLabel(index, "Cloth");

        private static string GetSpriteLabel(string[] source, int index, string noneLabel, string prefix)
        {
            if (source == null || source.Length == 0)
            {
                return noneLabel;
            }

            int clamped = Wrap(index, source.Length);
            return clamped == 0 ? noneLabel : $"{prefix}{clamped}";
        }

        private static string GetColorLabel(int index, string prefix)
        {
            return $"{prefix} {Wrap(index, 4) + 1}";
        }

        public static int Wrap(int value, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            int wrapped = value % count;
            return wrapped < 0 ? wrapped + count : wrapped;
        }

        public static Sprite LoadSprite(string resourcePath)
        {
            return string.IsNullOrWhiteSpace(resourcePath) ? null : Resources.Load<Sprite>(resourcePath);
        }
    }
}
