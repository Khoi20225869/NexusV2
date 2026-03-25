#if UNITY_EDITOR
using SoulForge.Bootstrap;
using SoulForge.CameraSystem;
using SoulForge.Combat;
using SoulForge.Data;
using SoulForge.Economy;
using SoulForge.Enemies;
using SoulForge.Feedback;
using SoulForge.Player;
using SoulForge.Rooms;
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
    public static class SoulForgePrototypeSceneBuilder
    {
        [MenuItem("Tools/SoulForge/Create Demo Floor Scenes")]
        public static void CreateRunPrototypeScene()
        {
            SoulForgePrototypeAssetCreator.CreatePrototypeAssets();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Floor_01";

            CreateCamera();
            CreateEventSystem();

            GameObject bootstrapRoot = new("Bootstrap");
            bootstrapRoot.AddComponent<GameBootstrap>();
            RunController runController = bootstrapRoot.AddComponent<RunController>();
            bootstrapRoot.AddComponent<RunResetController>();
            CharacterSelectionController selectionController = bootstrapRoot.AddComponent<CharacterSelectionController>();

            GameObject player = CreatePlayer();
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            RoomController roomA = CreateRoom("Room_A", new Vector3(0f, 0f, 0f));
            RoomController roomB = CreateRoom("Room_B", new Vector3(25f, 0f, 0f));
            RoomController roomC = CreateRoom("Room_C", new Vector3(50f, 0f, 0f));

            AssignRunController(runController, playerHealth, roomA, roomB, roomC);
            CreateFinishGate(runController, roomC);

            GameObject viewerRuntime = CreateViewerRuntime(runController, playerHealth, roomA);
            StateBroadcaster stateBroadcaster = viewerRuntime.GetComponent<StateBroadcaster>();
            AssignStateBroadcaster(runController, stateBroadcaster);

            CreateUI(runController);
            AssignSelectionController(selectionController);

            string scenePath = "Assets/Game/Scenes/Floor_01.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            DuplicateFloorScene(scenePath, "Assets/Game/Scenes/Floor_02.unity");
            DuplicateFloorScene(scenePath, "Assets/Game/Scenes/Floor_03.unity");
            AddSceneToBuildSettings(scenePath);
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath));
            Debug.Log("Demo floor scenes created.");
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8f;
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.AddComponent<CameraFeedback>();
            AudioSource audioSource = cameraObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static GameObject CreatePlayer()
        {
            GameObject playerPrefab = LoadDefaultSpumUnitPrefab();
            GameObject player = playerPrefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab)
                : GameObject.CreatePrimitive(PrimitiveType.Quad);

            player.name = "Player";
            if (player.GetComponent<MeshCollider>() != null)
            {
                Object.DestroyImmediate(player.GetComponent<MeshCollider>());
            }

            player.transform.position = new Vector3(-8f, 0f, 0f);
            player.transform.localScale = playerPrefab != null ? new Vector3(1f, 1f, 1f) : new Vector3(1f, 1.5f, 1f);

            Rigidbody2D rigidbody2D = player.AddComponent<Rigidbody2D>();
            rigidbody2D.gravityScale = 0f;
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;

            BoxCollider2D collider2D = player.GetComponent<BoxCollider2D>();
            if (collider2D == null)
            {
                collider2D = player.AddComponent<BoxCollider2D>();
            }
            collider2D.isTrigger = false;

            player.AddComponent<SpumCharacterView>();
            player.AddComponent<WeaponRuntime>();
            player.AddComponent<PlayerController>();
            player.AddComponent<PlayerAim>();
            player.AddComponent<PlayerHealth>();
            player.AddComponent<PlayerWeaponController>();
            player.AddComponent<PlayerInventory>();
            player.AddComponent<WeaponRuntimeBridge>();
            player.AddComponent<InventoryInputController>();
            player.AddComponent<SpriteFlashFeedback>();

            GameObject firePoint = new("FirePoint");
            firePoint.transform.SetParent(player.transform, false);
            firePoint.transform.localPosition = new Vector3(0.8f, 0f, 0f);

            AssignPlayerAssets(player);
            return player;
        }

        private static void AssignPlayerAssets(GameObject player)
        {
            HeroDefinition hero = AssetDatabase.LoadAssetAtPath<HeroDefinition>("Assets/Game/ScriptableObjects/Heroes/Hero_Knight.asset");
            WeaponDefinition pistol = AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Game/ScriptableObjects/Weapons/Weapon_Pistol.asset");
            TransientVisualEffect transientEffect = CreateTransientEffectPrefab();
            GameFeelController gameFeelController = Object.FindFirstObjectByType<GameFeelController>(FindObjectsInactive.Include);

            SerializedObject playerController = new(player.GetComponent<PlayerController>());
            playerController.FindProperty("heroDefinition").objectReferenceValue = hero;
            playerController.FindProperty("characterView").objectReferenceValue = player.GetComponent<SpumCharacterView>();
            playerController.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject playerHealth = new(player.GetComponent<PlayerHealth>());
            playerHealth.FindProperty("heroDefinition").objectReferenceValue = hero;
            playerHealth.FindProperty("characterView").objectReferenceValue = player.GetComponent<SpumCharacterView>();
            playerHealth.FindProperty("flashFeedback").objectReferenceValue = player.GetComponent<SpriteFlashFeedback>();
            playerHealth.FindProperty("cameraFeedback").objectReferenceValue = Object.FindFirstObjectByType<CameraFeedback>();
            playerHealth.FindProperty("gameFeelController").objectReferenceValue = gameFeelController;
            playerHealth.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject playerAim = new(player.GetComponent<PlayerAim>());
            playerAim.FindProperty("weaponDefinition").objectReferenceValue = pistol;
            playerAim.FindProperty("firePoint").objectReferenceValue = player.transform.Find("FirePoint");
            playerAim.FindProperty("visualRoot").objectReferenceValue = player.transform;
            playerAim.FindProperty("characterView").objectReferenceValue = player.GetComponent<SpumCharacterView>();
            playerAim.FindProperty("weaponRuntime").objectReferenceValue = player.GetComponent<WeaponRuntime>();
            playerAim.FindProperty("cameraFeedback").objectReferenceValue = Object.FindFirstObjectByType<CameraFeedback>();
            playerAim.FindProperty("hitEffectPrefab").objectReferenceValue = transientEffect;
            playerAim.FindProperty("gameFeelController").objectReferenceValue = gameFeelController;
            playerAim.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject weaponRuntime = new(player.GetComponent<WeaponRuntime>());
            weaponRuntime.FindProperty("weaponDefinition").objectReferenceValue = pistol;
            weaponRuntime.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject playerWeaponController = new(player.GetComponent<PlayerWeaponController>());
            playerWeaponController.FindProperty("weaponRuntime").objectReferenceValue = player.GetComponent<WeaponRuntime>();
            playerWeaponController.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject weaponBridge = new(player.GetComponent<WeaponRuntimeBridge>());
            weaponBridge.FindProperty("weaponRuntime").objectReferenceValue = player.GetComponent<WeaponRuntime>();
            weaponBridge.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject inventory = new(player.GetComponent<PlayerInventory>());
            inventory.FindProperty("playerWeaponController").objectReferenceValue = player.GetComponent<PlayerWeaponController>();
            inventory.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Projectile CreateProjectilePrototypePrefab()
        {
            const string prefabPath = "Assets/Game/Prefabs/Weapons/Projectile_Player.prefab";
            Projectile existing = AssetDatabase.LoadAssetAtPath<Projectile>(prefabPath);
            if (existing != null)
            {
                return existing;
            }

            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Quad);
            projectile.name = "Projectile_Player";
            Object.DestroyImmediate(projectile.GetComponent<MeshCollider>());
            projectile.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
            CircleCollider2D collider2D = projectile.AddComponent<CircleCollider2D>();
            collider2D.isTrigger = true;
            Projectile projectileComponent = projectile.AddComponent<Projectile>();
            SerializedObject projectileSo = new(projectileComponent);
            projectileSo.FindProperty("impactEffectPrefab").objectReferenceValue = CreateTransientEffectPrefab();
            projectileSo.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(projectile, prefabPath);
            Object.DestroyImmediate(projectile);
            return AssetDatabase.LoadAssetAtPath<Projectile>(prefabPath);
        }

        private static RoomController CreateRoom(string roomName, Vector3 position)
        {
            GameObject room = new(roomName);
            room.transform.position = position;

            RoomController roomController = room.AddComponent<RoomController>();
            RewardSpawner rewardSpawner = room.AddComponent<RewardSpawner>();
            ConfigureRewardSpawner(room.transform, rewardSpawner);

            SerializedObject roomControllerSo = new(roomController);
            roomControllerSo.FindProperty("roomId").stringValue = roomName.ToLowerInvariant();
            roomControllerSo.FindProperty("roomTier").enumValueIndex = roomName switch
            {
                "Room_A" => 0,
                "Room_B" => 1,
                _ => 2
            };
            roomControllerSo.FindProperty("rewardSpawner").objectReferenceValue = rewardSpawner;
            roomControllerSo.ApplyModifiedPropertiesWithoutUndo();
            rewardSpawner.OwnerRoom = roomController;

            EnemySpawner spawner = room.AddComponent<EnemySpawner>();
            SerializedObject spawnerSo = new(spawner);
            spawnerSo.FindProperty("ownerRoom").objectReferenceValue = roomController;
            spawnerSo.FindProperty("spawnOnStart").boolValue = roomName == "Room_A";
            spawnerSo.ApplyModifiedPropertiesWithoutUndo();

            CreateSpawnPoint(room.transform, "WeakSpawn", "weak_spawn", new Vector3(4f, 0f, 0f), spawner, false);
            CreateSpawnPoint(room.transform, "EliteSpawn", "elite_spawn", new Vector3(6f, 0f, 0f), spawner, true);

            return roomController;
        }

        private static void ConfigureRewardSpawner(Transform roomRoot, RewardSpawner rewardSpawner)
        {
            if (rewardSpawner == null)
            {
                return;
            }

            RewardDefinition reward = AssetDatabase.LoadAssetAtPath<RewardDefinition>("Assets/Game/ScriptableObjects/Rewards/Reward_Default.asset");
            WeaponDefinition pistol = AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Game/ScriptableObjects/Weapons/Weapon_Pistol.asset");
            WeaponDefinition shotgun = AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Game/ScriptableObjects/Weapons/Weapon_Shotgun.asset");
            WeaponDefinition viewerDrop = AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Game/ScriptableObjects/Weapons/Weapon_ViewerDrop.asset");
            WeaponChest chestPrefab = CreateWeaponChestPrefab();

            GameObject chestPoint = new("ChestSpawn");
            chestPoint.transform.SetParent(roomRoot, false);
            chestPoint.transform.localPosition = new Vector3(1.5f, -0.5f, 0f);

            SerializedObject so = new(rewardSpawner);
            SerializedProperty rewards = so.FindProperty("possibleRewards");
            rewards.ClearArray();
            rewards.InsertArrayElementAtIndex(0);
            rewards.GetArrayElementAtIndex(0).objectReferenceValue = reward;

            SerializedProperty chestWeapons = so.FindProperty("chestWeapons");
            chestWeapons.ClearArray();
            WeaponDefinition[] weaponPool = { pistol, shotgun, viewerDrop };
            for (int i = 0; i < weaponPool.Length; i++)
            {
                chestWeapons.InsertArrayElementAtIndex(i);
                chestWeapons.GetArrayElementAtIndex(i).objectReferenceValue = weaponPool[i];
            }

            SerializedProperty lootTable = so.FindProperty("chestLootTable");
            lootTable.ClearArray();
            for (int i = 0; i < weaponPool.Length; i++)
            {
                lootTable.InsertArrayElementAtIndex(i);
                SerializedProperty lootEntry = lootTable.GetArrayElementAtIndex(i);
                lootEntry.FindPropertyRelative("Weapon").objectReferenceValue = weaponPool[i];
                lootEntry.FindPropertyRelative("Weight").intValue = weaponPool[i] != null ? Mathf.Max(1, weaponPool[i].ChestWeight) : 1;
            }

            so.FindProperty("chestPrefab").objectReferenceValue = chestPrefab;
            so.FindProperty("chestSpawnPoint").objectReferenceValue = chestPoint.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateSpawnPoint(Transform roomRoot, string name, string spawnId, Vector3 localPosition, EnemySpawner spawner, bool elite)
        {
            GameObject spawnPoint = new(name);
            spawnPoint.transform.SetParent(roomRoot);
            spawnPoint.transform.localPosition = localPosition;

            EnemyController enemyPrefab = CreateEnemyPrototypePrefab(elite);
            EnemyDefinition definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(
                elite
                    ? "Assets/Game/ScriptableObjects/Enemies/Enemy_Elite.asset"
                    : "Assets/Game/ScriptableObjects/Enemies/Enemy_Weak.asset");

            SerializedObject so = new(spawner);
            SerializedProperty entries = so.FindProperty("spawnEntries");
            int index = entries.arraySize;
            entries.InsertArrayElementAtIndex(index);
            SerializedProperty entry = entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("SpawnId").stringValue = spawnId;
            entry.FindPropertyRelative("Prefab").objectReferenceValue = enemyPrefab;
            entry.FindPropertyRelative("Definition").objectReferenceValue = definition;
            entry.FindPropertyRelative("SpawnPoint").objectReferenceValue = spawnPoint.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static EnemyController CreateEnemyPrototypePrefab(bool elite)
        {
            string prefabName = elite ? "EnemyElitePrototype" : "EnemyWeakPrototype";
            string prefabPath = $"Assets/Game/Prefabs/Enemies/{prefabName}.prefab";
            GameObject sourcePrefab = LoadDefaultSpumUnitPrefab();
            GameObject enemy = sourcePrefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab)
                : GameObject.CreatePrimitive(PrimitiveType.Quad);

            enemy.name = prefabName;
            ConfigureEnemyPrototype(enemy, elite);

            PrefabUtility.SaveAsPrefabAsset(enemy, prefabPath);
            Object.DestroyImmediate(enemy);
            return AssetDatabase.LoadAssetAtPath<EnemyController>(prefabPath);
        }

        private static void ConfigureEnemyPrototype(GameObject enemy, bool elite)
        {
            if (enemy.GetComponent<MeshCollider>() != null)
            {
                Object.DestroyImmediate(enemy.GetComponent<MeshCollider>());
            }

            enemy.transform.localScale = elite ? new Vector3(1.2f, 1.2f, 1f) : new Vector3(1f, 1f, 1f);

            if (enemy.GetComponent<BoxCollider2D>() == null)
            {
                enemy.AddComponent<BoxCollider2D>();
            }

            SpumCharacterView characterView = enemy.GetComponent<SpumCharacterView>();
            if (characterView == null)
            {
                characterView = enemy.AddComponent<SpumCharacterView>();
            }

            if (enemy.GetComponent<SpriteFlashFeedback>() == null)
            {
                enemy.AddComponent<SpriteFlashFeedback>();
            }

            EnemyController controller = enemy.GetComponent<EnemyController>();
            if (controller == null)
            {
                controller = enemy.AddComponent<EnemyController>();
            }

            Projectile projectilePrefab = CreateEnemyProjectilePrototypePrefab(elite);

            Transform firePoint = enemy.transform.Find("FirePoint");
            if (firePoint == null)
            {
                GameObject firePointObject = new("FirePoint");
                firePointObject.transform.SetParent(enemy.transform, false);
                firePointObject.transform.localPosition = new Vector3(0.65f, 0.1f, 0f);
                firePoint = firePointObject.transform;
            }

            EnemyDefinition definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(
                elite
                    ? "Assets/Game/ScriptableObjects/Enemies/Enemy_Elite.asset"
                    : "Assets/Game/ScriptableObjects/Enemies/Enemy_Weak.asset");
            SerializedObject so = new(controller);
            so.FindProperty("definition").objectReferenceValue = definition;
            so.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
            so.FindProperty("firePoint").objectReferenceValue = firePoint;
            so.FindProperty("characterView").objectReferenceValue = characterView;
            so.FindProperty("flashFeedback").objectReferenceValue = enemy.GetComponent<SpriteFlashFeedback>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Projectile CreateEnemyProjectilePrototypePrefab(bool elite)
        {
            string prefabName = elite ? "Projectile_EnemyElite" : "Projectile_EnemyWeak";
            string prefabPath = $"Assets/Game/Prefabs/Weapons/{prefabName}.prefab";
            Projectile existing = AssetDatabase.LoadAssetAtPath<Projectile>(prefabPath);
            if (existing != null)
            {
                return existing;
            }

            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Quad);
            projectile.name = prefabName;
            Object.DestroyImmediate(projectile.GetComponent<MeshCollider>());
            projectile.transform.localScale = elite ? new Vector3(0.35f, 0.35f, 1f) : new Vector3(0.25f, 0.25f, 1f);

            MeshRenderer renderer = projectile.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"))
                {
                    color = elite ? new Color(1f, 0.45f, 0.2f, 1f) : new Color(1f, 0.2f, 0.2f, 1f)
                };
            }

            CircleCollider2D collider2D = projectile.AddComponent<CircleCollider2D>();
            collider2D.isTrigger = true;
            Projectile projectileComponent = projectile.AddComponent<Projectile>();
            SerializedObject projectileSo = new(projectileComponent);
            projectileSo.FindProperty("impactEffectPrefab").objectReferenceValue = CreateTransientEffectPrefab();
            projectileSo.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(projectile, prefabPath);
            Object.DestroyImmediate(projectile);
            return AssetDatabase.LoadAssetAtPath<Projectile>(prefabPath);
        }

        private static WeaponChest CreateWeaponChestPrefab()
        {
            const string prefabPath = "Assets/Game/Prefabs/Interactables/WeaponChest.prefab";
            WeaponChest existing = AssetDatabase.LoadAssetAtPath<WeaponChest>(prefabPath);
            if (existing != null)
            {
                return existing;
            }

            EnsurePrefabFolders();

            GameObject chest = new("WeaponChest");
            SpriteRenderer renderer = chest.AddComponent<SpriteRenderer>();
            renderer.color = new Color(0.68f, 0.46f, 0.16f, 1f);

            BoxCollider2D collider = chest.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.4f, 1.1f);

            WeaponChest weaponChest = chest.AddComponent<WeaponChest>();

            GameObject prompt = new("Prompt");
            prompt.transform.SetParent(chest.transform, false);
            prompt.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            TMP_Text promptText = prompt.AddComponent<TextMeshPro>();
            promptText.fontSize = 2.5f;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.color = Color.white;

            GameObject dropPoint = new("DropPoint");
            dropPoint.transform.SetParent(chest.transform, false);
            dropPoint.transform.localPosition = new Vector3(0f, 0.8f, 0f);

            SerializedObject so = new(weaponChest);
            so.FindProperty("pickupPrefab").objectReferenceValue = CreateWeaponPickupPrefab();
            so.FindProperty("dropPoint").objectReferenceValue = dropPoint.transform;
            so.FindProperty("promptText").objectReferenceValue = promptText;
            so.FindProperty("chestRenderer").objectReferenceValue = renderer;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(chest, prefabPath);
            Object.DestroyImmediate(chest);
            return AssetDatabase.LoadAssetAtPath<WeaponChest>(prefabPath);
        }

        private static WeaponPickup CreateWeaponPickupPrefab()
        {
            const string prefabPath = "Assets/Game/Prefabs/Interactables/WeaponPickup.prefab";
            WeaponPickup existing = AssetDatabase.LoadAssetAtPath<WeaponPickup>(prefabPath);
            if (existing != null)
            {
                return existing;
            }

            EnsurePrefabFolders();

            GameObject pickup = new("WeaponPickup");
            SpriteRenderer renderer = pickup.AddComponent<SpriteRenderer>();
            renderer.color = new Color(0.2f, 0.9f, 1f, 1f);

            CircleCollider2D collider = pickup.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.45f;

            WeaponPickup weaponPickup = pickup.AddComponent<WeaponPickup>();

            GameObject label = new("Label");
            label.transform.SetParent(pickup.transform, false);
            label.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            TMP_Text labelText = label.AddComponent<TextMeshPro>();
            labelText.fontSize = 2f;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;

            SerializedObject so = new(weaponPickup);
            so.FindProperty("labelText").objectReferenceValue = labelText;
            so.FindProperty("frameRenderer").objectReferenceValue = renderer;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(pickup, prefabPath);
            Object.DestroyImmediate(pickup);
            return AssetDatabase.LoadAssetAtPath<WeaponPickup>(prefabPath);
        }

        private static void EnsurePrefabFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/Game", "Prefabs");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Game/Prefabs/Interactables"))
            {
                AssetDatabase.CreateFolder("Assets/Game/Prefabs", "Interactables");
            }
        }

        private static TransientVisualEffect CreateTransientEffectPrefab()
        {
            const string prefabPath = "Assets/Game/Prefabs/Interactables/TransientEffect.prefab";
            TransientVisualEffect existing = AssetDatabase.LoadAssetAtPath<TransientVisualEffect>(prefabPath);
            if (existing != null)
            {
                return existing;
            }

            EnsurePrefabFolders();

            GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Quad);
            effect.name = "TransientEffect";
            Object.DestroyImmediate(effect.GetComponent<MeshCollider>());
            effect.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
            effect.AddComponent<TransientVisualEffect>();

            PrefabUtility.SaveAsPrefabAsset(effect, prefabPath);
            Object.DestroyImmediate(effect);
            return AssetDatabase.LoadAssetAtPath<TransientVisualEffect>(prefabPath);
        }

        private static void AssignRunController(RunController runController, PlayerHealth playerHealth, RoomController roomA, RoomController roomB, RoomController roomC)
        {
            SerializedObject so = new(runController);
            SerializedProperty roomSequence = so.FindProperty("roomSequence");
            roomSequence.ClearArray();
            RoomController[] rooms = { roomA, roomB, roomC };
            for (int i = 0; i < rooms.Length; i++)
            {
                roomSequence.InsertArrayElementAtIndex(i);
                roomSequence.GetArrayElementAtIndex(i).objectReferenceValue = rooms[i];
            }

            so.FindProperty("playerHealth").objectReferenceValue = playerHealth;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateViewerRuntime(RunController runController, PlayerHealth playerHealth, RoomController roomA)
        {
            GameObject viewerRuntime = new("ViewerRuntime");
            ViewerSessionService sessionService = viewerRuntime.AddComponent<ViewerSessionService>();
            ViewerEconomyService economyService = viewerRuntime.AddComponent<ViewerEconomyService>();
            ViewerRoomBudgetService budgetService = viewerRuntime.AddComponent<ViewerRoomBudgetService>();
            ViewerActionValidator validator = viewerRuntime.AddComponent<ViewerActionValidator>();
            ViewerActionQueue queue = viewerRuntime.AddComponent<ViewerActionQueue>();
            StateBroadcaster broadcaster = viewerRuntime.AddComponent<StateBroadcaster>();
            ViewerActionExecutor executor = viewerRuntime.AddComponent<ViewerActionExecutor>();
            viewerRuntime.AddComponent<LocalViewerCommandTester>();

            ViewerStoreCatalog storeCatalog = AssetDatabase.LoadAssetAtPath<ViewerStoreCatalog>("Assets/Game/ScriptableObjects/Viewer/ViewerStoreCatalog.asset");
            ViewerEconomyConfig economyConfig = AssetDatabase.LoadAssetAtPath<ViewerEconomyConfig>("Assets/Game/ScriptableObjects/Viewer/ViewerEconomyConfig.asset");
            WeaponDefinition viewerDropWeapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Game/ScriptableObjects/Weapons/Weapon_ViewerDrop.asset");
            WeaponPickup viewerPickupPrefab = CreateWeaponPickupPrefab();

            SerializedObject economySo = new(economyService);
            economySo.FindProperty("config").objectReferenceValue = economyConfig;
            economySo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject budgetSo = new(budgetService);
            budgetSo.FindProperty("config").objectReferenceValue = economyConfig;
            budgetSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject validatorSo = new(validator);
            validatorSo.FindProperty("storeCatalog").objectReferenceValue = storeCatalog;
            validatorSo.FindProperty("economyService").objectReferenceValue = economyService;
            validatorSo.FindProperty("roomBudgetService").objectReferenceValue = budgetService;
            validatorSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject queueSo = new(queue);
            queueSo.FindProperty("config").objectReferenceValue = economyConfig;
            queueSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject executorSo = new(executor);
            executorSo.FindProperty("validator").objectReferenceValue = validator;
            executorSo.FindProperty("queue").objectReferenceValue = queue;
            executorSo.FindProperty("economyService").objectReferenceValue = economyService;
            executorSo.FindProperty("broadcaster").objectReferenceValue = broadcaster;
            executorSo.FindProperty("enemySpawner").objectReferenceValue = roomA.GetComponent<EnemySpawner>();
            executorSo.FindProperty("playerHealth").objectReferenceValue = playerHealth;
            executorSo.FindProperty("playerWeaponController").objectReferenceValue = playerHealth.GetComponent<PlayerWeaponController>();
            executorSo.FindProperty("playerInventory").objectReferenceValue = playerHealth.GetComponent<PlayerInventory>();
            executorSo.FindProperty("runController").objectReferenceValue = runController;
            executorSo.FindProperty("viewerActionQueue").objectReferenceValue = queue;
            executorSo.FindProperty("roomBudgetService").objectReferenceValue = budgetService;
            executorSo.FindProperty("fallbackViewerWeapon").objectReferenceValue = viewerDropWeapon;
            executorSo.FindProperty("viewerWeaponPickupPrefab").objectReferenceValue = viewerPickupPrefab;
            executorSo.ApplyModifiedPropertiesWithoutUndo();

            _ = sessionService;
            return viewerRuntime;
        }

        private static void AssignStateBroadcaster(RunController runController, StateBroadcaster stateBroadcaster)
        {
            SerializedObject so = new(runController);
            so.FindProperty("stateBroadcaster").objectReferenceValue = stateBroadcaster;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateFinishGate(RunController runController, RoomController finalRoom)
        {
            GameObject gate = new("FinishGate");
            gate.transform.position = new Vector3(8.5f, -1.25f, 0f);
            BoxCollider2D collider = gate.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1.6f, 1.6f);
            collider.isTrigger = true;

            RunFinishGate finishGate = gate.AddComponent<RunFinishGate>();
            SerializedObject so = new(finishGate);
            so.FindProperty("runController").objectReferenceValue = runController;
            so.FindProperty("ownerRoom").objectReferenceValue = finalRoom;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateUI(RunController runController)
        {
            GameObject canvasObject = new("UIRoot");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject flash = new("ScreenFlash");
            flash.transform.SetParent(canvasObject.transform, false);
            RectTransform flashRect = flash.AddComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.offsetMin = Vector2.zero;
            flashRect.offsetMax = Vector2.zero;
            Image flashImage = flash.AddComponent<Image>();
            flashImage.color = Color.clear;

            GameObject feedback = new("GameFeel");
            feedback.transform.SetParent(canvasObject.transform, false);
            GameFeelController gameFeelController = feedback.AddComponent<GameFeelController>();
            AudioSource audioSource = Camera.main != null ? Camera.main.GetComponent<AudioSource>() : null;
            SerializedObject feelSo = new(gameFeelController);
            feelSo.FindProperty("screenFlashImage").objectReferenceValue = flashImage;
            feelSo.FindProperty("audioSource").objectReferenceValue = audioSource;
            feelSo.ApplyModifiedPropertiesWithoutUndo();

            GameObject runProgress = CreateText("RunProgressText", canvasObject.transform, new Vector2(140f, -20f), "Floor 1/3");
            RunProgressPresenter progressPresenter = runProgress.AddComponent<RunProgressPresenter>();
            SerializedObject progressSo = new(progressPresenter);
            progressSo.FindProperty("runController").objectReferenceValue = runController;
            progressSo.FindProperty("progressText").objectReferenceValue = runProgress.GetComponent<TMP_Text>();
            progressSo.ApplyModifiedPropertiesWithoutUndo();
            CreateInventoryPanel(canvasObject.transform);
            CreateResultScreen(canvasObject.transform);
        }

        private static void CreateInventoryPanel(Transform canvasRoot)
        {
            GameObject inventoryRoot = new("InventoryPanel");
            inventoryRoot.transform.SetParent(canvasRoot, false);
            RectTransform rootRect = inventoryRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(1f, 1f);
            rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.pivot = new Vector2(1f, 1f);
            rootRect.anchoredPosition = new Vector2(-24f, -24f);
            rootRect.sizeDelta = new Vector2(156f, 40f);

            InventoryPresenter presenter = inventoryRoot.AddComponent<InventoryPresenter>();

            Button toggleButton = CreateActionButton("InventoryToggleButton", inventoryRoot.transform, new Vector2(-78f, 0f), "Inventory", new Vector2(156f, 40f));
            RectTransform toggleRect = toggleButton.GetComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(1f, 1f);
            toggleRect.anchorMax = new Vector2(1f, 1f);
            toggleRect.pivot = new Vector2(1f, 1f);
            toggleRect.anchoredPosition = Vector2.zero;

            SerializedObject presenterSo = new(presenter);
            presenterSo.FindProperty("toggleButton").objectReferenceValue = toggleButton;
            presenterSo.FindProperty("toggleButtonText").objectReferenceValue = toggleButton.GetComponentInChildren<TMP_Text>();
            presenterSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateText(string name, Transform parent, Vector2 anchoredPosition, string text)
        {
            GameObject textObject = new(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(320f, 100f);

            TMP_Text tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 20;
            tmp.color = Color.white;
            return textObject;
        }

        private static void CreateResultScreen(Transform canvasRoot)
        {
            GameObject resultRoot = new("ResultScreen");
            resultRoot.transform.SetParent(canvasRoot, false);
            RectTransform rect = resultRoot.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(420f, 220f);

            Image background = resultRoot.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.82f);

            ResultScreenPresenter presenter = resultRoot.AddComponent<ResultScreenPresenter>();
            ResultScreenButtonsPresenter buttonsPresenter = resultRoot.AddComponent<ResultScreenButtonsPresenter>();

            GameObject title = CreateCenteredText("TitleText", resultRoot.transform, new Vector2(0f, -20f), "Run Result", 32);
            GameObject summary = CreateCenteredText("SummaryText", resultRoot.transform, new Vector2(0f, -70f), "Summary", 20);
            Button restartButton = CreateActionButton("RestartButton", resultRoot.transform, new Vector2(-90f, -150f), "Restart");
            Button hubButton = CreateActionButton("HubButton", resultRoot.transform, new Vector2(90f, -150f), "Hub");

            SerializedObject presenterSo = new(presenter);
            presenterSo.FindProperty("root").objectReferenceValue = resultRoot;
            presenterSo.FindProperty("titleText").objectReferenceValue = title.GetComponent<TMP_Text>();
            presenterSo.FindProperty("summaryText").objectReferenceValue = summary.GetComponent<TMP_Text>();
            presenterSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject buttonsSo = new(buttonsPresenter);
            buttonsSo.FindProperty("restartButton").objectReferenceValue = restartButton;
            buttonsSo.FindProperty("hubButton").objectReferenceValue = hubButton;
            buttonsSo.ApplyModifiedPropertiesWithoutUndo();

            resultRoot.SetActive(false);
        }

        private static GameObject CreateCenteredText(string name, Transform parent, Vector2 anchoredPosition, string text, float fontSize)
        {
            GameObject textObject = new(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(360f, 40f);

            TMP_Text tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return textObject;
        }

        private static Button CreateActionButton(string name, Transform parent, Vector2 anchoredPosition, string label)
        {
            return CreateActionButton(name, parent, anchoredPosition, label, new Vector2(140f, 42f));
        }

        private static Button CreateActionButton(string name, Transform parent, Vector2 anchoredPosition, string label, Vector2 size)
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
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            Button button = buttonObject.AddComponent<Button>();

            GameObject labelText = CreateCenteredText($"{name}_Text", buttonObject.transform, new Vector2(0f, 0f), label, 20);
            RectTransform labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = size - new Vector2(20f, 12f);

            return button;
        }

        private static void AssignSelectionController(CharacterSelectionController selectionController)
        {
            HeroRosterDefinition roster = AssetDatabase.LoadAssetAtPath<HeroRosterDefinition>("Assets/Game/ScriptableObjects/Heroes/HeroRoster.asset");

            SerializedObject so = new(selectionController);
            so.FindProperty("heroRoster").objectReferenceValue = roster;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject LoadDefaultSpumUnitPrefab()
        {
            GameObject unitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/SPUM/SPUM_Units/Unit0.prefab");
            if (unitPrefab != null)
            {
                return unitPrefab;
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SPUM/Prefab/UnitSave.prefab");
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            _ = scenePath;
            SoulForgeBoosterSceneBuilder.AddSceneToBuildSettings();
        }

        private static void DuplicateFloorScene(string sourceScenePath, string targetScenePath)
        {
            if (!System.IO.File.Exists(sourceScenePath))
            {
                return;
            }

            string sceneYaml = System.IO.File.ReadAllText(sourceScenePath);
            System.IO.File.WriteAllText(targetScenePath, sceneYaml);
        }
    }
}
#endif
