#if UNITY_EDITOR
using SoulForge.Bootstrap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoulForge.Editor
{
    public static class SoulForgeBoosterSceneBuilder
    {
        [MenuItem("Tools/SoulForge/Create Booster Scene")]
        public static void CreateBoosterScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Booster";

            GameObject routerRoot = new("BoosterBootstrap");
            routerRoot.AddComponent<BoosterSceneRouter>();

            string scenePath = "Assets/Game/Scenes/Booster.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AddSceneToBuildSettings();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath));
            Debug.Log("Booster startup scene created.");
        }

        public static void AddSceneToBuildSettings()
        {
            const string boosterScenePath = "Assets/Game/Scenes/Booster.unity";
            const string hubScenePath = "Assets/Game/Scenes/Hub.unity";
            const string runScenePath = "Assets/Game/Scenes/Floor_01.unity";
            const string floor2ScenePath = "Assets/Game/Scenes/Floor_02.unity";
            const string floor3ScenePath = "Assets/Game/Scenes/Floor_03.unity";
            const string viewerScenePath = "Assets/Game/Scenes/Viewer_Client.unity";

            var orderedScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

            AddIfExists(orderedScenes, boosterScenePath);
            AddIfExists(orderedScenes, hubScenePath);
            AddIfExists(orderedScenes, runScenePath);
            AddIfExists(orderedScenes, floor2ScenePath);
            AddIfExists(orderedScenes, floor3ScenePath);
            AddIfExists(orderedScenes, viewerScenePath);

            EditorBuildSettings.scenes = orderedScenes.ToArray();
        }

        private static void AddIfExists(System.Collections.Generic.ICollection<EditorBuildSettingsScene> scenes, string scenePath)
        {
            if (System.IO.File.Exists(scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }
        }
    }
}
#endif
