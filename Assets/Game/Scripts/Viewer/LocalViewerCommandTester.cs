using SoulForge.Economy;
using UnityEngine;

namespace SoulForge.Viewer
{
    public sealed class LocalViewerCommandTester : MonoBehaviour
    {
        [SerializeField] private ViewerActionExecutor executor;
        [SerializeField] private ViewerEconomyService economyService;
        [SerializeField] private string viewerId = "local_viewer";
        [SerializeField] private KeyCode grantCurrencyKey = KeyCode.Alpha0;
        [SerializeField] private KeyCode weakEnemyKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode eliteEnemyKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode healKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode weaponKey = KeyCode.Alpha4;
        [SerializeField] private int grantAmount = 100;

        private int commandCounter;
        public string ViewerId => viewerId;

        private void Awake()
        {
            if (executor == null)
            {
                executor = FindFirstObjectByType<ViewerActionExecutor>();
            }

            if (economyService == null)
            {
                economyService = FindFirstObjectByType<ViewerEconomyService>();
            }
        }

        private void Start()
        {
            economyService?.GrantJoinReward(viewerId);
        }

        private void Update()
        {
            if (Input.GetKeyDown(grantCurrencyKey))
            {
                economyService?.AddCurrency(viewerId, grantAmount);
            }

            if (Input.GetKeyDown(weakEnemyKey))
            {
                SubmitAction("spawn_weak_enemy");
            }

            if (Input.GetKeyDown(eliteEnemyKey))
            {
                SubmitAction("spawn_elite_enemy");
            }

            if (Input.GetKeyDown(healKey))
            {
                SubmitAction("drop_heal");
            }

            if (Input.GetKeyDown(weaponKey))
            {
                SubmitAction("drop_random_weapon");
            }
        }

        public bool SubmitAction(string actionId, string targetId = "")
        {
            if (executor == null)
            {
                return false;
            }

            commandCounter++;
            ViewerCommand command = new($"cmd_{commandCounter:000}", viewerId, actionId, targetId);
            return executor.TrySubmit(command);
        }
    }
}
