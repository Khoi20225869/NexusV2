#if UNITY_EDITOR
using SoulForge.Bootstrap;
using SoulForge.Data;
using SoulForge.Hub;
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
    public static class SoulForgeHubSceneBuilder
    {
        [MenuItem("Tools/SoulForge/Create Hub Scene")]
        public static void CreateHubScene()
        {
            SoulForgePrototypeAssetCreator.CreatePrototypeAssets();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Hub";

            CreateCamera();
            CreateEventSystem();
            CreateHostSession();
            Transform canvasRoot = CreateCanvas();
            HeroDefinition baseHero = AssetDatabase.LoadAssetAtPath<HeroDefinition>("Assets/Game/ScriptableObjects/Heroes/Hero_Default.asset");
            if (baseHero == null)
            {
                baseHero = AssetDatabase.LoadAssetAtPath<HeroDefinition>("Assets/Game/ScriptableObjects/Heroes/Hero_Knight.asset");
            }

            HubHeroPreviewController preview = CreateHeroPreview(baseHero);
            HubShopController shopController = CreateShopPanel(canvasRoot);
            CreateBlacksmithAltar(shopController);
            CreateCustomizerPanel(canvasRoot, baseHero, preview);

            string scenePath = "Assets/Game/Scenes/Hub.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AddSceneToBuildSettings(scenePath);
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath));
            Debug.Log("Hub baseline scene created.");
        }

        private static void CreateHostSession()
        {
            GameObject hostSession = new("HostSession");
            hostSession.AddComponent<ViewerSessionService>();
            hostSession.AddComponent<HostViewerWebSocketServer>();
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.5f;
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(0.07f, 0.08f, 0.12f, 1f);
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
            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.localScale = Vector3.one;
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.anchoredPosition = Vector2.zero;
            canvasRect.sizeDelta = Vector2.zero;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvasObject.transform;
        }

        private static HubHeroPreviewController CreateHeroPreview(HeroDefinition hero)
        {
            GameObject previewStage = new("PreviewStage");
            previewStage.transform.position = Vector3.zero;

            if (hero == null || hero.CharacterPrefab == null)
            {
                return null;
            }

            GameObject preview = (GameObject)PrefabUtility.InstantiatePrefab(hero.CharacterPrefab);
            preview.name = "Preview_CustomHero";
            preview.transform.SetParent(previewStage.transform, false);
            SetHubActorPose(preview, new Vector3(0f, 1.9f, 0f), 5.4f);

            SpumCharacterView characterView = preview.GetComponentInChildren<SpumCharacterView>(true);
            if (characterView == null)
            {
                characterView = preview.AddComponent<SpumCharacterView>();
            }

            HubHeroPreviewController previewController = preview.GetComponent<HubHeroPreviewController>();
            if (previewController == null)
            {
                previewController = preview.AddComponent<HubHeroPreviewController>();
            }

            SerializedObject previewSo = new(previewController);
            previewSo.FindProperty("characterView").objectReferenceValue = characterView;
            previewSo.ApplyModifiedPropertiesWithoutUndo();

            previewController.Configure(hero);
            return previewController;
        }

        private static void SetHubActorPose(GameObject actor, Vector3 worldPosition, float uniformScale)
        {
            if (actor == null)
            {
                return;
            }

            actor.transform.position = worldPosition;
            actor.transform.localScale = Vector3.one * uniformScale;
        }

        private static HubShopController CreateShopPanel(Transform canvasRoot)
        {
            GameObject panel = new("ShopPanel");
            panel.transform.SetParent(canvasRoot, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-38f, -18f);
            rect.sizeDelta = new Vector2(340f, 420f);

            Image background = panel.AddComponent<Image>();
            background.color = new Color(0.11f, 0.085f, 0.05f, 0.95f);
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.78f, 0.42f, 0.34f);
            outline.effectDistance = new Vector2(3f, -3f);

            HubShopController controller = panel.AddComponent<HubShopController>();
            WeaponDefinition pistol = AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Game/ScriptableObjects/Weapons/Weapon_Pistol.asset");
            WeaponDefinition shotgun = AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Game/ScriptableObjects/Weapons/Weapon_Shotgun.asset");
            WeaponDefinition staff = AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Game/ScriptableObjects/Weapons/Weapon_ViewerDrop.asset");

            CreateTopLeftText("ShopTitle", panel.transform, new Vector2(18f, -18f), "Blacksmith Altar", 26, FontStyles.Bold);
            GameObject wallet = CreateTopLeftText("WalletText", panel.transform, new Vector2(18f, -58f), "Crowns: 120", 18);
            GameObject selected = CreateTopLeftText("SelectedLoadoutText", panel.transform, new Vector2(18f, -88f), "Shop Loadout: Default", 15);
            GameObject hint = CreateTopLeftText("ShopHintText", panel.transform, new Vector2(18f, -118f), "Approach the altar or click it to browse weapons.", 14);
            hint.GetComponent<TMP_Text>().color = new Color(1f, 0.86f, 0.56f, 1f);
            Button closeButton = CreateActionButton("CloseShopButton", panel.transform, new Vector2(126f, -16f), "Close", new Vector2(84f, 34f), 16);

            SerializedObject so = new(controller);
            so.FindProperty("root").objectReferenceValue = panel;
            so.FindProperty("walletText").objectReferenceValue = wallet.GetComponent<TMP_Text>();
            so.FindProperty("selectedText").objectReferenceValue = selected.GetComponent<TMP_Text>();
            so.FindProperty("hintText").objectReferenceValue = hint.GetComponent<TMP_Text>();
            so.FindProperty("closeButton").objectReferenceValue = closeButton;
            so.FindProperty("items").arraySize = 3;

            WeaponDefinition[] weapons = { pistol, shotgun, staff };
            for (int i = 0; i < weapons.Length; i++)
            {
                Button button = CreateActionButton($"ShopButton_{i + 1}", panel.transform, new Vector2(0f, -182f - i * 76f), weapons[i] != null ? weapons[i].DisplayName : "Item", new Vector2(286f, 58f), 22);
                GameObject subtitle = CreateChildText($"{button.name}_Subtitle", button.transform, new Vector2(0f, -12f), new Vector2(230f, 22f), weapons[i] != null ? weapons[i].Rarity.ToString() : "Common", 14, TextAlignmentOptions.Center);
                subtitle.GetComponent<TMP_Text>().color = weapons[i] != null ? WeaponRarityPalette.GetColor(weapons[i].Rarity) : Color.white;
                GameObject price = CreateChildText($"{button.name}_Price", button.transform, new Vector2(0f, 18f), new Vector2(230f, 20f), $"{(weapons[i] != null ? weapons[i].ShopPrice : 0)} C", 13, TextAlignmentOptions.Center);

                SerializedProperty item = so.FindProperty("items").GetArrayElementAtIndex(i);
                item.FindPropertyRelative("Button").objectReferenceValue = button;
                item.FindPropertyRelative("Frame").objectReferenceValue = button.GetComponent<Image>();
                item.FindPropertyRelative("Label").objectReferenceValue = button.transform.Find($"{button.name}_Text")?.GetComponent<TMP_Text>();
                item.FindPropertyRelative("Price").objectReferenceValue = price.GetComponent<TMP_Text>();
                item.FindPropertyRelative("Weapon").objectReferenceValue = weapons[i];
                item.FindPropertyRelative("Cost").intValue = weapons[i] != null ? weapons[i].ShopPrice : 0;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            return controller;
        }

        private static void CreateCustomizerPanel(Transform canvasRoot, HeroDefinition baseHero, HubHeroPreviewController preview)
        {
            GameObject panel = new("HubPanel");
            panel.transform.SetParent(canvasRoot, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(-120f, 24f);
            panelRect.sizeDelta = new Vector2(1180f, 500f);

            Image background = panel.AddComponent<Image>();
            background.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            background.type = Image.Type.Sliced;
            background.color = new Color(0.06f, 0.09f, 0.14f, 0.84f);
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.75f, 0.88f, 1f, 0.12f);
            outline.effectDistance = new Vector2(4f, -4f);

            HubCustomizerController controller = panel.AddComponent<HubCustomizerController>();

            GameObject title = CreateCenteredText("TitleText", panel.transform, new Vector2(0f, -24f), "Forge Your Hero", 34, FontStyles.Bold);
            GameObject subtitle = CreateCenteredText("CurrentHeroText", panel.transform, new Vector2(0f, -66f), "Custom Adventurer", 20);
            GameObject description = CreateCenteredText("DescriptionText", panel.transform, new Vector2(0f, -104f), "Use the arrows to tune your loadout and look.", 16);
            GameObject stats = CreateCenteredText("HeroStatsText", panel.transform, new Vector2(0f, -144f), "Base hero stats", 16);
            stats.GetComponent<TMP_Text>().color = new Color(0.78f, 0.88f, 0.95f, 1f);

            SerializedObject controllerSo = new(controller);
            controllerSo.FindProperty("baseHeroDefinition").objectReferenceValue = baseHero;
            controllerSo.FindProperty("titleText").objectReferenceValue = title.GetComponent<TMP_Text>();
            controllerSo.FindProperty("descriptionText").objectReferenceValue = description.GetComponent<TMP_Text>();
            controllerSo.FindProperty("summaryText").objectReferenceValue = subtitle.GetComponent<TMP_Text>();
            controllerSo.FindProperty("heroStatsText").objectReferenceValue = stats.GetComponent<TMP_Text>();
            controllerSo.FindProperty("previewController").objectReferenceValue = preview;
            controllerSo.FindProperty("previewCharacterView").objectReferenceValue = preview != null ? preview.GetComponentInChildren<SpumCharacterView>(true) : null;
            controllerSo.FindProperty("customizationRows").arraySize = 10;

            GameObject rowsRoot = CreateRowsRoot(panel.transform, new Vector2(0f, -182f), new Vector2(620f, 332f));
            string[] rowNames = { "Hair", "FaceHair", "Helmet", "Armor", "Pant", "Weapon", "Back", "Eye Color", "Hair Color", "Cloth Tone" };
            for (int i = 0; i < rowNames.Length; i++)
            {
                GameObject rowRoot = CreateRowContainer($"{rowNames[i]}_Row", rowsRoot.transform, 620f, 28f);
                GameObject label = CreateLayoutText($"{rowNames[i]}_Label", rowRoot.transform, rowNames[i], 16, TextAlignmentOptions.Left, 180f);
                Button previous = CreateLayoutButton($"{rowNames[i]}_Prev", rowRoot.transform, "<", 40f, 32f, 18f);
                GameObject value = CreateLayoutText($"{rowNames[i]}_Value", rowRoot.transform, "Option", 16, TextAlignmentOptions.Center, 220f);
                value.GetComponent<TMP_Text>().color = new Color(1f, 0.9f, 0.68f, 1f);
                Button next = CreateLayoutButton($"{rowNames[i]}_Next", rowRoot.transform, ">", 40f, 32f, 18f);

                SerializedProperty row = controllerSo.FindProperty("customizationRows").GetArrayElementAtIndex(i);
                row.FindPropertyRelative("Label").objectReferenceValue = label.GetComponent<TMP_Text>();
                row.FindPropertyRelative("Value").objectReferenceValue = value.GetComponent<TMP_Text>();
                row.FindPropertyRelative("PreviousButton").objectReferenceValue = previous;
                row.FindPropertyRelative("NextButton").objectReferenceValue = next;
            }

            Button startButton = CreateActionButton("StartRunButton", panel.transform, new Vector2(0f, -490f), "Enter Run", new Vector2(260f, 56f), 24);
            controllerSo.FindProperty("startRunButton").objectReferenceValue = startButton;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateBlacksmithAltar(HubShopController shopController)
        {
            GameObject altarRoot = new("BlacksmithAltar");
            altarRoot.transform.position = new Vector3(5.8f, -1.2f, 0f);

            Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            GameObject baseObject = new("Base");
            baseObject.transform.SetParent(altarRoot.transform, false);
            SpriteRenderer baseRenderer = baseObject.AddComponent<SpriteRenderer>();
            baseRenderer.sprite = sprite;
            baseRenderer.color = new Color(0.24f, 0.21f, 0.19f, 1f);
            baseObject.transform.localScale = new Vector3(1.7f, 0.55f, 1f);

            GameObject coreObject = new("Core");
            coreObject.transform.SetParent(altarRoot.transform, false);
            coreObject.transform.localPosition = new Vector3(0f, 0.52f, 0f);
            SpriteRenderer coreRenderer = coreObject.AddComponent<SpriteRenderer>();
            coreRenderer.sprite = sprite;
            coreRenderer.color = new Color(0.4f, 0.32f, 0.24f, 1f);
            coreObject.transform.localScale = new Vector3(1.05f, 1.2f, 1f);
            coreRenderer.sortingOrder = 1;

            BoxCollider2D collider = altarRoot.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1.55f, 1.8f);
            collider.offset = new Vector2(0f, 0.38f);
            collider.isTrigger = true;

            BlacksmithAltarInteractor interactor = altarRoot.AddComponent<BlacksmithAltarInteractor>();

            GameObject promptObject = new("PromptText");
            promptObject.transform.SetParent(altarRoot.transform, false);
            promptObject.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            TextMeshPro prompt = promptObject.AddComponent<TextMeshPro>();
            prompt.alignment = TextAlignmentOptions.Center;
            prompt.fontSize = 4.2f;
            prompt.text = "Click altar to open forge";
            prompt.color = new Color(1f, 0.86f, 0.58f, 1f);

            SerializedObject so = new(interactor);
            so.FindProperty("shopController").objectReferenceValue = shopController;
            so.FindProperty("promptText").objectReferenceValue = prompt;
            so.FindProperty("altarRenderer").objectReferenceValue = coreRenderer;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateTopLeftText(string name, Transform parent, Vector2 anchoredPosition, string text, float fontSize, FontStyles style = FontStyles.Normal)
        {
            GameObject textObject = new(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(240f, 28f);

            TMP_Text tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.white;
            return textObject;
        }

        private static GameObject CreateCenteredText(string name, Transform parent, Vector2 anchoredPosition, string text, float fontSize, FontStyles style = FontStyles.Normal)
        {
            GameObject textObject = new(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(760f, 40f);

            TMP_Text tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return textObject;
        }

        private static GameObject CreateChildText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, string text, float fontSize, TextAlignmentOptions alignment)
        {
            GameObject textObject = new(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            TMP_Text tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            return textObject;
        }

        private static Button CreateActionButton(string name, Transform parent, Vector2 anchoredPosition, string label, Vector2 size, float fontSize)
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
            image.color = new Color(0.15f, 0.3f, 0.42f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            Outline outline = buttonObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.18f);
            outline.effectDistance = new Vector2(2f, -2f);

            GameObject labelText = CreateChildText($"{name}_Text", buttonObject.transform, Vector2.zero, size - new Vector2(12f, 10f), label, fontSize, TextAlignmentOptions.Center);
            TMP_Text tmp = labelText.GetComponent<TMP_Text>();
            tmp.fontStyle = FontStyles.Bold;
            RectTransform labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchoredPosition = Vector2.zero;
            return button;
        }

        private static GameObject CreateRowsRoot(Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject root = new("CustomizerRows");
            root.transform.SetParent(parent, false);
            RectTransform rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.spacing = 4f;
            layout.padding = new RectOffset(10, 10, 0, 0);
            return root;
        }

        private static GameObject CreateRowContainer(string name, Transform parent, float width, float height)
        {
            GameObject row = new(name);
            row.transform.SetParent(parent, false);
            RectTransform rect = row.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);

            LayoutElement element = row.AddComponent<LayoutElement>();
            element.preferredWidth = width;
            element.preferredHeight = height;

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.spacing = 12f;
            return row;
        }

        private static GameObject CreateLayoutText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment, float width)
        {
            GameObject textObject = new(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, 28f);

            LayoutElement layout = textObject.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 28f;

            TMP_Text tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            return textObject;
        }

        private static Button CreateLayoutButton(string name, Transform parent, string label, float width, float height, float fontSize)
        {
            Button button = CreateActionButton(name, parent, Vector2.zero, label, new Vector2(width, height), fontSize);
            LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;
            return button;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            const string hubScenePath = "Assets/Game/Scenes/Hub.unity";
            const string runScenePath = "Assets/Game/Scenes/Run_Prototype.unity";
            var orderedScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            if (System.IO.File.Exists(hubScenePath))
            {
                orderedScenes.Add(new EditorBuildSettingsScene(hubScenePath, true));
            }

            if (System.IO.File.Exists(runScenePath))
            {
                orderedScenes.Add(new EditorBuildSettingsScene(runScenePath, true));
            }

            if (!orderedScenes.Exists(scene => scene.path == scenePath))
            {
                orderedScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }

            EditorBuildSettings.scenes = orderedScenes.ToArray();
        }
    }
}
#endif
