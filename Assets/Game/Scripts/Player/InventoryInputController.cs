using UnityEngine;

namespace SoulForge.Player
{
    public sealed class InventoryInputController : MonoBehaviour
    {
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private KeyCode previousWeaponKey = KeyCode.Q;
        [SerializeField] private KeyCode nextWeaponKey = KeyCode.E;

        private void Awake()
        {
            if (playerInventory == null)
            {
                playerInventory = GetComponent<PlayerInventory>();
            }
        }

        private void Update()
        {
            if (playerInventory == null)
            {
                return;
            }

            if (Input.GetKeyDown(previousWeaponKey))
            {
                playerInventory.CycleWeapon(-1);
            }

            if (Input.GetKeyDown(nextWeaponKey))
            {
                playerInventory.CycleWeapon(1);
            }
        }
    }
}
