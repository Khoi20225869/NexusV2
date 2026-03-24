using System;

namespace SoulForge.Viewer
{
    [Serializable]
    public sealed class ViewerSessionSnapshot
    {
        public string SessionId;
        public string ViewerId;
        public int RoomIndex;
        public string RoomPhase;
        public float HostHp;
        public float HostShield;
        public int AliveEnemyCount;
        public int QueueCount;
        public int RoomBudget;
        public int ViewerBalance;
    }
}
