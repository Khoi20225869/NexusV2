using SoulForge.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace SoulForge.UI
{
    public sealed class ResultScreenButtonsPresenter : MonoBehaviour
    {
        [SerializeField] private RunResetController runResetController;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button hubButton;

        private void Awake()
        {
            if (runResetController == null)
            {
                runResetController = FindFirstObjectByType<RunResetController>(FindObjectsInactive.Include);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (hubButton != null)
            {
                hubButton.onClick.AddListener(OnHubClicked);
            }
        }

        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
            }

            if (hubButton != null)
            {
                hubButton.onClick.RemoveListener(OnHubClicked);
            }
        }

        private void OnRestartClicked()
        {
            runResetController?.RestartRun();
        }

        private void OnHubClicked()
        {
            runResetController?.ReturnToHub();
        }
    }
}
