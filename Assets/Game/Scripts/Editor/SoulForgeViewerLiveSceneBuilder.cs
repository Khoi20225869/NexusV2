#if UNITY_EDITOR
using SoulForge.Data;
using SoulForge.UI;
using SoulForge.Viewer;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SoulForge.Editor
{
    public static class SoulForgeViewerLiveSceneBuilder
    {
        [MenuItem("Tools/SoulForge/Create Viewer Live Scene")]
        public static void CreateViewerLiveScene()
        {
            const string scenePath = "Assets/Game/Scenes/Viewer_Live.unity";
            Scene scene;
            if (System.IO.File.Exists(scenePath))
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                scene.name = "Viewer_Live";

                CreateCamera();
                CreateEventSystem();
                Transform canvasRoot = CreateCanvas();

                CreateHeader(canvasRoot);
                CreateLiveViewport(canvasRoot, out ViewerTargetingController targetingController);
                CreateStore(canvasRoot, targetingController);
                CreateHistory(canvasRoot);
            }

            EnsureViewerLiveCoreObjects();
            EditorSceneManager.SaveScene(scene, scenePath);
            AddSceneToBuildSettings(scenePath);
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath));
            Debug.Log("Viewer live scene synced from current layout.");
        }

        private static void EnsureViewerLiveCoreObjects()
        {
            if (Object.FindFirstObjectByType<Camera>() == null)
            {
                CreateCamera();
            }

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                CreateEventSystem();
            }

            if (Object.FindFirstObjectByType<Canvas>() == null)
            {
                CreateCanvas();
            }

            if (Object.FindFirstObjectByType<ViewerLiveHudPresenter>() == null ||
                Object.FindFirstObjectByType<ViewerLiveSpectatorPresenter>() == null ||
                Object.FindFirstObjectByType<ViewerTargetingController>() == null ||
                Object.FindFirstObjectByType<ViewerTargetSurface>() == null)
            {
                Debug.LogWarning("Viewer_Live is missing one or more live-view components. Current layout was preserved; add missing components manually or rebuild the scene if needed.");
            }
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(0.05f, 0.07f, 0.1f, 1f);
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static Transform CreateCanvas()
        {
            GameObject canvasObject = new("UIRoot");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvasObject.transform;
        }

        private static void CreateHeader(Transform canvasRoot)
        {
            GameObject panel = CreatePanel("TopBar", canvasRoot, new Vector2(0f, -18f), new Vector2(1820f, 92f));

            TMP_Text connectionText = CreateText("ConnectionText", panel.transform, new Vector2(-730f, -20f), new Vector2(360f, 30f), "Status: Offline", 20, TextAlignmentOptions.Left, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text floorText = CreateText("FloorText", panel.transform, new Vector2(-200f, -20f), new Vector2(200f, 30f), "Floor 1", 22, TextAlignmentOptions.Center, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text phaseText = CreateText("PhaseText", panel.transform, new Vector2(40f, -20f), new Vector2(240f, 30f), "Phase: explore", 20, TextAlignmentOptions.Center, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text hpText = CreateText("HpText", panel.transform, new Vector2(360f, -20f), new Vector2(240f, 30f), "Host HP: 0", 20, TextAlignmentOptions.Center, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text hintText = CreateText("HintText", panel.transform, new Vector2(0f, -56f), new Vector2(1240f, 24f), "Connect from Viewer_Client, then use the live view to target actions.", 16, TextAlignmentOptions.Center, FontStyles.Normal).GetComponent<TMP_Text>();

            ViewerLiveHudPresenter presenter = panel.AddComponent<ViewerLiveHudPresenter>();
            SerializedObject so = new(presenter);
            so.FindProperty("connectionText").objectReferenceValue = connectionText;
            so.FindProperty("floorText").objectReferenceValue = floorText;
            so.FindProperty("phaseText").objectReferenceValue = phaseText;
            so.FindProperty("hpText").objectReferenceValue = hpText;
            so.ApplyModifiedPropertiesWithoutUndo();

            _ = hintText;
        }

        private static void CreateLiveViewport(Transform canvasRoot, out ViewerTargetingController targetingController)
        {
            GameObject panel = CreatePanel("LivePanel", canvasRoot, new Vector2(-280f, -132f), new Vector2(1080f, 820f));
            RectTransform panelRect = panel.GetComponent<RectTransform>();

            CreateText("LiveTitle", panel.transform, new Vector2(0f, -18f), new Vector2(640f, 28f), "Host Live View", 28, TextAlignmentOptions.Center, FontStyles.Bold);

            GameObject viewport = new("Viewport");
            viewport.transform.SetParent(panel.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = new Vector2(0.5f, 1f);
            viewportRect.anchorMax = new Vector2(0.5f, 1f);
            viewportRect.pivot = new Vector2(0.5f, 1f);
            viewportRect.anchoredPosition = new Vector2(0f, -72f);
            viewportRect.sizeDelta = new Vector2(980f, 640f);

            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            viewportImage.type = Image.Type.Sliced;
            viewportImage.color = new Color(0.1f, 0.14f, 0.2f, 0.98f);

            GameObject grid = new("GridOverlay");
            grid.transform.SetParent(viewport.transform, false);
            RectTransform gridRect = grid.AddComponent<RectTransform>();
            gridRect.anchorMin = Vector2.zero;
            gridRect.anchorMax = Vector2.one;
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;

            Image gridImage = grid.AddComponent<Image>();
            gridImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            gridImage.color = new Color(1f, 1f, 1f, 0.08f);
            gridImage.raycastTarget = false;

            GameObject markerRoot = new("MarkerRoot");
            markerRoot.transform.SetParent(viewport.transform, false);
            RectTransform markerRootRect = markerRoot.AddComponent<RectTransform>();
            markerRootRect.anchorMin = Vector2.zero;
            markerRootRect.anchorMax = Vector2.one;
            markerRootRect.offsetMin = Vector2.zero;
            markerRootRect.offsetMax = Vector2.zero;

            GameObject overlay = new("TargetOverlay");
            overlay.transform.SetParent(viewport.transform, false);
            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            Image overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0.35f, 0.65f, 1f, 0.12f);
            overlayImage.enabled = false;
            overlayImage.raycastTarget = false;

            TMP_Text targetHint = CreateText("TargetHint", panel.transform, new Vector2(0f, -736f), new Vector2(920f, 26f), "Choose an action, then click the live view to place it.", 18, TextAlignmentOptions.Center, FontStyles.Normal).GetComponent<TMP_Text>();
            TMP_Text clickFeedback = CreateText("ClickFeedback", panel.transform, new Vector2(0f, -770f), new Vector2(920f, 22f), string.Empty, 15, TextAlignmentOptions.Center, FontStyles.Normal).GetComponent<TMP_Text>();
            Button cancelButton = CreateButton("CancelTargetButton", panel.transform, new Vector2(400f, -728f), new Vector2(120f, 36f), "Cancel", 18);
            cancelButton.gameObject.SetActive(false);

            targetingController = panel.AddComponent<ViewerTargetingController>();
            SerializedObject targetingSo = new(targetingController);
            targetingSo.FindProperty("targetingHintText").objectReferenceValue = targetHint;
            targetingSo.FindProperty("targetingOverlay").objectReferenceValue = overlayImage;
            targetingSo.FindProperty("cancelButton").objectReferenceValue = cancelButton;
            targetingSo.ApplyModifiedPropertiesWithoutUndo();

            ViewerLiveSpectatorPresenter spectator = viewport.AddComponent<ViewerLiveSpectatorPresenter>();
            SerializedObject spectatorSo = new(spectator);
            spectatorSo.FindProperty("viewportRect").objectReferenceValue = viewportRect;
            spectatorSo.FindProperty("markerRoot").objectReferenceValue = markerRootRect;
            spectatorSo.ApplyModifiedPropertiesWithoutUndo();

            ViewerTargetSurface targetSurface = viewport.AddComponent<ViewerTargetSurface>();
            SerializedObject targetSurfaceSo = new(targetSurface);
            targetSurfaceSo.FindProperty("viewportRect").objectReferenceValue = viewportRect;
            targetSurfaceSo.FindProperty("targetingController").objectReferenceValue = targetingController;
            targetSurfaceSo.FindProperty("clickFeedbackText").objectReferenceValue = clickFeedback;
            targetSurfaceSo.ApplyModifiedPropertiesWithoutUndo();

            _ = panelRect;
        }

        private static void CreateStore(Transform canvasRoot, ViewerTargetingController targetingController)
        {
            ViewerStoreCatalog catalog = AssetDatabase.LoadAssetAtPath<ViewerStoreCatalog>("Assets/Game/ScriptableObjects/Viewer/ViewerStoreCatalog.asset");
            GameObject panel = CreatePanel("StorePanel", canvasRoot, new Vector2(620f, -132f), new Vector2(560f, 820f));
            TMP_Text header = CreateText("StoreHeader", panel.transform, new Vector2(0f, -18f), new Vector2(420f, 28f), "Viewer Actions", 28, TextAlignmentOptions.Center, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text balance = CreateText("BalanceText", panel.transform, new Vector2(-110f, -56f), new Vector2(180f, 24f), "Crowns: 0", 18, TextAlignmentOptions.Left, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text budget = CreateText("BudgetText", panel.transform, new Vector2(110f, -56f), new Vector2(180f, 24f), "Budget: 0", 18, TextAlignmentOptions.Right, FontStyles.Bold).GetComponent<TMP_Text>();

            ViewerStorePresenter presenter = panel.AddComponent<ViewerStorePresenter>();
            ViewerEconomyPresenter economyPresenter = panel.AddComponent<ViewerEconomyPresenter>();
            SerializedObject presenterSo = new(presenter);
            presenterSo.FindProperty("storeCatalog").objectReferenceValue = catalog;
            presenterSo.FindProperty("headerText").objectReferenceValue = header;
            presenterSo.FindProperty("targetingController").objectReferenceValue = targetingController;
            presenterSo.FindProperty("actionButtons").arraySize = 4;

            for (int i = 0; i < 4; i++)
            {
                GameObject buttonObject = CreateStoreButton(panel.transform, i);
                ViewerStoreButton storeButton = buttonObject.AddComponent<ViewerStoreButton>();
                SerializedObject buttonSo = new(storeButton);
                buttonSo.FindProperty("labelText").objectReferenceValue = buttonObject.transform.Find("Label")?.GetComponent<TMP_Text>();
                buttonSo.FindProperty("priceText").objectReferenceValue = buttonObject.transform.Find("Price")?.GetComponent<TMP_Text>();
                buttonSo.FindProperty("descriptionText").objectReferenceValue = buttonObject.transform.Find("Description")?.GetComponent<TMP_Text>();
                buttonSo.FindProperty("stateText").objectReferenceValue = buttonObject.transform.Find("State")?.GetComponent<TMP_Text>();
                buttonSo.FindProperty("background").objectReferenceValue = buttonObject.GetComponent<Image>();
                buttonSo.ApplyModifiedPropertiesWithoutUndo();
                presenterSo.FindProperty("actionButtons").GetArrayElementAtIndex(i).objectReferenceValue = storeButton;
            }

            presenterSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject economySo = new(economyPresenter);
            economySo.FindProperty("balanceText").objectReferenceValue = balance;
            economySo.FindProperty("budgetText").objectReferenceValue = budget;
            economySo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateHistory(Transform canvasRoot)
        {
            GameObject panel = CreatePanel("HistoryPanel", canvasRoot, new Vector2(620f, -976f), new Vector2(560f, 176f));
            TMP_Text history = CreateText("HistoryText", panel.transform, new Vector2(0f, -20f), new Vector2(500f, 122f), "History", 16, TextAlignmentOptions.TopLeft, FontStyles.Bold).GetComponent<TMP_Text>();

            ViewerActionHistoryPresenter presenter = panel.AddComponent<ViewerActionHistoryPresenter>();
            SerializedObject so = new(presenter);
            so.FindProperty("remoteViewerClient").objectReferenceValue = Object.FindFirstObjectByType<ViewerWebSocketClient>();
            so.FindProperty("historyText").objectReferenceValue = history;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject panel = new(name);
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = panel.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = new Color(0.09f, 0.12f, 0.17f, 0.94f);
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.12f);
            outline.effectDistance = new Vector2(3f, -3f);
            return panel;
        }

        private static GameObject CreateText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, string text, float fontSize, TextAlignmentOptions alignment, FontStyles style)
        {
            GameObject textObject = new(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            TMP_Text tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return textObject;
        }

        private static GameObject CreateStoreButton(Transform parent, int index)
        {
            GameObject buttonObject = new($"StoreButton_{index + 1}");
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -100f - index * 136f);
            rect.sizeDelta = new Vector2(420f, 112f);

            Image image = buttonObject.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = new Color(0.13f, 0.29f, 0.41f, 1f);
            buttonObject.AddComponent<Button>();

            CreateText("Price", buttonObject.transform, new Vector2(0f, -8f), new Vector2(360f, 18f), "0", 14, TextAlignmentOptions.Center, FontStyles.Normal);
            CreateText("Label", buttonObject.transform, new Vector2(0f, -30f), new Vector2(360f, 22f), "Action", 22, TextAlignmentOptions.Center, FontStyles.Bold);
            CreateText("Description", buttonObject.transform, new Vector2(0f, -54f), new Vector2(380f, 22f), "Description", 13, TextAlignmentOptions.Center, FontStyles.Normal);
            CreateText("State", buttonObject.transform, new Vector2(0f, -82f), new Vector2(360f, 18f), "Send", 14, TextAlignmentOptions.Center, FontStyles.Normal);
            return buttonObject;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, string label, float fontSize)
        {
            GameObject buttonObject = new(name);
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = buttonObject.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = new Color(0.2f, 0.3f, 0.42f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            CreateText($"{name}_Label", buttonObject.transform, Vector2.zero, size - new Vector2(12f, 6f), label, fontSize, TextAlignmentOptions.Center, FontStyles.Bold);
            return button;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(scene => scene.path == scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
#endif
