using UnityEngine;

namespace SoulForge.Data
{
    [CreateAssetMenu(menuName = "SoulForge/Hero Definition", fileName = "HeroDefinition")]
    public sealed class HeroDefinition : ScriptableObject
    {
        [field: SerializeField] public string HeroId { get; private set; } = "hero_default";
        [field: SerializeField] public string DisplayName { get; private set; } = "Knight";
        [field: SerializeField] public string Description { get; private set; } = "Balanced close-range adventurer.";
        [field: SerializeField] public WeaponDefinition StartingWeapon { get; private set; }
        [field: SerializeField] public GameObject CharacterPrefab { get; private set; }
        [field: SerializeField] public int InventoryCapacity { get; private set; } = 8;
        [field: SerializeField] public int AttackAnimationId { get; private set; } = 4;
        [field: SerializeField] public Color EyeColor { get; private set; } = Color.white;
        [field: SerializeField] public Color HairColor { get; private set; } = Color.white;
        [field: SerializeField] public float MaxHealth { get; private set; } = 6f;
        [field: SerializeField] public float MaxShield { get; private set; } = 4f;
        [field: SerializeField] public float MoveSpeed { get; private set; } = 5f;
        [field: SerializeField] public float DashCooldown { get; private set; } = 4f;
    }
}
