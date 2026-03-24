using SoulForge.Player;
using SoulForge.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoulForge.Bootstrap
{
    public sealed class RunResetController : MonoBehaviour
    {
        [SerializeField] private RunController runController;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private ResultScreenPresenter resultScreenPresenter;
        [SerializeField] private string hubSceneName = "Hub";

        private void Awake()
        {
            if (runController == null)
            {
                runController = FindFirstObjectByType<RunController>();
            }

            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>();
            }

            if (resultScreenPresenter == null)
            {
                resultScreenPresenter = FindFirstObjectByType<ResultScreenPresenter>(FindObjectsInactive.Include);
            }
        }

        private void OnEnable()
        {
            if (runController != null)
            {
                runController.RunCompleted += OnRunCompleted;
            }

            if (playerHealth != null)
            {
                playerHealth.Died += OnPlayerDied;
            }
        }

        private void OnDisable()
        {
            if (runController != null)
            {
                runController.RunCompleted -= OnRunCompleted;
            }

            if (playerHealth != null)
            {
                playerHealth.Died -= OnPlayerDied;
            }
        }

        public void OnPlayerDied()
        {
            resultScreenPresenter?.ShowDefeat();
        }

        public void OnRunCompleted()
        {
            resultScreenPresenter?.ShowVictory();
        }

        public void RestartRun()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.name);
        }

        public void ReturnToHub()
        {
            if (string.IsNullOrWhiteSpace(hubSceneName))
            {
                RestartRun();
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(hubSceneName))
            {
                Debug.LogWarning($"Hub scene '{hubSceneName}' is not loadable. Restarting current run instead.");
                RestartRun();
                return;
            }

            SceneManager.LoadScene(hubSceneName);
        }
    }
}
