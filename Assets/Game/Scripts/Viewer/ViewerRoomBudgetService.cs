using SoulForge.Economy;
using UnityEngine;
using UnityEngine.Events;

namespace SoulForge.Viewer
{
    public sealed class ViewerRoomBudgetService : MonoBehaviour
    {
        [System.Serializable]
        public sealed class BudgetChangedEvent : UnityEvent<int> { }

        [SerializeField] private ViewerEconomyConfig config;
        [SerializeField] private int currentBudget;
        [SerializeField] private BudgetChangedEvent onBudgetChanged;

        public int CurrentBudget => currentBudget;

        public event System.Action<int> BudgetChanged;

        private void Start()
        {
            ResetBudget();
        }

        public void ResetBudget()
        {
            currentBudget = config != null ? config.DefaultRoomBudget : 5;
            BudgetChanged?.Invoke(currentBudget);
            onBudgetChanged?.Invoke(currentBudget);
        }

        public bool CanSpend(int budgetCost)
        {
            return currentBudget >= Mathf.Max(0, budgetCost);
        }

        public bool TrySpend(int budgetCost)
        {
            budgetCost = Mathf.Max(0, budgetCost);
            if (currentBudget < budgetCost)
            {
                return false;
            }

            currentBudget -= budgetCost;
            BudgetChanged?.Invoke(currentBudget);
            onBudgetChanged?.Invoke(currentBudget);
            return true;
        }

        public void AddBudget(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentBudget += amount;
            BudgetChanged?.Invoke(currentBudget);
            onBudgetChanged?.Invoke(currentBudget);
        }
    }
}
