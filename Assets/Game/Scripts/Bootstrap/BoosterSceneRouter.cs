using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoulForge.Bootstrap
{
    public sealed class BoosterSceneRouter : MonoBehaviour
    {
        [SerializeField] private string firstTimeSceneName = "Hub";
        [SerializeField] private string returningPlayerSceneName = "Floor_01";

        private void Start()
        {
            string targetSceneName = RunSessionState.HasSavedProfile
                ? returningPlayerSceneName
                : firstTimeSceneName;

            if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                Debug.LogWarning($"Booster scene could not load '{targetSceneName}'.");
                return;
            }

            SceneManager.LoadScene(targetSceneName);
        }
    }
}
