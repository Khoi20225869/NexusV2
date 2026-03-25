using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SoulForge.Bootstrap
{
    public sealed class FloorSceneFader : MonoBehaviour
    {
        private static FloorSceneFader instance;

        private Canvas canvas;
        private GraphicRaycaster graphicRaycaster;
        private Image fadeImage;
        private Coroutine activeTransition;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            EnsureInstance();
            instance.HandleSceneLoaded(SceneManager.GetActiveScene().name);
        }

        public static void FadeToScene(string sceneName, float duration = 0.45f)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return;
            }

            EnsureInstance();
            instance.BeginSceneTransition(sceneName, duration);
        }

        private static void EnsureInstance()
        {
            if (instance != null)
            {
                return;
            }

            GameObject root = new("FloorSceneFader");
            DontDestroyOnLoad(root);
            instance = root.AddComponent<FloorSceneFader>();
            instance.InitializeVisuals();
        }

        private void InitializeVisuals()
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            gameObject.AddComponent<CanvasScaler>();
            graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();

            GameObject imageObject = new("Fade");
            imageObject.transform.SetParent(transform, false);

            RectTransform rect = imageObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            fadeImage = imageObject.AddComponent<Image>();
            fadeImage.color = Color.black;
            SetAlpha(0f);
            SetInputBlocked(false);

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void HandleSceneLoaded(string sceneName)
        {
            if (!IsFloorScene(sceneName))
            {
                SetAlpha(0f);
                SetInputBlocked(false);
                return;
            }

            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
            }

            activeTransition = StartCoroutine(FadeRoutine(1f, 0f, 0.45f));
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            HandleSceneLoaded(scene.name);
        }

        private void BeginSceneTransition(string sceneName, float duration)
        {
            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
            }

            activeTransition = StartCoroutine(FadeOutAndLoad(sceneName, duration));
        }

        private IEnumerator FadeOutAndLoad(string sceneName, float duration)
        {
            yield return FadeRoutine(GetAlpha(), 1f, duration);
            SceneManager.LoadScene(sceneName);
            activeTransition = null;
        }

        private IEnumerator FadeRoutine(float from, float to, float duration)
        {
            if (fadeImage == null)
            {
                yield break;
            }

            SetInputBlocked(IsFloorScene(SceneManager.GetActiveScene().name));
            float time = 0f;
            duration = Mathf.Max(0.01f, duration);

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(from, to, time / duration));
                yield return null;
            }

            SetAlpha(to);
            SetInputBlocked(IsFloorScene(SceneManager.GetActiveScene().name) && to > 0.001f);
        }

        private void SetAlpha(float alpha)
        {
            if (fadeImage == null)
            {
                return;
            }

            Color color = fadeImage.color;
            color.a = Mathf.Clamp01(alpha);
            fadeImage.color = color;
        }

        private float GetAlpha()
        {
            return fadeImage != null ? fadeImage.color.a : 0f;
        }

        private void SetInputBlocked(bool blocked)
        {
            if (graphicRaycaster != null)
            {
                graphicRaycaster.enabled = blocked;
            }

            if (fadeImage != null)
            {
                fadeImage.raycastTarget = blocked;
            }
        }

        private static bool IsFloorScene(string sceneName)
        {
            return !string.IsNullOrWhiteSpace(sceneName) && sceneName.StartsWith("Floor_", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
