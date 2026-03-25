using SoulForge.Bootstrap;
using SoulForge.Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoulForge.Rooms
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class RunFinishGate : MonoBehaviour
    {
        [SerializeField] private RunController runController;
        [SerializeField] private RoomController ownerRoom;
        [SerializeField] private bool requireRoomCleared = false;
        [SerializeField] private float markerHeight = 1.15f;
        [SerializeField] private float markerScale = 0.75f;
        [SerializeField] private float guideDotSpacing = 0.55f;
        [SerializeField] private float guideDotScale = 0.28f;
        [SerializeField] private int maxGuideDots = 18;
        [SerializeField] private Color markerColor = new(1f, 0.87f, 0.35f, 1f);
        [SerializeField] private Color guideColor = new(1f, 0.9f, 0.45f, 1f);

        private readonly List<SpriteRenderer> guideDots = new();
        private Transform playerTransform;
        private SpriteRenderer markerRenderer;
        private bool transitioning;

        private void Awake()
        {
            if (runController == null)
            {
                runController = FindFirstObjectByType<RunController>();
            }

            Collider2D triggerCollider = GetComponent<Collider2D>();
            triggerCollider.isTrigger = true;
        }

        private void Start()
        {
            CreateMarker();
            CreateGuideDots();
        }

        private void Update()
        {
            if (transitioning)
            {
                SetGuideVisible(false);
                return;
            }

            if (playerTransform == null)
            {
                PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
                playerTransform = playerHealth != null ? playerHealth.transform : null;
            }

            AnimateMarker();
            UpdateGuideDots();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerHealth>() == null)
            {
                return;
            }

            if (requireRoomCleared && ownerRoom != null && !ownerRoom.IsCleared)
            {
                return;
            }

            if (transitioning)
            {
                return;
            }

            string nextSceneName = ResolveNextFloorSceneName();
            if (string.IsNullOrWhiteSpace(nextSceneName))
            {
                runController?.CompleteRun();
                return;
            }

            transitioning = true;
            FloorSceneFader.FadeToScene(nextSceneName);
        }

        private void CreateMarker()
        {
            GameObject marker = new("TargetMarker");
            marker.transform.SetParent(transform, false);
            marker.transform.localPosition = new Vector3(0f, markerHeight, 0f);
            markerRenderer = marker.AddComponent<SpriteRenderer>();
            markerRenderer.sprite = CreateRuntimeSprite();
            markerRenderer.color = markerColor;
            markerRenderer.sortingOrder = 20;
            marker.transform.localScale = new Vector3(markerScale, markerScale, 1f);
        }

        private void CreateGuideDots()
        {
            for (int i = 0; i < maxGuideDots; i++)
            {
                GameObject dot = new($"GuideDot_{i + 1:00}");
                dot.transform.SetParent(transform, false);
                SpriteRenderer renderer = dot.AddComponent<SpriteRenderer>();
                renderer.sprite = CreateRuntimeSprite();
                renderer.color = guideColor;
                renderer.sortingOrder = 10;
                dot.transform.localScale = new Vector3(guideDotScale, guideDotScale, 1f);
                dot.SetActive(false);
                guideDots.Add(renderer);
            }
        }

        private void AnimateMarker()
        {
            if (markerRenderer == null)
            {
                return;
            }

            float pulse = 0.9f + Mathf.Sin(Time.time * 3f) * 0.08f;
            markerRenderer.transform.localScale = new Vector3(markerScale * pulse, markerScale * pulse, 1f);
        }

        private void UpdateGuideDots()
        {
            if (playerTransform == null || guideDots.Count == 0)
            {
                SetGuideVisible(false);
                return;
            }

            Vector3 start = playerTransform.position;
            Vector3 end = transform.position;
            Vector3 delta = end - start;
            float distance = delta.magnitude;
            if (distance <= 0.01f)
            {
                SetGuideVisible(false);
                return;
            }

            Vector3 direction = delta / distance;
            int activeDots = Mathf.Min(guideDots.Count, Mathf.Max(0, Mathf.FloorToInt(distance / guideDotSpacing) - 1));

            for (int i = 0; i < guideDots.Count; i++)
            {
                bool isActive = i < activeDots;
                guideDots[i].gameObject.SetActive(isActive);
                if (!isActive)
                {
                    continue;
                }

                float step = guideDotSpacing * (i + 1);
                guideDots[i].transform.position = start + direction * step;
            }
        }

        private void SetGuideVisible(bool isVisible)
        {
            for (int i = 0; i < guideDots.Count; i++)
            {
                if (guideDots[i] != null)
                {
                    guideDots[i].gameObject.SetActive(isVisible);
                }
            }
        }

        private static Sprite CreateRuntimeSprite()
        {
            return Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), new Vector2(0.5f, 0.5f), 32f);
        }

        private static string ResolveNextFloorSceneName()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (!currentSceneName.StartsWith("Floor_", System.StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            string suffix = currentSceneName.Substring("Floor_".Length);
            if (!int.TryParse(suffix, out int currentFloor))
            {
                return string.Empty;
            }

            string nextSceneName = $"Floor_{currentFloor + 1:00}";
            return Application.CanStreamedLevelBeLoaded(nextSceneName) ? nextSceneName : string.Empty;
        }
    }
}
