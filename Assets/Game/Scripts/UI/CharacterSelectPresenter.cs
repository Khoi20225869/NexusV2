using System;
using SoulForge.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulForge.UI
{
    public sealed class CharacterSelectPresenter : MonoBehaviour
    {
        [Serializable]
        private struct HeroButtonView
        {
            public Button Button;
            public TMP_Text Label;
        }

        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private HeroButtonView[] heroButtons;

        private Action<HeroDefinition> onSelected;
        private HeroDefinition[] currentHeroes = Array.Empty<HeroDefinition>();

        public void Show(HeroRosterDefinition roster, Action<HeroDefinition> onHeroSelected)
        {
            onSelected = onHeroSelected;
            currentHeroes = roster != null ? roster.Heroes.ToArray() : Array.Empty<HeroDefinition>();

            if (titleText != null)
            {
                titleText.text = "Choose Your Hero";
            }

            if (descriptionText != null)
            {
                descriptionText.text = currentHeroes.Length > 0 ? currentHeroes[0].Description : "No heroes configured.";
            }

            for (int i = 0; i < heroButtons.Length; i++)
            {
                HeroButtonView view = heroButtons[i];
                if (view.Button == null)
                {
                    continue;
                }

                int capturedIndex = i;
                view.Button.onClick.RemoveAllListeners();

                bool isActive = i < currentHeroes.Length && currentHeroes[i] != null;
                view.Button.gameObject.SetActive(isActive);
                view.Button.interactable = isActive;

                if (view.Label != null)
                {
                    view.Label.text = isActive ? currentHeroes[i].DisplayName : "Empty";
                }

                if (!isActive)
                {
                    continue;
                }

                view.Button.onClick.AddListener(() => SelectIndex(capturedIndex));
            }

            SetVisible(true);
        }

        public void Hide()
        {
            for (int i = 0; i < heroButtons.Length; i++)
            {
                if (heroButtons[i].Button != null)
                {
                    heroButtons[i].Button.onClick.RemoveAllListeners();
                }
            }

            SetVisible(false);
        }

        private void SelectIndex(int index)
        {
            if (index < 0 || index >= currentHeroes.Length)
            {
                return;
            }

            HeroDefinition hero = currentHeroes[index];
            if (descriptionText != null && hero != null)
            {
                descriptionText.text = hero.Description;
            }

            onSelected?.Invoke(hero);
        }

        private void SetVisible(bool isVisible)
        {
            if (root != null)
            {
                root.SetActive(isVisible);
                return;
            }

            gameObject.SetActive(isVisible);
        }
    }
}
