using SoulForge.Data;
using SoulForge.Economy;
using SoulForge.Viewer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulForge.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class ViewerStoreButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image background;
        [SerializeField] private Color availableColor = new(0.18f, 0.45f, 0.24f, 1f);
        [SerializeField] private Color unavailableColor = new(0.33f, 0.18f, 0.18f, 1f);

        private Button button;
        private ViewerActionDefinition actionDefinition;
        private ViewerActionExecutor executor;
        private ViewerActionValidator validator;
        private ViewerRoomBudgetService budgetService;
        private LocalViewerCommandTester localViewerCommandTester;
        private ViewerWebSocketClient remoteViewerClient;
        private ViewerEconomyService economyService;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(Submit);
        }

        private void Update()
        {
            RefreshState();
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(Submit);
            }
        }

        public void Setup(
            ViewerActionDefinition action,
            ViewerActionExecutor targetExecutor,
            ViewerActionValidator targetValidator,
            ViewerEconomyService targetEconomyService,
            ViewerRoomBudgetService targetBudgetService,
            LocalViewerCommandTester targetLocalViewerCommandTester,
            ViewerWebSocketClient targetRemoteViewerClient)
        {
            actionDefinition = action;
            executor = targetExecutor;
            validator = targetValidator;
            economyService = targetEconomyService;
            budgetService = targetBudgetService;
            localViewerCommandTester = targetLocalViewerCommandTester;
            remoteViewerClient = targetRemoteViewerClient;

            if (labelText != null)
            {
                labelText.text = action != null && !string.IsNullOrWhiteSpace(action.DisplayName) ? action.DisplayName : action != null ? action.ActionId : "missing_action";
            }

            if (priceText != null)
            {
                priceText.text = action != null ? $"{action.Price} C" : "-";
            }

            if (descriptionText != null)
            {
                descriptionText.text = action != null ? action.Description : string.Empty;
            }

            if (background != null && action != null)
            {
                background.color = action.AccentColor;
            }

            RefreshState();
        }

        private void Submit()
        {
            if (actionDefinition == null)
            {
                return;
            }

            if (localViewerCommandTester != null)
            {
                localViewerCommandTester.SubmitAction(actionDefinition.ActionId, actionDefinition.TargetId);
                return;
            }

            remoteViewerClient?.SubmitAction(actionDefinition.ActionId, actionDefinition.TargetId);
        }

        private void RefreshState()
        {
            if (button == null || actionDefinition == null)
            {
                return;
            }

            if (localViewerCommandTester == null)
            {
                bool ready = remoteViewerClient != null && remoteViewerClient.IsConnected;
                button.interactable = ready;

                if (background != null)
                {
                    background.color = ready ? Color.Lerp(actionDefinition.AccentColor, availableColor, 0.55f) : unavailableColor;
                }

                if (stateText != null)
                {
                    stateText.text = ready ? "Send" : "Offline";
                }

                return;
            }

            string viewerId = localViewerCommandTester.ViewerId;
            float cooldownRemaining = validator != null ? validator.GetCooldownRemaining(actionDefinition.ActionId) : 0f;
            int balance = economyService != null ? economyService.GetBalance(viewerId) : 0;
            int budget = budgetService != null ? budgetService.CurrentBudget : 0;
            bool canAfford = balance >= actionDefinition.Price;
            bool canBudget = budget >= actionDefinition.BudgetCost;
            bool canPurchase = validator != null ? validator.CanPurchase(viewerId, actionDefinition) : canAfford && canBudget;

            button.interactable = canPurchase;

            if (background != null)
            {
                background.color = canPurchase ? Color.Lerp(actionDefinition.AccentColor, availableColor, 0.55f) : unavailableColor;
            }

            if (stateText == null)
            {
                return;
            }

            if (cooldownRemaining > 0f)
            {
                stateText.text = $"CD {cooldownRemaining:0.0}s";
                return;
            }

            if (!canAfford)
            {
                stateText.text = "Need Crowns";
                return;
            }

            if (!canBudget)
            {
                stateText.text = "No Budget";
                return;
            }

            stateText.text = "Ready";
        }
    }
}
