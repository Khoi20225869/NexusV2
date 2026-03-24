using TMPro;
using UnityEngine;

namespace SoulForge.Hub
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class BlacksmithAltarInteractor : MonoBehaviour
    {
        [SerializeField] private HubShopController shopController;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private SpriteRenderer altarRenderer;
        [SerializeField] private Color idleColor = new(0.38f, 0.32f, 0.24f, 1f);
        [SerializeField] private Color hoverColor = new(0.78f, 0.66f, 0.34f, 1f);
        [SerializeField] private Color openColor = new(1f, 0.84f, 0.44f, 1f);
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private bool playerNearby;
        private bool pointerOver;

        private void Awake()
        {
            if (altarRenderer == null)
            {
                altarRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            SetPrompt("Click altar to open forge");
            RefreshVisual();
        }

        private void Update()
        {
            if (playerNearby && Input.GetKeyDown(interactKey))
            {
                ToggleShop();
            }
        }

        private void OnMouseDown()
        {
            ToggleShop();
        }

        private void OnMouseEnter()
        {
            pointerOver = true;
            RefreshVisual();
        }

        private void OnMouseExit()
        {
            pointerOver = false;
            RefreshVisual();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            playerNearby = true;
            SetPrompt($"Press {interactKey} or click altar");
            RefreshVisual();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            playerNearby = false;
            SetPrompt("Click altar to open forge");
            RefreshVisual();
        }

        public void ToggleShop()
        {
            if (shopController == null)
            {
                return;
            }

            shopController.ToggleShop();
            SetPrompt(shopController.IsOpen ? "Forge open" : "Forge closed");
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            if (altarRenderer == null)
            {
                return;
            }

            if (shopController != null && shopController.IsOpen)
            {
                altarRenderer.color = openColor;
                return;
            }

            altarRenderer.color = playerNearby || pointerOver ? hoverColor : idleColor;
        }

        private void SetPrompt(string message)
        {
            if (promptText != null)
            {
                promptText.text = message;
            }
        }
    }
}
