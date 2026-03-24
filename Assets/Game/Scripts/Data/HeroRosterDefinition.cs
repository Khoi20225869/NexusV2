using System.Collections.Generic;
using UnityEngine;

namespace SoulForge.Data
{
    [CreateAssetMenu(menuName = "SoulForge/Hero Roster", fileName = "HeroRoster")]
    public sealed class HeroRosterDefinition : ScriptableObject
    {
        [field: SerializeField] public List<HeroDefinition> Heroes { get; private set; } = new();
    }
}
