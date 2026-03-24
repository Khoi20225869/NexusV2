using SoulForge.Player;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class HudPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;

        private void Awake()
        {
            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>();
            }
        }

        public PlayerHealth PlayerHealth => playerHealth;
    }
}
