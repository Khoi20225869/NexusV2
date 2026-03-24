using System;
using SoulForge.Data;
using SoulForge.Hub;
using SoulForge.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SoulForge.Bootstrap
{
    public sealed class HubCustomizerController : MonoBehaviour
    {
        [Serializable]
        private struct CustomizationRowView
        {
            public TMP_Text Label;
            public TMP_Text Value;
            public Button PreviousButton;
            public Button NextButton;
        }

        [SerializeField] private HeroDefinition baseHeroDefinition;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text heroStatsText;
        [SerializeField] private Button startRunButton;
        [SerializeField] private HubHeroPreviewController previewController;
        [SerializeField] private SpumCharacterView previewCharacterView;
        [SerializeField] private CustomizationRowView[] customizationRows;
        [SerializeField] private string runSceneName = "Run_Prototype";

        private HeroAppearanceSelection appearanceSelection;

        private void Awake()
        {
            appearanceSelection = RunSessionState.AppearanceSelection != null
                ? RunSessionState.AppearanceSelection.Clone()
                : new HeroAppearanceSelection();

            if (previewCharacterView == null && previewController != null)
            {
                previewCharacterView = previewController.GetComponent<SpumCharacterView>();
            }

            if (previewCharacterView == null && previewController != null)
            {
                previewCharacterView = previewController.GetComponentInChildren<SpumCharacterView>(true);
            }
        }

        private void Start()
        {
            if (previewController == null)
            {
                previewController = FindFirstObjectByType<HubHeroPreviewController>(FindObjectsInactive.Include);
            }

            if (previewCharacterView == null)
            {
                previewCharacterView = previewController != null
                    ? previewController.GetComponentInChildren<SpumCharacterView>(true)
                    : FindFirstObjectByType<SpumCharacterView>(FindObjectsInactive.Include);
            }

            BindButtons();
            ApplyToPreview();
            RefreshTexts();
        }

        private void OnDestroy()
        {
            if (startRunButton != null)
            {
                startRunButton.onClick.RemoveListener(StartRun);
            }

            for (int i = 0; i < customizationRows.Length; i++)
            {
                if (customizationRows[i].PreviousButton != null)
                {
                    customizationRows[i].PreviousButton.onClick.RemoveAllListeners();
                }

                if (customizationRows[i].NextButton != null)
                {
                    customizationRows[i].NextButton.onClick.RemoveAllListeners();
                }
            }
        }

        private void BindButtons()
        {
            if (startRunButton != null)
            {
                startRunButton.onClick.RemoveListener(StartRun);
                startRunButton.onClick.AddListener(StartRun);
            }

            for (int i = 0; i < customizationRows.Length; i++)
            {
                int captured = i;
                if (customizationRows[i].PreviousButton != null)
                {
                    customizationRows[i].PreviousButton.onClick.RemoveAllListeners();
                    customizationRows[i].PreviousButton.onClick.AddListener(() => ChangeOption(captured, -1));
                }

                if (customizationRows[i].NextButton != null)
                {
                    customizationRows[i].NextButton.onClick.RemoveAllListeners();
                    customizationRows[i].NextButton.onClick.AddListener(() => ChangeOption(captured, 1));
                }
            }
        }

        private void ChangeOption(int rowIndex, int delta)
        {
            switch (rowIndex)
            {
                case 0:
                    appearanceSelection.HairStyleIndex = SpumAppearanceCatalog.Wrap(appearanceSelection.HairStyleIndex + delta, SpumAppearanceCatalog.HairStyles.Length);
                    break;
                case 1:
                    appearanceSelection.FaceHairIndex = SpumAppearanceCatalog.Wrap(appearanceSelection.FaceHairIndex + delta, SpumAppearanceCatalog.FaceHairs.Length);
                    break;
                case 2:
                    appearanceSelection.HelmetIndex = SpumAppearanceCatalog.Wrap(appearanceSelection.HelmetIndex + delta, SpumAppearanceCatalog.Helmets.Length);
                    break;
                case 3:
                    appearanceSelection.ArmorIndex = SpumAppearanceCatalog.Wrap(appearanceSelection.ArmorIndex + delta, SpumAppearanceCatalog.Armors.Length);
                    break;
                case 4:
                    appearanceSelection.PantIndex = SpumAppearanceCatalog.Wrap(appearanceSelection.PantIndex + delta, SpumAppearanceCatalog.Pants.Length);
                    break;
                case 5:
                    appearanceSelection.WeaponIndex = SpumAppearanceCatalog.Wrap(appearanceSelection.WeaponIndex + delta, SpumAppearanceCatalog.Weapons.Length);
                    break;
                case 6:
                    appearanceSelection.BackIndex = SpumAppearanceCatalog.Wrap(appearanceSelection.BackIndex + delta, SpumAppearanceCatalog.BackItems.Length);
                    break;
                case 7:
                    appearanceSelection.EyeColorIndex = SpumAppearanceCatalog.Wrap(appearanceSelection.EyeColorIndex + delta, SpumAppearanceCatalog.EyeColors.Length);
                    break;
                case 8:
                    appearanceSelection.HairColorIndex = SpumAppearanceCatalog.Wrap(appearanceSelection.HairColorIndex + delta, SpumAppearanceCatalog.HairColors.Length);
                    break;
                case 9:
                    appearanceSelection.ClothColorIndex = SpumAppearanceCatalog.Wrap(appearanceSelection.ClothColorIndex + delta, SpumAppearanceCatalog.ClothColors.Length);
                    break;
            }

            ApplyToPreview();
            RefreshTexts();
        }

        private void ApplyToPreview()
        {
            if (baseHeroDefinition != null)
            {
                RunSessionState.SelectHero(baseHeroDefinition);
            }

            RunSessionState.SetAppearance(appearanceSelection);

            if (previewController != null && baseHeroDefinition != null)
            {
                previewController.Configure(baseHeroDefinition, appearanceSelection);
            }
            else if (previewCharacterView != null && baseHeroDefinition != null)
            {
                previewCharacterView.ApplyHeroStyle(baseHeroDefinition);
                previewCharacterView.ApplyAppearance(appearanceSelection);
                previewCharacterView.ResetState();
            }

            previewController?.SetSelected(true);
        }

        private void RefreshTexts()
        {
            if (titleText != null)
            {
                titleText.text = "Forge Your Hero";
            }

            if (descriptionText != null)
            {
                descriptionText.text = "Use the arrows to customize your adventurer before entering the run.";
            }

            if (summaryText != null)
            {
                summaryText.text = "Custom Adventurer";
            }

            if (heroStatsText != null && baseHeroDefinition != null)
            {
                heroStatsText.text = $"HP {baseHeroDefinition.MaxHealth:0}   Shield {baseHeroDefinition.MaxShield:0}\nMove {baseHeroDefinition.MoveSpeed:0.0}   Dash CD {baseHeroDefinition.DashCooldown:0.0}s\nStart: {(RunSessionState.ShopWeaponOverride != null ? RunSessionState.ShopWeaponOverride.DisplayName : baseHeroDefinition.StartingWeapon != null ? baseHeroDefinition.StartingWeapon.DisplayName : "Default")}";
            }

            SetRow(0, "Hair", SpumAppearanceCatalog.GetHairLabel(appearanceSelection.HairStyleIndex));
            SetRow(1, "FaceHair", SpumAppearanceCatalog.GetFaceHairLabel(appearanceSelection.FaceHairIndex));
            SetRow(2, "Helmet", SpumAppearanceCatalog.GetHelmetLabel(appearanceSelection.HelmetIndex));
            SetRow(3, "Armor", SpumAppearanceCatalog.GetArmorLabel(appearanceSelection.ArmorIndex));
            SetRow(4, "Pant", SpumAppearanceCatalog.GetPantLabel(appearanceSelection.PantIndex));
            SetRow(5, "Weapon", SpumAppearanceCatalog.GetWeaponLabel(appearanceSelection.WeaponIndex));
            SetRow(6, "Back", SpumAppearanceCatalog.GetBackLabel(appearanceSelection.BackIndex));
            SetRow(7, "Eye Color", SpumAppearanceCatalog.GetEyeColorLabel(appearanceSelection.EyeColorIndex));
            SetRow(8, "Hair Color", SpumAppearanceCatalog.GetHairColorLabel(appearanceSelection.HairColorIndex));
            SetRow(9, "Cloth Tone", SpumAppearanceCatalog.GetClothColorLabel(appearanceSelection.ClothColorIndex));
        }

        private void SetRow(int index, string label, string value)
        {
            if (index < 0 || index >= customizationRows.Length)
            {
                return;
            }

            if (customizationRows[index].Label != null)
            {
                customizationRows[index].Label.text = label;
            }

            if (customizationRows[index].Value != null)
            {
                customizationRows[index].Value.text = value;
            }
        }

        public void StartRun()
        {
            if (baseHeroDefinition != null)
            {
                RunSessionState.SelectHero(baseHeroDefinition);
            }

            RunSessionState.SetAppearance(appearanceSelection);

            if (!Application.CanStreamedLevelBeLoaded(runSceneName))
            {
                Debug.LogWarning($"Run scene '{runSceneName}' is not loadable.");
                return;
            }

            SceneManager.LoadScene(runSceneName);
        }
    }
}
