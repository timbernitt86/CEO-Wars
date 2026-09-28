#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CEOWars.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        private const string ScenePath = "Assets/CEO_Wars/Scenes/Main.unity";
        private const string SetupKey = "CEOWars.ProjectSetup.v1";

        static ProjectSetup()
        {
            EditorApplication.delayCall += EnsureProjectReady;
        }

        [MenuItem("CEO Wars/Setup Project")]
        public static void EnsureProjectReady()
        {
            if (!File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            PlayerSettings.productName = "CEO Wars";
            PlayerSettings.companyName = "CEO Wars Studio";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.ceowars.game");
            PlayerSettings.bundleVersion = "0.1.0";

            if (!SessionState.GetBool(SetupKey, false))
            {
                SessionState.SetBool(SetupKey, true);
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                Debug.Log("CEO Wars project is ready. Press Play.");
            }
        }
    }
}
#endif
