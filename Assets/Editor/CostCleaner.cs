using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// Editor utility to find GameObjects named "Cost" (case-insensitive) and ensure they have
// four children in order: Timber, Stone, Metal, Cash. Each child will have an Image
// and a child "Label" with a TextMeshProUGUI text component. The images will be set
// from sprites found under Assets/Art/UI/ResourceIcons (names: Wood, Stone, Metal, Cash).
public static class CostCleaner
{
    private static readonly string[] ExpectedNames = { "Timber", "Stone", "Metal", "Cash" };
    private static readonly string[] SpriteLookupNames = { "Wood", "Stone", "Metal", "Cash" };

    [MenuItem("Tools/Cost Cleaner/Setup Costs in Scene")]
    public static void SetupCostsInScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.isLoaded)
        {
            EditorUtility.DisplayDialog("CostCleaner", "No active scene loaded.", "OK");
            return;
        }

        var rootGOs = scene.GetRootGameObjects();
        var found = 0;
        Debug.Log("found: " + found);
        foreach (var root in rootGOs)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(t.name, "Cost", System.StringComparison.OrdinalIgnoreCase))
                {

                    Debug.Log("Processsing: " + t.name);
                    ProcessCostObject(t.gameObject);
                    found++;
                }
            }
        }

        if (found > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorUtility.DisplayDialog("CostCleaner", $"Processed {found} Cost object(s).", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("CostCleaner", "No Cost objects found in scene.", "OK");
        }
    }

    private static void ProcessCostObject(GameObject costGO)
    {
        Undo.RegisterCompleteObjectUndo(costGO.transform, "CostCleaner: Update Cost object");

        // Collect existing direct children
        var children = costGO.transform.Cast<Transform>().ToList();
        var matched = new HashSet<Transform>();

        // Ensure expected children exist, try to match by name first, then by presence of Image/TMP components
        for (int i = 0; i < ExpectedNames.Length; i++)
        {
            var childName = ExpectedNames[i];
            Transform match = null;

            // 1) exact name match (preferred)
            match = children.FirstOrDefault(c => !matched.Contains(c) && c.name == childName);

            // 2) any child that already contains an Image or TMP (misnamed resource child)
            if (match == null)
            {
                match = children.FirstOrDefault(c => !matched.Contains(c) &&
                    (c.GetComponentInChildren<Image>(true) != null || c.GetComponentInChildren<TMP_Text>(true) != null));
            }

            // 3) create new child
            if (match == null)
            {
                var newChildGO = new GameObject(childName, typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(newChildGO, "CostCleaner: Create child");
                newChildGO.transform.SetParent(costGO.transform, false);
                match = newChildGO.transform;
            }
            else if (match.name != childName)
            {
                // rename misnamed child to expected name
                Undo.RecordObject(match, "CostCleaner: Rename child");
                match.name = childName;
            }

            matched.Add(match);

            // ensure sibling index
            if (match.GetSiblingIndex() != i)
                match.SetSiblingIndex(i);

            EnsureImageAndLabel(match.gameObject, SpriteLookupNames[i], ":0");
        }

        // Remove any extra children that were not matched
        var toRemove = children.Where(c => !matched.Contains(c)).ToList();
        foreach (var extra in toRemove)
        {
            Undo.DestroyObjectImmediate(extra.gameObject);
        }
    }

    private static void EnsureImageAndLabel(GameObject childGO, string spriteName, string labelText)
    {
        // Find existing Image on this child or its descendants
        var img = childGO.GetComponentInChildren<Image>(true);
        if (img == null || img.gameObject == null)
        {
            // add Image directly to child if none exists
            img = Undo.AddComponent<Image>(childGO);
        }

        var sprite = LoadSpriteByName(spriteName);
        if (sprite != null && img.sprite != sprite)
        {
            img.sprite = sprite;
            EditorUtility.SetDirty(img);
        }

        // Find existing TMP_Text on this child or its descendants (label might be misnamed)
        var tmp = childGO.GetComponentInChildren<TMP_Text>(true);
        Transform labelTransform = null;
        if (tmp != null)
        {
            labelTransform = tmp.transform;
        }
        else
        {
            // create a Label child if no TMP found
            var labelGO = new GameObject("Label", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(labelGO, "CostCleaner: Create Label");
            labelGO.transform.SetParent(childGO.transform, false);
            labelTransform = labelGO.transform;
            tmp = Undo.AddComponent<TextMeshProUGUI>(labelTransform.gameObject);
        }

        if (tmp.text != labelText)
        {
            tmp.text = labelText;
            EditorUtility.SetDirty(tmp);
        }
    }

    private static Sprite LoadSpriteByName(string name)
    {
        // Search for sprites in the project by name
        var guids = AssetDatabase.FindAssets(name + " t:Sprite");
        if (guids != null && guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        return null;
    }
}

