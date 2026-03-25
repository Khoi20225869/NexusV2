using SoulForge.Data;
using SoulForge.Hub;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SoulForge.Bootstrap
{
    public sealed class HubController : MonoBehaviour
    {
        [System.Serializable]
        private struct HeroButtonView
        {
            public Button Button;
            public Image Frame;
            public TMP_Text Label;
            public TMP_Text Subtitle;
        }

        [SerializeField] private HeroRosterDefinition heroRoster;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text currentHeroText;
        [SerializeField] private TMP_Text heroStatsText;
        [SerializeField] private Button startRunButton;
        [SerializeField] private HeroButtonView[] heroButtons;
        [SerializeField] private HubHeroPreviewController[] previewControllers;
        [SerializeField] private string runSceneName = "Floor_01";

        private HeroDefinition selectedHero;

        private void Awake()
        {
            BindButtons();
            SelectInitialHero();
            RefreshTexts();
            RefreshHeroCards();
        }

        private void OnDestroy()
        {
            if (startRunButton != null)
            {
                startRunButton.onClick.RemoveListener(StartRun);
            }

            for (int i = 0; i < heroButtons.Length; i++)
            {
                if (heroButtons[i].Button != null)
                {
                    heroButtons[i].Button.onClick.RemoveAllListeners();
                }
            }
        }

        public void SelectHero(HeroDefinition heroDefinition)
        {
            if (heroDefinition == null)
            {
                return;
            }

            selectedHero = heroDefinition;
            RunSessionState.SelectHero(heroDefinition);
            RefreshTexts();
            RefreshHeroCards();
        }

        public void StartRun()
        {
            if (selectedHero == null && heroRoster != null && heroRoster.Heroes.Count > 0)
            {
                SelectHero(heroRoster.Heroes[0]);
            }

            if (!Application.CanStreamedLevelBeLoaded(runSceneName))
            {
                Debug.LogWarning($"Run scene '{runSceneName}' is not loadable.");
                return;
            }

            SceneManager.LoadScene(runSceneName);
        }

        private void BindButtons()
        {
            if (startRunButton != null)
            {
                startRunButton.onClick.RemoveListener(StartRun);
                startRunButton.onClick.AddListener(StartRun);
            }

            for (int i = 0; i < heroButtons.Length; i++)
            {
                HeroButtonView view = heroButtons[i];
                if (view.Button == null)
                {
                    continue;
                }

                view.Button.onClick.RemoveAllListeners();
                int capturedIndex = i;

                bool valid = heroRoster != null && capturedIndex < heroRoster.Heroes.Count && heroRoster.Heroes[capturedIndex] != null;
                view.Button.gameObject.SetActive(valid);
                view.Button.interactable = valid;

                if (view.Label != null)
                {
                    view.Label.text = valid ? heroRoster.Heroes[capturedIndex].DisplayName : "Empty";
                }

                if (view.Subtitle != null)
                {
                    view.Subtitle.text = valid && heroRoster.Heroes[capturedIndex].StartingWeapon != null
                        ? heroRoster.Heroes[capturedIndex].StartingWeapon.DisplayName
                        : "No Weapon";
                }

                if (valid)
                {
                    view.Button.onClick.AddListener(() => SelectHero(heroRoster.Heroes[capturedIndex]));
                }
            }
        }

        private void SelectInitialHero()
        {
            if (RunSessionState.HasHeroSelection)
            {
                selectedHero = RunSessionState.SelectedHero;
                return;
            }

            if (heroRoster != null && heroRoster.Heroes.Count > 0)
            {
                SelectHero(heroRoster.Heroes[0]);
            }
        }

        private void RefreshTexts()
        {
            if (titleText != null)
            {
                titleText.text = "SoulForge Hub";
            }

            if (descriptionText != null)
            {
                descriptionText.text = selectedHero != null
                    ? selectedHero.Description
                    : "Choose a hero before entering the run.";
            }

            if (currentHeroText != null)
            {
                currentHeroText.text = selectedHero != null
                    ? $"Selected: {selectedHero.DisplayName}"
                    : "Selected: None";
            }

            if (heroStatsText != null)
            {
                heroStatsText.text = selectedHero != null
                    ? $"HP {selectedHero.MaxHealth:0}   Shield {selectedHero.MaxShield:0}\nMove {selectedHero.MoveSpeed:0.0}   Dash CD {selectedHero.DashCooldown:0.0}s\nStart: {(selectedHero.StartingWeapon != null ? selectedHero.StartingWeapon.DisplayName : "Default Blade")}"
                    : "Pick a hero to inspect combat stats.";
            }
        }

        private void RefreshHeroCards()
        {
            for (int i = 0; i < heroButtons.Length; i++)
            {
                bool valid = heroRoster != null && i < heroRoster.Heroes.Count && heroRoster.Heroes[i] != null;
                bool isSelected = valid && heroRoster.Heroes[i] == selectedHero;

                if (heroButtons[i].Frame != null)
                {
                    heroButtons[i].Frame.color = isSelected
                        ? new Color(0.72f, 0.54f, 0.18f, 0.96f)
                        : new Color(0.12f, 0.18f, 0.24f, 0.92f);
                }

                if (heroButtons[i].Label != null)
                {
                    heroButtons[i].Label.color = isSelected ? new Color(1f, 0.93f, 0.75f, 1f) : Color.white;
                }

                if (heroButtons[i].Subtitle != null)
                {
                    heroButtons[i].Subtitle.color = isSelected ? new Color(1f, 0.83f, 0.49f, 1f) : new Color(0.74f, 0.82f, 0.88f, 1f);
                }

                if (previewControllers != null && i < previewControllers.Length && previewControllers[i] != null)
                {
                    previewControllers[i].SetSelected(isSelected);
                }
            }
        }
    }
}
