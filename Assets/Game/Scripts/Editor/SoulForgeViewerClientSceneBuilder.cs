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
            CreateConnectionPanel(canvasRoot);
            CreateStateFeed(canvasRoot, broadcaster);
            CreateHistory(canvasRoot, broadcaster);
            CreateStore(canvasRoot, broadcaster, catalog);

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
            ViewerWebSocketClient client = runtime.AddComponent<ViewerWebSocketClient>();
            SerializedObject so = new(client);
            so.FindProperty("autoConnect").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
            return broadcaster;
        }

        private static void CreateHeader(Transform canvasRoot)
        {
            CreateText("Title", canvasRoot, new Vector2(0f, -24f), new Vector2(900f, 44f), "Viewer Client", 34, TextAlignmentOptions.Center, FontStyles.Bold);
            CreateText("Hint", canvasRoot, new Vector2(0f, -68f), new Vector2(900f, 32f), "Connect to host IP, then buy actions while watching the run.", 18, TextAlignmentOptions.Center, FontStyles.Normal);
        }

        private static void CreateConnectionPanel(Transform canvasRoot)
        {
            GameObject panel = CreatePanel("ConnectionPanel", canvasRoot, new Vector2(0f, -118f), new Vector2(1180f, 220f));
            ViewerWebSocketClient client = Object.FindFirstObjectByType<ViewerWebSocketClient>();
            ViewerConnectionPresenter presenter = panel.AddComponent<ViewerConnectionPresenter>();

            CreateText("ConnectionLabel", panel.transform, new Vector2(-465f, -26f), new Vector2(140f, 32f), "Host IP", 20, TextAlignmentOptions.Left, FontStyles.Bold);
            TMP_InputField hostInput = CreateInputField("HostInput", panel.transform, new Vector2(-320f, -20f), new Vector2(220f, 44f), client != null ? client.HostIp : "127.0.0.1");
            CreateText("SessionLabel", panel.transform, new Vector2(-55f, -26f), new Vector2(170f, 32f), "Session Code", 20, TextAlignmentOptions.Left, FontStyles.Bold);
            TMP_InputField sessionInput = CreateInputField("SessionInput", panel.transform, new Vector2(110f, -20f), new Vector2(170f, 44f), client != null ? client.SessionCode : "LOCAL1");
            Button connectButton = CreateButton("ConnectButton", panel.transform, new Vector2(305f, -20f), new Vector2(132f, 44f), "Connect", 20);
            Button disconnectButton = CreateButton("DisconnectButton", panel.transform, new Vector2(452f, -20f), new Vector2(146f, 44f), "Disconnect", 20);
            Button reconnectButton = CreateButton("ReconnectButton", panel.transform, new Vector2(610f, -20f), new Vector2(146f, 44f), "Reconnect", 20);
            TMP_Text viewerIdText = CreateText("ViewerIdText", panel.transform, new Vector2(-330f, -78f), new Vector2(280f, 28f), "Viewer ID", 18, TextAlignmentOptions.Left, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text statusText = CreateText("ConnectionStatus", panel.transform, new Vector2(110f, -78f), new Vector2(630f, 28f), "Offline", 18, TextAlignmentOptions.Left, FontStyles.Normal).GetComponent<TMP_Text>();
            TMP_Text recentSessionsText = CreateText("RecentSessionsText", panel.transform, new Vector2(0f, -120f), new Vector2(1040f, 72f), "Recent Sessions\nNone yet", 16, TextAlignmentOptions.TopLeft, FontStyles.Normal).GetComponent<TMP_Text>();

            SerializedObject so = new(presenter);
            so.FindProperty("viewerWebSocketClient").objectReferenceValue = client;
            so.FindProperty("hostIpInput").objectReferenceValue = hostInput;
            so.FindProperty("sessionCodeInput").objectReferenceValue = sessionInput;
            so.FindProperty("connectButton").objectReferenceValue = connectButton;
            so.FindProperty("disconnectButton").objectReferenceValue = disconnectButton;
            so.FindProperty("reconnectButton").objectReferenceValue = reconnectButton;
            so.FindProperty("statusText").objectReferenceValue = statusText;
            so.FindProperty("viewerIdText").objectReferenceValue = viewerIdText;
            so.FindProperty("recentSessionsText").objectReferenceValue = recentSessionsText;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateStateFeed(Transform canvasRoot, StateBroadcaster broadcaster)
        {
            GameObject panel = CreatePanel("StatePanel", canvasRoot, new Vector2(-420f, -278f), new Vector2(540f, 540f));

            TMP_Text summary = CreateText("SummaryText", panel.transform, new Vector2(0f, -18f), new Vector2(480f, 126f), "Summary", 22, TextAlignmentOptions.TopLeft, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text connection = CreateText("ConnectionText", panel.transform, new Vector2(0f, -142f), new Vector2(480f, 34f), "Connection", 18, TextAlignmentOptions.Left, FontStyles.Normal).GetComponent<TMP_Text>();
            TMP_Text snapshot = CreateText("SnapshotText", panel.transform, new Vector2(0f, -188f), new Vector2(480f, 124f), "Snapshot", 18, TextAlignmentOptions.TopLeft, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text delta = CreateText("DeltaText", panel.transform, new Vector2(0f, -326f), new Vector2(480f, 100f), "Delta", 18, TextAlignmentOptions.TopLeft, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text result = CreateText("ActionResultText", panel.transform, new Vector2(0f, -438f), new Vector2(480f, 76f), "Result", 18, TextAlignmentOptions.TopLeft, FontStyles.Bold).GetComponent<TMP_Text>();

            ViewerStateFeed feed = panel.AddComponent<ViewerStateFeed>();
            SerializedObject so = new(feed);
            so.FindProperty("stateBroadcaster").objectReferenceValue = broadcaster;
            so.FindProperty("viewerWebSocketClient").objectReferenceValue = Object.FindFirstObjectByType<ViewerWebSocketClient>();
            so.FindProperty("summaryText").objectReferenceValue = summary;
            so.FindProperty("connectionText").objectReferenceValue = connection;
            so.FindProperty("snapshotText").objectReferenceValue = snapshot;
            so.FindProperty("deltaText").objectReferenceValue = delta;
            so.FindProperty("actionResultText").objectReferenceValue = result;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateHistory(Transform canvasRoot, StateBroadcaster broadcaster)
        {
            GameObject panel = CreatePanel("HistoryPanel", canvasRoot, new Vector2(-420f, -844f), new Vector2(540f, 188f));
            TMP_Text history = CreateText("HistoryText", panel.transform, new Vector2(0f, -24f), new Vector2(480f, 126f), "History", 18, TextAlignmentOptions.TopLeft, FontStyles.Bold).GetComponent<TMP_Text>();

            ViewerActionHistoryPresenter presenter = panel.AddComponent<ViewerActionHistoryPresenter>();
            SerializedObject so = new(presenter);
            so.FindProperty("stateBroadcaster").objectReferenceValue = broadcaster;
            so.FindProperty("remoteViewerClient").objectReferenceValue = Object.FindFirstObjectByType<ViewerWebSocketClient>();
            so.FindProperty("historyText").objectReferenceValue = history;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateStore(Transform canvasRoot, StateBroadcaster broadcaster, ViewerStoreCatalog catalog)
        {
            GameObject panel = CreatePanel("StorePanel", canvasRoot, new Vector2(420f, -244f), new Vector2(640f, 660f));
            TMP_Text header = CreateText("StoreHeader", panel.transform, new Vector2(0f, -20f), new Vector2(520f, 32f), "Viewer Store", 28, TextAlignmentOptions.Center, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text balance = CreateText("BalanceText", panel.transform, new Vector2(-140f, -64f), new Vector2(220f, 26f), "Crowns: 0", 18, TextAlignmentOptions.Left, FontStyles.Bold).GetComponent<TMP_Text>();
            TMP_Text budget = CreateText("BudgetText", panel.transform, new Vector2(140f, -64f), new Vector2(220f, 26f), "Budget: 0", 18, TextAlignmentOptions.Right, FontStyles.Bold).GetComponent<TMP_Text>();

            ViewerStorePresenter presenter = panel.AddComponent<ViewerStorePresenter>();
            ViewerEconomyPresenter economyPresenter = panel.AddComponent<ViewerEconomyPresenter>();
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
                buttonSo.FindProperty("descriptionText").objectReferenceValue = buttonObject.transform.Find("Description")?.GetComponent<TMP_Text>();
                buttonSo.FindProperty("stateText").objectReferenceValue = buttonObject.transform.Find("State")?.GetComponent<TMP_Text>();
                buttonSo.FindProperty("background").objectReferenceValue = buttonObject.GetComponent<Image>();
                buttonSo.ApplyModifiedPropertiesWithoutUndo();

                presenterSo.FindProperty("actionButtons").GetArrayElementAtIndex(i).objectReferenceValue = storeButton;
            }

            presenterSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject economySo = new(economyPresenter);
            economySo.FindProperty("stateBroadcaster").objectReferenceValue = broadcaster;
            economySo.FindProperty("balanceText").objectReferenceValue = balance;
            economySo.FindProperty("budgetText").objectReferenceValue = budget;
            economySo.FindProperty("remoteViewerClient").objectReferenceValue = Object.FindFirstObjectByType<ViewerWebSocketClient>();
            economySo.ApplyModifiedPropertiesWithoutUndo();
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
            rect.anchoredPosition = new Vector2(0f, -122f - index * 124f);
            rect.sizeDelta = new Vector2(440f, 100f);

            Image image = buttonObject.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = new Color(0.13f, 0.29f, 0.41f, 1f);
            buttonObject.AddComponent<Button>();

            CreateText("Price", buttonObject.transform, new Vector2(0f, -10f), new Vector2(380f, 20f), "0", 14, TextAlignmentOptions.Center, FontStyles.Normal);
            CreateText("Label", buttonObject.transform, new Vector2(0f, -30f), new Vector2(380f, 24f), "Action", 22, TextAlignmentOptions.Center, FontStyles.Bold);
            CreateText("Description", buttonObject.transform, new Vector2(0f, -52f), new Vector2(390f, 20f), "Description", 13, TextAlignmentOptions.Center, FontStyles.Normal);
            CreateText("State", buttonObject.transform, new Vector2(0f, -76f), new Vector2(380f, 20f), "Send", 14, TextAlignmentOptions.Center, FontStyles.Normal);
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
            image.color = new Color(0.13f, 0.29f, 0.41f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            CreateText($"{name}_Label", buttonObject.transform, Vector2.zero, size - new Vector2(16f, 8f), label, fontSize, TextAlignmentOptions.Center, FontStyles.Bold);
            return button;
        }

        private static TMP_InputField CreateInputField(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, string text)
        {
            GameObject inputObject = new(name);
            inputObject.transform.SetParent(parent, false);
            RectTransform rect = inputObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = inputObject.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = new Color(0.14f, 0.18f, 0.23f, 1f);

            TMP_InputField input = inputObject.AddComponent<TMP_InputField>();

            TMP_Text textComponent = CreateText("Text", inputObject.transform, new Vector2(-10f, -6f), new Vector2(size.x - 28f, size.y - 12f), text, 20, TextAlignmentOptions.Left, FontStyles.Normal).GetComponent<TMP_Text>();
            textComponent.textWrappingMode = TextWrappingModes.NoWrap;
            textComponent.color = Color.white;
            textComponent.rectTransform.anchorMin = Vector2.zero;
            textComponent.rectTransform.anchorMax = Vector2.one;
            textComponent.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            TMP_Text placeholder = CreateText("Placeholder", inputObject.transform, new Vector2(-10f, -6f), new Vector2(size.x - 28f, size.y - 12f), "192.168.1.3", 20, TextAlignmentOptions.Left, FontStyles.Italic).GetComponent<TMP_Text>();
            placeholder.color = new Color(1f, 1f, 1f, 0.35f);
            placeholder.rectTransform.anchorMin = Vector2.zero;
            placeholder.rectTransform.anchorMax = Vector2.one;
            placeholder.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            input.textViewport = rect;
            input.textComponent = (TextMeshProUGUI)textComponent;
            input.placeholder = placeholder;
            input.text = text;
            return input;
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
