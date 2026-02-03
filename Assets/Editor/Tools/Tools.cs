using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Tools : EditorWindow
{
    private static SceneAsset cachedScene;
    private static bool overrideEnabled;

    void OnGUI()
    {
        GUILayout.Label("Play Mode Start Scene", EditorStyles.boldLabel);

        // Toggle enable / disable
        bool newOverrideEnabled = EditorGUILayout.Toggle("Override Start Scene", overrideEnabled);

        if (newOverrideEnabled != overrideEnabled)
        {
            overrideEnabled = newOverrideEnabled;

            if (!overrideEnabled)
            {
                // Disable override → restore default behavior
                EditorSceneManager.playModeStartScene = null;
            }
            else
            {
                // Enable override → restore cached scene if available
                EditorSceneManager.playModeStartScene = cachedScene;
            }
        }

        using (new EditorGUI.DisabledScope(!overrideEnabled))
        {
            cachedScene = (SceneAsset)EditorGUILayout.ObjectField(
                new GUIContent("Start Scene"),
                cachedScene,
                typeof(SceneAsset),
                false
            );

            if (cachedScene != null)
                EditorSceneManager.playModeStartScene = cachedScene;

            var scenePath = "Assets/Scenes/MainMenu.unity";
            if (GUILayout.Button("Set start Scene: " + scenePath))
                SetPlayModeStartScene(scenePath);
        }
    }

    void SetPlayModeStartScene(string scenePath)
    {
        SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        if (scene != null)
        {
            cachedScene = scene;
            if (overrideEnabled)
                EditorSceneManager.playModeStartScene = scene;
        }
        else
        {
            Debug.LogWarning("Could not find Scene " + scenePath);
        }
    }

    [MenuItem("Tools/Play Mode Scene Manager")]
    static void Open()
    {
        GetWindow<Tools>("Play Mode Scene Manager");
    }
}