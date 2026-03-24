using System;

namespace SoulForge.Viewer
{
    [Serializable]
    public sealed class ViewerSessionDelta
    {
        public string EventType;
        public string RoomPhase;
        public float HostHp;
        public int AliveEnemyCount;
        public int QueueCount;
        public int RoomBudget;
    }
}
