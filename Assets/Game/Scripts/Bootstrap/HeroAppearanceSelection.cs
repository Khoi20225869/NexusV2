using System;
using UnityEngine;

namespace SoulForge.Bootstrap
{
    [Serializable]
    public sealed class HeroAppearanceSelection
    {
        [SerializeField] private int hairStyleIndex;
        [SerializeField] private int faceHairIndex;
        [SerializeField] private int helmetIndex;
        [SerializeField] private int armorIndex;
        [SerializeField] private int pantIndex;
        [SerializeField] private int weaponIndex;
        [SerializeField] private int backIndex;
        [SerializeField] private int eyeColorIndex;
        [SerializeField] private int hairColorIndex;
        [SerializeField] private int clothColorIndex;

        public int HairStyleIndex
        {
            get => hairStyleIndex;
            set => hairStyleIndex = Mathf.Max(0, value);
        }

        public int FaceHairIndex
        {
            get => faceHairIndex;
            set => faceHairIndex = Mathf.Max(0, value);
        }

        public int HelmetIndex
        {
            get => helmetIndex;
            set => helmetIndex = Mathf.Max(0, value);
        }

        public int ArmorIndex
        {
            get => armorIndex;
            set => armorIndex = Mathf.Max(0, value);
        }

        public int PantIndex
        {
            get => pantIndex;
            set => pantIndex = Mathf.Max(0, value);
        }

        public int WeaponIndex
        {
            get => weaponIndex;
            set => weaponIndex = Mathf.Max(0, value);
        }

        public int BackIndex
        {
            get => backIndex;
            set => backIndex = Mathf.Max(0, value);
        }

        public int EyeColorIndex
        {
            get => eyeColorIndex;
            set => eyeColorIndex = Mathf.Max(0, value);
        }

        public int HairColorIndex
        {
            get => hairColorIndex;
            set => hairColorIndex = Mathf.Max(0, value);
        }

        public int ClothColorIndex
        {
            get => clothColorIndex;
            set => clothColorIndex = Mathf.Max(0, value);
        }

        public HeroAppearanceSelection Clone()
        {
            return new HeroAppearanceSelection
            {
                HairStyleIndex = HairStyleIndex,
                FaceHairIndex = FaceHairIndex,
                HelmetIndex = HelmetIndex,
                ArmorIndex = ArmorIndex,
                PantIndex = PantIndex,
                WeaponIndex = WeaponIndex,
                BackIndex = BackIndex,
                EyeColorIndex = EyeColorIndex,
                HairColorIndex = HairColorIndex,
                ClothColorIndex = ClothColorIndex
            };
        }
    }
}
