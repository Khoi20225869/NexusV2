using SoulForge.Data;
using SoulForge.Player;
using SoulForge.UI;
using UnityEngine;

namespace SoulForge.Bootstrap
{
    public sealed class CharacterSelectionController : MonoBehaviour
    {
        [SerializeField] private HeroRosterDefinition heroRoster;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerAim playerAim;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private WeaponRuntimeBridge weaponRuntimeBridge;
        [SerializeField] private SpumCharacterView characterView;
        [SerializeField] private ResultScreenPresenter resultScreenPresenter;

        public HeroRosterDefinition HeroRoster => heroRoster;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<PlayerController>();
            }

            if (playerAim == null)
            {
                playerAim = FindFirstObjectByType<PlayerAim>();
            }

            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>();
            }

            if (playerInventory == null)
            {
                playerInventory = FindFirstObjectByType<PlayerInventory>();
            }

            if (weaponRuntimeBridge == null)
            {
                weaponRuntimeBridge = FindFirstObjectByType<WeaponRuntimeBridge>();
            }

            if (characterView == null)
            {
                characterView = FindFirstObjectByType<SpumCharacterView>();
            }

            if (resultScreenPresenter == null)
            {
                resultScreenPresenter = FindFirstObjectByType<ResultScreenPresenter>(FindObjectsInactive.Include);
            }

            SetGameplayEnabled(false);
        }

        private void Start()
        {
            HeroDefinition heroDefinition = ResolveInitialHero();
            if (heroDefinition == null)
            {
                return;
            }

            SelectHero(heroDefinition);
        }

        public void SelectHero(HeroDefinition heroDefinition)
        {
            if (heroDefinition == null)
            {
                return;
            }

            playerController?.ApplyHero(heroDefinition);
            playerHealth?.ApplyHero(heroDefinition, true);
            weaponRuntimeBridge?.ApplyHero(heroDefinition);
            characterView?.ApplyHeroStyle(heroDefinition);
            characterView?.ApplyAppearance(RunSessionState.AppearanceSelection);
            playerInventory?.InitializeHero(heroDefinition);
            resultScreenPresenter?.Hide();
            SetGameplayEnabled(true);
            Time.timeScale = 1f;
        }

        private HeroDefinition ResolveInitialHero()
        {
            if (RunSessionState.HasHeroSelection)
            {
                return RunSessionState.SelectedHero;
            }

            HeroDefinition fallbackHero = heroRoster != null && heroRoster.Heroes.Count > 0
                ? heroRoster.Heroes[0]
                : null;

            if (RunSessionState.TryRestoreSavedProfile(heroRoster, fallbackHero))
            {
                return RunSessionState.SelectedHero;
            }

            if (fallbackHero != null)
            {
                RunSessionState.SelectHero(fallbackHero);
                return fallbackHero;
            }

            return null;
        }

        private void SetGameplayEnabled(bool isEnabled)
        {
            if (playerController != null)
            {
                playerController.enabled = isEnabled;
            }

            if (playerAim != null)
            {
                playerAim.enabled = isEnabled;
            }
        }
    }
}
