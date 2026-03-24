using SoulForge.Data;
using SoulForge.Combat;
using UnityEngine;

namespace SoulForge.Player
{
    public sealed class WeaponRuntimeBridge : MonoBehaviour
    {
        [SerializeField] private WeaponRuntime weaponRuntime;

        private void Awake()
        {
            if (weaponRuntime == null)
            {
                weaponRuntime = GetComponent<WeaponRuntime>();
            }
        }

        public void ApplyHero(HeroDefinition heroDefinition)
        {
            if (weaponRuntime == null || heroDefinition == null)
            {
                return;
            }

            WeaponDefinition weaponDefinition = SoulForge.Bootstrap.RunSessionState.ShopWeaponOverride != null
                ? SoulForge.Bootstrap.RunSessionState.ShopWeaponOverride
                : heroDefinition.StartingWeapon;

            if (weaponDefinition != null)
            {
                weaponRuntime.SetWeapon(weaponDefinition);
            }
        }
    }
}
