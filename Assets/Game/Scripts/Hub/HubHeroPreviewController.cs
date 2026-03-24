using SoulForge.Data;
using SoulForge.Bootstrap;
using SoulForge.UI;
using UnityEngine;

namespace SoulForge.Hub
{
    public sealed class HubHeroPreviewController : MonoBehaviour
    {
        [SerializeField] private SpumCharacterView characterView;
        [SerializeField] private Vector3 selectedScale = new(6.2f, 6.2f, 6.2f);
        [SerializeField] private Vector3 unselectedScale = new(5.4f, 5.4f, 5.4f);

        private bool selected;

        private void Awake()
        {
            if (characterView == null)
            {
                characterView = GetComponent<SpumCharacterView>();
            }

            if (characterView == null)
            {
                characterView = GetComponentInChildren<SpumCharacterView>(true);
            }
        }

        private void OnEnable()
        {
            ApplySelectionVisual();
            characterView?.ResetState();
        }

        private void Update()
        {
        }

        public void Configure(HeroDefinition heroDefinition)
        {
            if (characterView == null || heroDefinition == null)
            {
                return;
            }

            characterView.ApplyHeroStyle(heroDefinition);
            characterView.ResetState();
        }

        public void Configure(HeroDefinition heroDefinition, HeroAppearanceSelection appearanceSelection)
        {
            if (characterView == null || heroDefinition == null)
            {
                return;
            }

            characterView.ApplyHeroStyle(heroDefinition);
            characterView.ApplyAppearance(appearanceSelection);
            characterView.ResetState();
        }

        public void SetSelected(bool isSelected)
        {
            selected = isSelected;
            ApplySelectionVisual();
            characterView?.ResetState();
        }

        private void ApplySelectionVisual()
        {
            transform.localScale = selected ? selectedScale : unselectedScale;
        }
    }
}
