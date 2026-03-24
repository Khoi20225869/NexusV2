#if UNITY_EDITOR
using System.IO;
using SoulForge.Data;
using SoulForge.Economy;
using UnityEditor;
using UnityEngine;

namespace SoulForge.Editor
{
    public static class SoulForgePrototypeAssetCreator
    {
        private const string Root = "Assets/Game/ScriptableObjects";

        [MenuItem("Tools/SoulForge/Create Prototype Assets")]
        public static void CreatePrototypeAssets()
        {
            EnsureFolders();

            HeroDefinition hero = CreateAsset<HeroDefinition>($"{Root}/Heroes/Hero_Default.asset");
            HeroDefinition knight = CreateAsset<HeroDefinition>($"{Root}/Heroes/Hero_Knight.asset");
            HeroDefinition ranger = CreateAsset<HeroDefinition>($"{Root}/Heroes/Hero_Ranger.asset");
            HeroDefinition mage = CreateAsset<HeroDefinition>($"{Root}/Heroes/Hero_Mage.asset");
            HeroRosterDefinition roster = CreateAsset<HeroRosterDefinition>($"{Root}/Heroes/HeroRoster.asset");
            WeaponDefinition pistol = CreateAsset<WeaponDefinition>($"{Root}/Weapons/Weapon_Pistol.asset");
            WeaponDefinition shotgun = CreateAsset<WeaponDefinition>($"{Root}/Weapons/Weapon_Shotgun.asset");
            WeaponDefinition viewerDropWeapon = CreateAsset<WeaponDefinition>($"{Root}/Weapons/Weapon_ViewerDrop.asset");

            EnemyDefinition weakEnemy = CreateAsset<EnemyDefinition>($"{Root}/Enemies/Enemy_Weak.asset");
            EnemyDefinition eliteEnemy = CreateAsset<EnemyDefinition>($"{Root}/Enemies/Enemy_Elite.asset");
            RewardDefinition reward = CreateAsset<RewardDefinition>($"{Root}/Rewards/Reward_Default.asset");
            EncounterDefinition encounter = CreateAsset<EncounterDefinition>($"{Root}/Encounters/Encounter_Default.asset");

            ViewerActionDefinition spawnWeak = CreateAsset<ViewerActionDefinition>($"{Root}/Viewer/ViewerAction_SpawnWeak.asset");
            ViewerActionDefinition spawnElite = CreateAsset<ViewerActionDefinition>($"{Root}/Viewer/ViewerAction_SpawnElite.asset");
            ViewerActionDefinition dropHeal = CreateAsset<ViewerActionDefinition>($"{Root}/Viewer/ViewerAction_DropHeal.asset");
            ViewerActionDefinition dropWeapon = CreateAsset<ViewerActionDefinition>($"{Root}/Viewer/ViewerAction_DropWeapon.asset");
            ViewerStoreCatalog storeCatalog = CreateAsset<ViewerStoreCatalog>($"{Root}/Viewer/ViewerStoreCatalog.asset");
            ViewerEconomyConfig economyConfig = CreateAsset<ViewerEconomyConfig>($"{Root}/Viewer/ViewerEconomyConfig.asset");

            AssignWeaponDefaults(pistol, "weapon_pistol", "Pulse Pistol", "Balanced automatic sidearm.", WeaponRarity.Common, RoomTier.Tier1, RoomTier.Tier3, 25, 12, new Color(0.64f, 0.88f, 1f), 1f, 4f, 14f);
            AssignWeaponDefaults(shotgun, "weapon_shotgun", "Scatter Blaster", "Short-range burst weapon.", WeaponRarity.Uncommon, RoomTier.Tier2, RoomTier.Tier3, 50, 7, new Color(0.52f, 1f, 0.62f), 2f, 1.2f, 11f);
            AssignWeaponDefaults(viewerDropWeapon, "weapon_viewer_drop", "Arc Staff", "Viewer-funded energy weapon.", WeaponRarity.Rare, RoomTier.Tier3, RoomTier.Tier3, 65, 4, new Color(0.98f, 0.58f, 1f), 1.5f, 2.5f, 13f);

            GameObject spumHeroPrefab = LoadBaseSpumUnitPrefab();
            GameObject knightPrefab = CreateHeroPrefab(
                "Hero_Knight",
                new Color(0.36f, 0.85f, 1f),
                new Color(0.78f, 0.72f, 0.28f),
                "Assets/Resources/SPUM/SPUM_Sprites/Items/6_Weapons/Sword_1.png",
                string.Empty,
                new Color(0.92f, 0.92f, 1f, 1f));
            GameObject rangerPrefab = CreateHeroPrefab(
                "Hero_Ranger",
                new Color(0.55f, 1f, 0.55f),
                new Color(0.18f, 0.7f, 0.28f),
                "Assets/Resources/SPUM/SPUM_Sprites/Items/6_Weapons/Bow_1.png",
                "Assets/Resources/SPUM/SPUM_Sprites/Items/7_Back/BowBack_1.png",
                new Color(0.82f, 1f, 0.82f, 1f));
            GameObject magePrefab = CreateHeroPrefab(
                "Hero_Mage",
                new Color(1f, 0.48f, 0.92f),
                new Color(0.64f, 0.3f, 1f),
                "Assets/Resources/SPUM/SPUM_Sprites/Items/6_Weapons/Ward_1.png",
                "Assets/Resources/SPUM/SPUM_Sprites/Items/7_Back/Back_3.png",
                new Color(0.92f, 0.84f, 1f, 1f));

            AssignHeroDefaults(hero, "hero_default", "Knight", "Balanced frontliner with reliable pistol shots.", pistol, spumHeroPrefab, 6f, 4f, 5f, 4f, 8);
            AssignHeroDefaults(knight, "hero_knight", "Knight", "Balanced frontliner with reliable pistol shots.", pistol, knightPrefab, 7f, 4f, 5f, 4f, 8, 4, new Color(0.36f, 0.85f, 1f), new Color(0.78f, 0.72f, 0.28f));
            AssignHeroDefaults(ranger, "hero_ranger", "Ranger", "Fast skirmisher built around burst damage.", shotgun, rangerPrefab, 5f, 3f, 6.25f, 3f, 7, 5, new Color(0.55f, 1f, 0.55f), new Color(0.18f, 0.7f, 0.28f));
            AssignHeroDefaults(mage, "hero_mage", "Mage", "Fragile caster with high utility weapon drops.", viewerDropWeapon, magePrefab, 4.5f, 5f, 5.25f, 5f, 9, 6, new Color(1f, 0.48f, 0.92f), new Color(0.64f, 0.3f, 1f));
            AssignRosterDefaults(roster, knight, ranger, mage);

            AssignEnemyDefaults(weakEnemy, "enemy_weak", 3f, 2.6f, 4f, 1.2f, 1f, true, 8f);
            AssignEnemyDefaults(eliteEnemy, "enemy_elite", 8f, 2.1f, 5f, 0.8f, 2f, true, 9f);
            AssignRewardDefaults(reward);
            AssignEncounterDefaults(encounter, weakEnemy);

            AssignViewerActionDefaults(spawnWeak, "spawn_weak_enemy", "Summon Grunt", "Send a basic enemy into the room.", Viewer.ViewerActionCategory.Hostile, 20, 4f, 1, "weak_spawn", 0f, new Color(0.85f, 0.34f, 0.34f));
            AssignViewerActionDefaults(spawnElite, "spawn_elite_enemy", "Summon Elite", "Drop a dangerous elite into the fight.", Viewer.ViewerActionCategory.Hostile, 75, 12f, 3, "elite_spawn", 0f, new Color(0.92f, 0.54f, 0.16f));
            AssignViewerActionDefaults(dropHeal, "drop_heal", "Emergency Heal", "Restore a small chunk of host HP.", Viewer.ViewerActionCategory.Support, 30, 8f, 1, string.Empty, 2f, new Color(0.26f, 0.74f, 0.42f));
            AssignViewerActionDefaults(dropWeapon, "drop_random_weapon", "Forge Weapon", "Drop a funded weapon for the host inventory.", Viewer.ViewerActionCategory.Support, 55, 10f, 2, string.Empty, 0f, new Color(0.34f, 0.58f, 0.95f));
            AssignStoreDefaults(storeCatalog, spawnWeak, spawnElite, dropHeal, dropWeapon);
            AssignEconomyDefaults(economyConfig);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = storeCatalog;
            Debug.Log("SoulForge prototype assets created/refreshed.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Game");
            EnsureFolder("Assets/Game/ScriptableObjects");
            EnsureFolder($"{Root}/Heroes");
            EnsureFolder($"{Root}/Weapons");
            EnsureFolder($"{Root}/Enemies");
            EnsureFolder($"{Root}/Encounters");
            EnsureFolder($"{Root}/Rewards");
            EnsureFolder($"{Root}/Viewer");
            EnsureFolder("Assets/Game/Prefabs");
            EnsureFolder("Assets/Game/Prefabs/Heroes");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string folderName = Path.GetFileName(path);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static T CreateAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void AssignHeroDefaults(
            HeroDefinition hero,
            string heroId,
            string displayName,
            string description,
            WeaponDefinition startingWeapon,
            GameObject characterPrefab,
            float maxHealth,
            float maxShield,
            float moveSpeed,
            float dashCooldown,
            int inventoryCapacity,
            int attackAnimationId = 4,
            Color? eyeColor = null,
            Color? hairColor = null)
        {
            SerializedObject so = new(hero);
            so.FindProperty("<HeroId>k__BackingField").stringValue = heroId;
            so.FindProperty("<DisplayName>k__BackingField").stringValue = displayName;
            so.FindProperty("<Description>k__BackingField").stringValue = description;
            so.FindProperty("<StartingWeapon>k__BackingField").objectReferenceValue = startingWeapon;
            so.FindProperty("<CharacterPrefab>k__BackingField").objectReferenceValue = characterPrefab;
            so.FindProperty("<InventoryCapacity>k__BackingField").intValue = inventoryCapacity;
            so.FindProperty("<AttackAnimationId>k__BackingField").intValue = attackAnimationId;
            so.FindProperty("<EyeColor>k__BackingField").colorValue = eyeColor ?? Color.white;
            so.FindProperty("<HairColor>k__BackingField").colorValue = hairColor ?? Color.white;
            so.FindProperty("<MaxHealth>k__BackingField").floatValue = maxHealth;
            so.FindProperty("<MaxShield>k__BackingField").floatValue = maxShield;
            so.FindProperty("<MoveSpeed>k__BackingField").floatValue = moveSpeed;
            so.FindProperty("<DashCooldown>k__BackingField").floatValue = dashCooldown;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignRosterDefaults(HeroRosterDefinition roster, params HeroDefinition[] heroes)
        {
            SerializedObject so = new(roster);
            SerializedProperty items = so.FindProperty("<Heroes>k__BackingField");
            items.ClearArray();

            for (int i = 0; i < heroes.Length; i++)
            {
                items.InsertArrayElementAtIndex(i);
                items.GetArrayElementAtIndex(i).objectReferenceValue = heroes[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignWeaponDefaults(WeaponDefinition weapon, string id, string displayName, string description, WeaponRarity rarity, RoomTier minRoomTier, RoomTier maxRoomTier, int shopPrice, int chestWeight, Color accentColor, float damage, float fireRate, float projectileSpeed)
        {
            SerializedObject so = new(weapon);
            so.FindProperty("<WeaponId>k__BackingField").stringValue = id;
            so.FindProperty("<DisplayName>k__BackingField").stringValue = displayName;
            so.FindProperty("<Description>k__BackingField").stringValue = description;
            so.FindProperty("<Rarity>k__BackingField").enumValueIndex = (int)rarity;
            so.FindProperty("<MinRoomTier>k__BackingField").enumValueIndex = (int)minRoomTier;
            so.FindProperty("<MaxRoomTier>k__BackingField").enumValueIndex = (int)maxRoomTier;
            so.FindProperty("<ShopPrice>k__BackingField").intValue = shopPrice;
            so.FindProperty("<ChestWeight>k__BackingField").intValue = chestWeight;
            so.FindProperty("<AccentColor>k__BackingField").colorValue = accentColor;
            so.FindProperty("<Damage>k__BackingField").floatValue = damage;
            so.FindProperty("<FireRate>k__BackingField").floatValue = fireRate;
            so.FindProperty("<ProjectileSpeed>k__BackingField").floatValue = projectileSpeed;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignEnemyDefaults(
            EnemyDefinition enemy,
            string id,
            float maxHealth,
            float moveSpeed,
            float attackRange,
            float attackRate,
            float contactDamage,
            bool usesProjectile,
            float projectileSpeed)
        {
            SerializedObject so = new(enemy);
            so.FindProperty("<EnemyId>k__BackingField").stringValue = id;
            so.FindProperty("<MaxHealth>k__BackingField").floatValue = maxHealth;
            so.FindProperty("<MoveSpeed>k__BackingField").floatValue = moveSpeed;
            so.FindProperty("<AttackRange>k__BackingField").floatValue = attackRange;
            so.FindProperty("<AttackRate>k__BackingField").floatValue = attackRate;
            so.FindProperty("<ContactDamage>k__BackingField").floatValue = contactDamage;
            so.FindProperty("<UsesProjectile>k__BackingField").boolValue = usesProjectile;
            so.FindProperty("<ProjectileSpeed>k__BackingField").floatValue = projectileSpeed;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignRewardDefaults(RewardDefinition reward)
        {
            SerializedObject so = new(reward);
            so.FindProperty("<RewardId>k__BackingField").stringValue = "reward_default";
            so.FindProperty("<DisplayName>k__BackingField").stringValue = "Core Cache";
            so.FindProperty("<Description>k__BackingField").stringValue = "Placeholder reward for room clear.";
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignEncounterDefaults(EncounterDefinition encounter, EnemyDefinition enemy)
        {
            SerializedObject so = new(encounter);
            so.FindProperty("<EncounterId>k__BackingField").stringValue = "encounter_default";
            SerializedProperty waves = so.FindProperty("<Waves>k__BackingField");
            waves.ClearArray();
            waves.InsertArrayElementAtIndex(0);
            SerializedProperty wave = waves.GetArrayElementAtIndex(0);
            wave.FindPropertyRelative("Enemy").objectReferenceValue = enemy;
            wave.FindPropertyRelative("Count").intValue = 3;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignViewerActionDefaults(
            ViewerActionDefinition action,
            string actionId,
            string displayName,
            string description,
            Viewer.ViewerActionCategory category,
            int price,
            float cooldown,
            int budgetCost,
            string targetId,
            float healAmount,
            Color accentColor)
        {
            SerializedObject so = new(action);
            so.FindProperty("<ActionId>k__BackingField").stringValue = actionId;
            so.FindProperty("<DisplayName>k__BackingField").stringValue = displayName;
            so.FindProperty("<Description>k__BackingField").stringValue = description;
            so.FindProperty("<Category>k__BackingField").enumValueIndex = (int)category;
            so.FindProperty("<Price>k__BackingField").intValue = price;
            so.FindProperty("<Cooldown>k__BackingField").floatValue = cooldown;
            so.FindProperty("<BudgetCost>k__BackingField").intValue = budgetCost;
            so.FindProperty("<TargetId>k__BackingField").stringValue = targetId;
            so.FindProperty("<HealAmount>k__BackingField").floatValue = healAmount;
            so.FindProperty("<AccentColor>k__BackingField").colorValue = accentColor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignStoreDefaults(
            ViewerStoreCatalog storeCatalog,
            ViewerActionDefinition spawnWeak,
            ViewerActionDefinition spawnElite,
            ViewerActionDefinition dropHeal,
            ViewerActionDefinition dropWeapon)
        {
            SerializedObject so = new(storeCatalog);
            SerializedProperty actions = so.FindProperty("<Actions>k__BackingField");
            actions.ClearArray();

            ViewerActionDefinition[] actionArray = { spawnWeak, spawnElite, dropHeal, dropWeapon };
            for (int i = 0; i < actionArray.Length; i++)
            {
                actions.InsertArrayElementAtIndex(i);
                actions.GetArrayElementAtIndex(i).objectReferenceValue = actionArray[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignEconomyDefaults(ViewerEconomyConfig config)
        {
            SerializedObject so = new(config);
            so.FindProperty("<JoinReward>k__BackingField").intValue = 120;
            so.FindProperty("<WatchRewardPerMinute>k__BackingField").intValue = 20;
            so.FindProperty("<RoomClearReward>k__BackingField").intValue = 25;
            so.FindProperty("<BossClearReward>k__BackingField").intValue = 75;
            so.FindProperty("<MaxQueuedCommands>k__BackingField").intValue = 6;
            so.FindProperty("<DefaultRoomBudget>k__BackingField").intValue = 6;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateHeroPrefab(string prefabName, Color eyeColor, Color hairColor, string weaponSpritePath, string backSpritePath, Color clothTint)
        {
            string prefabPath = $"Assets/Game/Prefabs/Heroes/{prefabName}.prefab";
            GameObject sourcePrefab = LoadBaseSpumUnitPrefab();
            if (sourcePrefab == null)
            {
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab);
            instance.name = prefabName;
            ConfigureHeroPrefab(instance, eyeColor, hairColor, weaponSpritePath, backSpritePath, clothTint);
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private static void ConfigureHeroPrefab(GameObject heroObject, Color eyeColor, Color hairColor, string weaponSpritePath, string backSpritePath, Color clothTint)
        {
            SPUM_Prefabs prefabs = heroObject.GetComponent<SPUM_Prefabs>();
            if (prefabs == null || prefabs._spriteOBj == null)
            {
                return;
            }

            SPUM_SpriteList spriteList = prefabs._spriteOBj;
            ApplyColor(spriteList._eyeList, eyeColor);
            ApplyColor(spriteList._hairList, hairColor, 0, 3);
            ApplyColor(spriteList._clothList, clothTint);
            ApplyColor(spriteList._armorList, clothTint);

            ClearSprite(spriteList._weaponList, 0, 1, 2, 3);
            ClearSprite(spriteList._backList, 0);

            Sprite weaponSprite = LoadSpriteAt(weaponSpritePath);
            if (weaponSprite != null)
            {
                SetSprite(spriteList._weaponList, weaponSprite, 0);
            }

            Sprite backSprite = LoadSpriteAt(backSpritePath);
            if (backSprite != null)
            {
                SetSprite(spriteList._backList, backSprite, 0);
            }
        }

        private static void ApplyColor(System.Collections.Generic.IReadOnlyList<SpriteRenderer> renderers, Color color, params int[] indexes)
        {
            if (renderers == null)
            {
                return;
            }

            if (indexes == null || indexes.Length == 0)
            {
                for (int i = 0; i < renderers.Count; i++)
                {
                    if (renderers[i] != null)
                    {
                        renderers[i].color = color;
                    }
                }

                return;
            }

            for (int i = 0; i < indexes.Length; i++)
            {
                int index = indexes[i];
                if (index >= 0 && index < renderers.Count && renderers[index] != null)
                {
                    renderers[index].color = color;
                }
            }
        }

        private static void SetSprite(System.Collections.Generic.IReadOnlyList<SpriteRenderer> renderers, Sprite sprite, params int[] indexes)
        {
            if (renderers == null || sprite == null || indexes == null)
            {
                return;
            }

            for (int i = 0; i < indexes.Length; i++)
            {
                int index = indexes[i];
                if (index >= 0 && index < renderers.Count && renderers[index] != null)
                {
                    renderers[index].sprite = sprite;
                }
            }
        }

        private static void ClearSprite(System.Collections.Generic.IReadOnlyList<SpriteRenderer> renderers, params int[] indexes)
        {
            if (renderers == null || indexes == null)
            {
                return;
            }

            for (int i = 0; i < indexes.Length; i++)
            {
                int index = indexes[i];
                if (index >= 0 && index < renderers.Count && renderers[index] != null)
                {
                    renderers[index].sprite = null;
                }
            }
        }

        private static Sprite LoadSpriteAt(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite)
                {
                    return sprite;
                }
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static GameObject LoadBaseSpumUnitPrefab()
        {
            GameObject unitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/SPUM/SPUM_Units/Unit0.prefab");
            if (unitPrefab != null)
            {
                return unitPrefab;
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SPUM/Prefab/UnitSave.prefab");
        }
    }
}
#endif
