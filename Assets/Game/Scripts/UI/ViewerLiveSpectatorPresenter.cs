using System.Collections.Generic;
using SoulForge.Viewer;
using UnityEngine;
using UnityEngine.UI;

namespace SoulForge.UI
{
    public sealed class ViewerLiveSpectatorPresenter : MonoBehaviour
    {
        private const string ViewportName = "Viewport";
        private const string MarkerRootName = "MarkerRoot";

        [SerializeField] private StateBroadcaster stateBroadcaster;
        [SerializeField] private RectTransform viewportRect;
        [SerializeField] private RectTransform markerRoot;

        private readonly Dictionary<string, Image> markers = new();
        private static Sprite runtimeSprite;
        private ViewerViewportMarkerState lastPlayerMarker;
        private ViewerViewportMarkerState lastTargetMarker;
        private ViewerViewportMarkerState[] lastEnemyMarkers;

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            EnsureReferences();
            if (stateBroadcaster != null)
            {
                stateBroadcaster.SnapshotBroadcast += OnSnapshot;
                stateBroadcaster.DeltaBroadcast += OnDelta;
            }
        }

        private void OnDisable()
        {
            if (stateBroadcaster != null)
            {
                stateBroadcaster.SnapshotBroadcast -= OnSnapshot;
                stateBroadcaster.DeltaBroadcast -= OnDelta;
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            EnsureReferences();
            RenderMarkers(lastPlayerMarker, lastTargetMarker, lastEnemyMarkers);
        }

        private void OnSnapshot(ViewerSessionSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            lastPlayerMarker = snapshot.PlayerMarker;
            lastTargetMarker = snapshot.TargetMarker;
            lastEnemyMarkers = snapshot.EnemyMarkers;
            RenderMarkers(snapshot.PlayerMarker, snapshot.TargetMarker, snapshot.EnemyMarkers);
        }

        private void OnDelta(ViewerSessionDelta delta)
        {
            if (delta == null)
            {
                return;
            }

            lastPlayerMarker = delta.PlayerMarker;
            lastTargetMarker = delta.TargetMarker;
            lastEnemyMarkers = delta.EnemyMarkers;
            RenderMarkers(delta.PlayerMarker, delta.TargetMarker, delta.EnemyMarkers);
        }

        private void RenderMarkers(ViewerViewportMarkerState player, ViewerViewportMarkerState target, ViewerViewportMarkerState[] enemies)
        {
            HashSet<string> activeIds = new();
            RenderMarker(player, new Color(0.3f, 0.9f, 1f, 1f), 18f, activeIds);
            RenderMarker(target, new Color(1f, 0.85f, 0.3f, 1f), 22f, activeIds);

            if (enemies != null)
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    RenderMarker(enemies[i], new Color(1f, 0.35f, 0.35f, 0.95f), 14f, activeIds);
                }
            }

            foreach (KeyValuePair<string, Image> pair in markers)
            {
                if (pair.Value != null)
                {
                    pair.Value.gameObject.SetActive(activeIds.Contains(pair.Key));
                }
            }
        }

        private void RenderMarker(ViewerViewportMarkerState marker, Color color, float size, ISet<string> activeIds)
        {
            if (viewportRect == null || markerRoot == null || marker == null || !marker.Visible || string.IsNullOrWhiteSpace(marker.Id))
            {
                return;
            }

            activeIds.Add(marker.Id);

            if (!markers.TryGetValue(marker.Id, out Image image) || image == null)
            {
                image = CreateMarker(marker.Id, color, size);
                markers[marker.Id] = image;
            }

            image.color = color;
            RectTransform rect = image.rectTransform;
            rect.sizeDelta = new Vector2(size, size);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);

            Rect container = markerRoot.rect;
            if (container.width <= 0f || container.height <= 0f)
            {
                container = viewportRect.rect;
            }

            float x = (marker.ViewportX - 0.5f) * container.width;
            float y = (marker.ViewportY - 0.5f) * container.height;
            rect.anchoredPosition = new Vector2(x, y);
            image.gameObject.SetActive(true);
        }

        private Image CreateMarker(string id, Color color, float size)
        {
            GameObject markerObject = new(id);
            markerObject.transform.SetParent(markerRoot, false);
            RectTransform rect = markerObject.AddComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);

            Image image = markerObject.AddComponent<Image>();
            image.sprite = GetRuntimeSprite();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void EnsureReferences()
        {
            if (stateBroadcaster == null)
            {
                stateBroadcaster = FindFirstObjectByType<StateBroadcaster>();
            }

            RectTransform namedViewport = FindRectTransformByName(ViewportName);
            if (namedViewport != null && namedViewport != viewportRect)
            {
                viewportRect = namedViewport;
            }
            else if (viewportRect == null || viewportRect.rect.width <= 0f || viewportRect.rect.height <= 0f)
            {
                viewportRect = ResolveViewportRect();
            }

            RectTransform resolvedMarkerRoot = ResolveMarkerRoot(viewportRect);
            if (resolvedMarkerRoot != null && resolvedMarkerRoot != markerRoot)
            {
                markerRoot = resolvedMarkerRoot;
            }
            else if (markerRoot == null || markerRoot.parent != viewportRect)
            {
                markerRoot = resolvedMarkerRoot;
            }
        }

        private RectTransform ResolveViewportRect()
        {
            RectTransform selfRect = transform as RectTransform;
            if (selfRect != null && selfRect.rect.width > 0f && selfRect.rect.height > 0f)
            {
                return selfRect;
            }

            RectTransform namedViewport = FindRectTransformByName(ViewportName);
            return namedViewport != null ? namedViewport : selfRect;
        }

        private static RectTransform ResolveMarkerRoot(RectTransform viewport)
        {
            if (viewport == null)
            {
                return null;
            }

            for (int i = 0; i < viewport.childCount; i++)
            {
                if (viewport.GetChild(i) is RectTransform child && child.name == MarkerRootName)
                {
                    return child;
                }
            }

            return viewport;
        }

        private RectTransform FindRectTransformByName(string objectName)
        {
            RectTransform[] rects = FindObjectsByType<RectTransform>(FindObjectsSortMode.None);
            for (int i = 0; i < rects.Length; i++)
            {
                RectTransform rect = rects[i];
                if (rect != null && rect.name == objectName && rect.gameObject.scene.IsValid())
                {
                    return rect;
                }
            }

            return null;
        }

        private static Sprite GetRuntimeSprite()
        {
            if (runtimeSprite == null)
            {
                runtimeSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), new Vector2(0.5f, 0.5f), 32f);
            }

            return runtimeSprite;
        }
    }
}
