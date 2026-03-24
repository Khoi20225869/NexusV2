using SoulForge.Bootstrap;
using SoulForge.Player;
using UnityEngine;

namespace SoulForge.Rooms
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class RoomTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private RunController runController;
        [SerializeField] private RoomController targetRoom;
        [SerializeField] private bool requireSourceRoomCleared = true;
        [SerializeField] private RoomController sourceRoom;

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

            if (requireSourceRoomCleared && sourceRoom != null && !sourceRoom.IsCleared)
            {
                return;
            }

            runController?.EnterRoom(targetRoom);
        }
    }
}
