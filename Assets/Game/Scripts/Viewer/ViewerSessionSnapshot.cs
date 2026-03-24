using System;

namespace SoulForge.Viewer
{
    [Serializable]
    public sealed class ViewerSessionSnapshot
    {
        public string SessionId;
        public int RoomIndex;
        public string RoomPhase;
        public float HostHp;
        public float HostShield;
        public int AliveEnemyCount;
        public int QueueCount;
        public int RoomBudget;
    }
}
