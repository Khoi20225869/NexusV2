using UnityEngine;

namespace SoulForge.Economy
{
    [CreateAssetMenu(menuName = "SoulForge/Viewer Economy Config", fileName = "ViewerEconomyConfig")]
    public sealed class ViewerEconomyConfig : ScriptableObject
    {
        [field: SerializeField] public int JoinReward { get; private set; } = 100;
        [field: SerializeField] public int WatchRewardPerMinute { get; private set; } = 10;
        [field: SerializeField] public int RoomClearReward { get; private set; } = 15;
        [field: SerializeField] public int BossClearReward { get; private set; } = 50;
        [field: SerializeField] public int MaxQueuedCommands { get; private set; } = 8;
        [field: SerializeField] public int DefaultRoomBudget { get; private set; } = 5;
    }
}
