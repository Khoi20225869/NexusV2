using UnityEngine;

namespace SoulForge.Data
{
    [CreateAssetMenu(menuName = "SoulForge/Weapon Definition", fileName = "WeaponDefinition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [field: SerializeField] public string WeaponId { get; private set; } = "weapon_default";
        [field: SerializeField] public string DisplayName { get; private set; } = "Weapon";
        [field: SerializeField] public string Description { get; private set; } = "Starter weapon.";
        [field: SerializeField] public WeaponRarity Rarity { get; private set; } = WeaponRarity.Common;
        [field: SerializeField] public RoomTier MinRoomTier { get; private set; } = RoomTier.Tier1;
        [field: SerializeField] public RoomTier MaxRoomTier { get; private set; } = RoomTier.Tier3;
        [field: SerializeField] public int ShopPrice { get; private set; } = 25;
        [field: SerializeField] public int ChestWeight { get; private set; } = 10;
        [field: SerializeField] public Color AccentColor { get; private set; } = Color.white;
        [field: SerializeField] public float Damage { get; private set; } = 1f;
        [field: SerializeField] public float FireRate { get; private set; } = 4f;
        [field: SerializeField] public float ProjectileSpeed { get; private set; } = 14f;
    }
}
