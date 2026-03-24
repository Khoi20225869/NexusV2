using UnityEngine;

namespace SoulForge.Data
{
    [CreateAssetMenu(menuName = "SoulForge/Reward Definition", fileName = "RewardDefinition")]
    public sealed class RewardDefinition : ScriptableObject
    {
        [field: SerializeField] public string RewardId { get; private set; } = "reward_default";
        [field: SerializeField] public string DisplayName { get; private set; } = "Reward";
        [field: SerializeField] public string Description { get; private set; } = "Placeholder reward.";
    }
}
