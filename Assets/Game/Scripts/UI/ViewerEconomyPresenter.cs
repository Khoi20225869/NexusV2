using SoulForge.Economy;
using SoulForge.Viewer;
using TMPro;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class ViewerEconomyPresenter : MonoBehaviour
    {
        [SerializeField] private ViewerEconomyService economyService;
        [SerializeField] private ViewerRoomBudgetService roomBudgetService;
        [SerializeField] private LocalViewerCommandTester localViewerCommandTester;
        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private TMP_Text budgetText;

        private void Awake()
        {
            if (economyService == null)
            {
                economyService = FindFirstObjectByType<ViewerEconomyService>();
            }

            if (roomBudgetService == null)
            {
                roomBudgetService = FindFirstObjectByType<ViewerRoomBudgetService>();
            }

            if (localViewerCommandTester == null)
            {
                localViewerCommandTester = FindFirstObjectByType<LocalViewerCommandTester>();
            }
        }

        private void OnEnable()
        {
            if (economyService == null)
            {
                return;
            }

            economyService.BalanceChanged += OnBalanceChanged;
        }

        private void OnDisable()
        {
            if (economyService == null)
            {
                return;
            }

            economyService.BalanceChanged -= OnBalanceChanged;
        }

        private void Update()
        {
            if (budgetText != null && roomBudgetService != null)
            {
                budgetText.text = $"Budget: {roomBudgetService.CurrentBudget}";
            }

            if (balanceText != null && economyService != null && localViewerCommandTester != null)
            {
                balanceText.text = $"Crowns: {economyService.GetBalance(localViewerCommandTester.ViewerId)}";
            }
        }

        public void OnBalanceChanged(string viewerId, int balance)
        {
            if (balanceText == null || localViewerCommandTester == null || viewerId != localViewerCommandTester.ViewerId)
            {
                return;
            }

            balanceText.text = $"Crowns: {balance}";
        }
    }
}
