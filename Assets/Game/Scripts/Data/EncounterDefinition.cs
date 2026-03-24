using System.Collections.Generic;
using UnityEngine;

namespace SoulForge.Data
{
    [CreateAssetMenu(menuName = "SoulForge/Encounter Definition", fileName = "EncounterDefinition")]
    public sealed class EncounterDefinition : ScriptableObject
    {
        [System.Serializable]
        public struct EnemyWaveEntry
        {
            public EnemyDefinition Enemy;
            public int Count;
        }

        [field: SerializeField] public string EncounterId { get; private set; } = "encounter_default";
        [field: SerializeField] public List<EnemyWaveEntry> Waves { get; private set; } = new();
    }
}
