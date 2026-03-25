#if UNITY_EDITOR
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
            SoulForgeViewerLiveSceneBuilder.CreateViewerLiveScene();

            const string scenePath = "Assets/Game/Scenes/Viewer_Client.unity";
            Scene scene;
            if (System.IO.File.Exists(scenePath))
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                scene.name = "Viewer_Client";

                CreateCamera();
                CreateEventSystem();
                Transform canvasRoot = CreateCanvas();
                CreateViewerRuntime();
                CreateHeader(canvasRoot);
                CreateConnectionPanel(canvasRoot);
            }

            EnsureViewerClientCoreObjects();
            EditorSceneManager.SaveScene(scene, scenePath);
            AddSceneToBuildSettings(scenePath);
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath));
            Debug.Log("Viewer client scene synced from current layout.");
        }

        private static void EnsureViewerClientCoreObjects()
        {
            if (Object.FindFirstObjectByType<Camera>() == null)
            {
                CreateCamera();
            }

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                CreateEventSystem();
            }

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                CreateCanvas();
            }

            ViewerWebSocketClient client = Object.FindFirstObjectByType<ViewerWebSocketClient>();
            if (client == null)
            {
                CreateViewerRuntime();
                return;
            }

            GameObject runtime = client.gameObject;
            if (runtime.GetComponent<StateBroadcaster>() == null)
            {
                runtime.AddComponent<StateBroadcaster>();
            }

            SerializedObject clientSo = new(client);
            clientSo.FindProperty("autoConnect").boolValue = false;
            clientSo.ApplyModifiedPropertiesWithoutUndo();
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

        private static void CreateViewerRuntime()
        {
            GameObject runtime = new("ViewerRuntime");
            runtime.AddComponent<StateBroadcaster>();
            ViewerWebSocketClient client = runtime.AddComponent<ViewerWebSocketClient>();
            SerializedObject so = new(client);
            so.FindProperty("autoConnect").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateHeader(Transform canvasRoot)
        {
            CreateText("Title", canvasRoot, new Vector2(0f, -120f), new Vector2(900f, 44f), "Viewer Connect", 40, TextAlignmentOptions.Center, FontStyles.Bold);
            CreateText("Hint", canvasRoot, new Vector2(0f, -172f), new Vector2(900f, 32f), "Enter host IP and session code, then connect to the live viewer.", 18, TextAlignmentOptions.Center, FontStyles.Normal);
        }

        private static void CreateConnectionPanel(Transform canvasRoot)
        {
            GameObject panel = CreatePanel("ConnectionPanel", canvasRoot, new Vector2(0f, -290f), new Vector2(880f, 220f));
            ViewerWebSocketClient client = Object.FindFirstObjectByType<ViewerWebSocketClient>();
            ViewerConnectionPresenter presenter = panel.AddComponent<ViewerConnectionPresenter>();

            CreateText("ConnectionLabel", panel.transform, new Vector2(-280f, -34f), new Vector2(140f, 32f), "Host IP", 20, TextAlignmentOptions.Left, FontStyles.Bold);
            TMP_InputField hostInput = CreateInputField("HostInput", panel.transform, new Vector2(-150f, -28f), new Vector2(220f, 44f), client != null ? client.HostIp : "127.0.0.1");
            CreateText("SessionLabel", panel.transform, new Vector2(70f, -34f), new Vector2(170f, 32f), "Session Code", 20, TextAlignmentOptions.Left, FontStyles.Bold);
            TMP_InputField sessionInput = CreateInputField("SessionInput", panel.transform, new Vector2(220f, -28f), new Vector2(170f, 44f), client != null ? client.SessionCode : "LOCAL1");
            Button connectButton = CreateButton("ConnectButton", panel.transform, new Vector2(0f, -112f), new Vector2(220f, 52f), "Connect", 22);
            TMP_Text statusText = CreateText("ConnectionStatus", panel.transform, new Vector2(0f, -170f), new Vector2(760f, 28f), "Offline", 18, TextAlignmentOptions.Center, FontStyles.Normal).GetComponent<TMP_Text>();

            SerializedObject so = new(presenter);
            so.FindProperty("viewerWebSocketClient").objectReferenceValue = client;
            so.FindProperty("hostIpInput").objectReferenceValue = hostInput;
            so.FindProperty("sessionCodeInput").objectReferenceValue = sessionInput;
            so.FindProperty("connectButton").objectReferenceValue = connectButton;
            so.FindProperty("statusText").objectReferenceValue = statusText;
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
            tmp.raycastTarget = false;
            return textObject;
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
