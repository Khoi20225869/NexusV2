using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulForge.UI
{
    public sealed class InventoryPresenter : MonoBehaviour
    {
        [SerializeField] private Button toggleButton;
        [SerializeField] private TMP_Text toggleButtonText;

        private void Awake()
        {
            Refresh();
        }

        private void OnEnable()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(OnToggleClicked);
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(OnToggleClicked);
            }
        }

        public void Refresh()
        {
            if (toggleButtonText != null)
            {
                toggleButtonText.text = "Inventory";
            }
        }

        private void OnToggleClicked()
        {
            Refresh();
        }
    }
}
