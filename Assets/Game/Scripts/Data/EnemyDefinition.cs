using UnityEngine;

namespace SoulForge.Data
{
    [CreateAssetMenu(menuName = "SoulForge/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [field: SerializeField] public string EnemyId { get; private set; } = "enemy_default";
        [field: SerializeField] public float MaxHealth { get; private set; } = 3f;
        [field: SerializeField] public float MoveSpeed { get; private set; } = 2f;
        [field: SerializeField] public float AttackRange { get; private set; } = 4f;
        [field: SerializeField] public float AttackRate { get; private set; } = 1f;
        [field: SerializeField] public float ContactDamage { get; private set; } = 1f;
        [field: SerializeField] public bool UsesProjectile { get; private set; } = true;
        [field: SerializeField] public float ProjectileSpeed { get; private set; } = 8f;
    }
}
