using UnityEngine;

namespace SoulForge.Combat
{
    public interface IDamageable
    {
        CombatTeam Team { get; }
        Transform Transform { get; }
        void ApplyDamage(float damage);
    }
}
