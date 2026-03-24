using System;
using UnityEngine;

namespace SoulForge.Data
{
    [Serializable]
    public sealed class WeaponLootEntry
    {
        public WeaponDefinition Weapon;
        [Min(1)] public int Weight = 1;
    }
}
