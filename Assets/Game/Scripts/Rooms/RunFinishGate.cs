using SoulForge.Bootstrap;
using SoulForge.Player;
using UnityEngine;

namespace SoulForge.Rooms
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class RunFinishGate : MonoBehaviour
    {
        [SerializeField] private RunController runController;
        [SerializeField] private RoomController ownerRoom;
        [SerializeField] private bool requireRoomCleared = true;

        private void Awake()
        {
            if (runController == null)
            {
                runController = FindFirstObjectByType<RunController>();
            }

            Collider2D triggerCollider = GetComponent<Collider2D>();
            triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerHealth>() == null)
            {
                return;
            }

            if (requireRoomCleared && ownerRoom != null && !ownerRoom.IsCleared)
            {
                return;
            }

            runController?.CompleteRun();
        }
    }
}
