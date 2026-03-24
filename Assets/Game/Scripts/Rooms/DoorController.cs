using UnityEngine;

namespace SoulForge.Rooms
{
    public sealed class DoorController : MonoBehaviour
    {
        [SerializeField] private Collider2D blockingCollider;
        [SerializeField] private GameObject closedVisual;
        [SerializeField] private GameObject openVisual;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            ApplyState(IsOpen);
        }

        public void OpenDoor()
        {
            IsOpen = true;
            ApplyState(IsOpen);
        }

        public void CloseDoor()
        {
            IsOpen = false;
            ApplyState(IsOpen);
        }

        private void ApplyState(bool isOpen)
        {
            if (blockingCollider != null)
            {
                blockingCollider.enabled = !isOpen;
            }

            if (closedVisual != null)
            {
                closedVisual.SetActive(!isOpen);
            }

            if (openVisual != null)
            {
                openVisual.SetActive(isOpen);
            }
        }
    }
}
