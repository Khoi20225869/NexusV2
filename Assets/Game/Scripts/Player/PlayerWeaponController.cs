using SoulForge.Combat;
using SoulForge.Data;
using UnityEngine;

namespace SoulForge.Player
{
    public sealed class PlayerWeaponController : MonoBehaviour
    {
        [SerializeField] private WeaponRuntime weaponRuntime;

        public event System.Action<WeaponDefinition> WeaponEquipped;

        public WeaponDefinition CurrentWeapon => weaponRuntime != null ? weaponRuntime.WeaponDefinition : null;

        private void Awake()
        {
            if (weaponRuntime == null)
            {
                weaponRuntime = GetComponent<WeaponRuntime>();
            }
        }

        public void Equip(WeaponDefinition weaponDefinition)
        {
            if (weaponRuntime == null)
            {
                return;
            }

            weaponRuntime.SetWeapon(weaponDefinition);
            WeaponEquipped?.Invoke(weaponDefinition);
        }
    }
}
