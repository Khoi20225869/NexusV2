using SoulForge.Data;
using UnityEngine;

namespace SoulForge.Combat
{
    public sealed class WeaponRuntime : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition weaponDefinition;

        public WeaponDefinition WeaponDefinition => weaponDefinition;

        public float Damage => weaponDefinition != null ? weaponDefinition.Damage : 1f;
        public float FireRate => weaponDefinition != null ? weaponDefinition.FireRate : 4f;
        public float AttackRange => weaponDefinition != null ? weaponDefinition.AttackRange : 1.3f;
        public float ProjectileSpeed => weaponDefinition != null ? weaponDefinition.ProjectileSpeed : 14f;

        public void SetWeapon(WeaponDefinition newWeaponDefinition)
        {
            weaponDefinition = newWeaponDefinition;
        }
    }
}
