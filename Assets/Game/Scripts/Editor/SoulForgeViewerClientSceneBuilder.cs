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
    public static class SoulForgeViewerClientSceneBuilder
    {
        [MenuItem("Tools/SoulForge/Create Viewer Client Scene")]
        public static void CreateViewerClientScene()
        {
            SoulForgePrototypeAssetCreator.CreatePrototypeAssets();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Viewer_Client";

            CreateCamera();
            CreateEventSystem();
            Transform canvasRoot = CreateCanvas();
            ViewerStoreCatalog catalog = AssetDatabase.LoadAssetAtPath<ViewerStoreCatalog>("Assets/Game/ScriptableObjects/Viewer/ViewerStoreCatalog.asset");

            StateBroadcaster broadcaster = CreateViewerRuntime();
            CreateHeader(canvasRoot);
            CreateStateFeed(canvasRoot, broadcaster);
            CreateHistory(canvasRoot, broadcaster);
            CreateStore(canvasRoot, catalog);

            const string scenePath = "Assets/Game/Scenes/Viewer_Client.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AddSceneToBuildSettings(scenePath);
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath));
            Debug.Log("Viewer client scene created.");
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.11f, 1f);
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

        private static StateBroadcaster CreateViewerRuntime()
        {
            GameObject runtime = new("ViewerRuntime");
            StateBroadcaster broadcaster = runtime.AddComponent<StateBroadcaster>();
            runtime.AddComponent<ViewerWebSocketClient>();
            return broadcaster;
        }

        private static void CreateHeader(Transform canvasRoot)
        {
            CreateText("Title", canvasRoot, new Vector2(0f, -24f), new Vector2(900f, 44f), "Viewer Client", 34, TextAlignmentOptions.Center, FontStyles.Bold);
            CreateText("Hint", canvasRoot, new Vector2(0f, -68f), new Vector2(900f, 32f), "Set Host IP on ViewerWebSocketClient, then press Play.", 18, TextAlignmentOptions.Center, FontStyles.Normal);
        }

        private static void CreateStateFeed(Transform canvasRoot, StateBroadcaster broadcaster)
        {
            GameObject panel = CreatePanel("StatePanel", canvasRoot, new Vector2(-400f, -160f), new Vector2(520f, 420f));

            TMP_Text snapshot = CreateText("SnapshotText", panel.transform, new Vector2(0f, -18f), new Vector2(460f, 120f), "Snapshot", 18, TextAlignmentOptions.TopLeft, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text delta = CreateText("DeltaText", panel.transform, new Vector2(0f, -156f), new Vector2(460f, 100f), "Delta", 18, TextAlignmentOptions.TopLeft, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text result = CreateText("ActionResultText", panel.transform, new Vector2(0f, -272f), new Vector2(460f, 82f), "Result", 18, TextAlignmentOptions.TopLeft, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text json = CreateText("RawJsonText", panel.transform, new Vector2(0f, -360f), new Vector2(460f, 52f), "Raw", 13, TextAlignmentOptions.TopLeft, FontStyles.Normal).GetComponent<TMP_Text>();

            ViewerStateFeed feed = panel.AddComponent<ViewerStateFeed>();
            SerializedObject so = new(feed);
            so.FindProperty("stateBroadcaster").objectReferenceValue = broadcaster;
            so.FindProperty("snapshotText").objectReferenceValue = snapshot;
            so.FindProperty("deltaText").objectReferenceValue = delta;
            so.FindProperty("actionResultText").objectReferenceValue = result;
            so.FindProperty("rawJsonText").objectReferenceValue = json;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateHistory(Transform canvasRoot, StateBroadcaster broadcaster)
        {
            GameObject panel = CreatePanel("HistoryPanel", canvasRoot, new Vector2(-400f, -608f), new Vector2(520f, 170f));
            TMP_Text history = CreateText("HistoryText", panel.transform, new Vector2(0f, -24f), new Vector2(460f, 110f), "History", 18, TextAlignmentOptions.TopLeft, FontStyles.Bold).GetComponent<TMP_Text>();

            ViewerActionHistoryPresenter presenter = panel.AddComponent<ViewerActionHistoryPresenter>();
            SerializedObject so = new(presenter);
            so.FindProperty("stateBroadcaster").objectReferenceValue = broadcaster;
            so.FindProperty("historyText").objectReferenceValue = history;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateStore(Transform canvasRoot, ViewerStoreCatalog catalog)
        {
            GameObject panel = CreatePanel("StorePanel", canvasRoot, new Vector2(420f, -220f), new Vector2(620f, 520f));
            TMP_Text header = CreateText("StoreHeader", panel.transform, new Vector2(0f, -20f), new Vector2(520f, 32f), "Viewer Store", 28, TextAlignmentOptions.Center, FontStyles.Bold).GetComponent<TMP_Text>();

            ViewerStorePresenter presenter = panel.AddComponent<ViewerStorePresenter>();
            SerializedObject presenterSo = new(presenter);
            presenterSo.FindProperty("storeCatalog").objectReferenceValue = catalog;
            presenterSo.FindProperty("headerText").objectReferenceValue = header;
            presenterSo.FindProperty("actionButtons").arraySize = 4;

            for (int i = 0; i < 4; i++)
            {
                GameObject buttonObject = CreateStoreButton(panel.transform, i);
                ViewerStoreButton storeButton = buttonObject.AddComponent<ViewerStoreButton>();

                SerializedObject buttonSo = new(storeButton);
                buttonSo.FindProperty("labelText").objectReferenceValue = buttonObject.transform.Find("Label")?.GetComponent<TMP_Text>();
                buttonSo.FindProperty("priceText").objectReferenceValue = buttonObject.transform.Find("Price")?.GetComponent<TMP_Text>();
                buttonSo.FindProperty("stateText").objectReferenceValue = buttonObject.transform.Find("State")?.GetComponent<TMP_Text>();
                buttonSo.FindProperty("background").objectReferenceValue = buttonObject.GetComponent<Image>();
                buttonSo.ApplyModifiedPropertiesWithoutUndo();

                presenterSo.FindProperty("actionButtons").GetArrayElementAtIndex(i).objectReferenceValue = storeButton;
            }

            presenterSo.ApplyModifiedPropertiesWithoutUndo();
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
            image.color = new Color(0.09f, 0.12f, 0.17f, 0.92f);
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
            rect.anchoredPosition = new Vector2(0f, -88f - index * 102f);
            rect.sizeDelta = new Vector2(420f, 84f);

            Image image = buttonObject.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = new Color(0.13f, 0.29f, 0.41f, 1f);
            buttonObject.AddComponent<Button>();

            CreateText("Price", buttonObject.transform, new Vector2(0f, -8f), new Vector2(360f, 20f), "0", 14, TextAlignmentOptions.Center, FontStyles.Normal);
            CreateText("Label", buttonObject.transform, new Vector2(0f, -30f), new Vector2(360f, 24f), "Action", 22, TextAlignmentOptions.Center, FontStyles.Bold);
            CreateText("State", buttonObject.transform, new Vector2(0f, -56f), new Vector2(360f, 20f), "Send", 14, TextAlignmentOptions.Center, FontStyles.Normal);
            return buttonObject;
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
