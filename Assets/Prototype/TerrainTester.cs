using UnityEditor;
using UnityEngine;

public class TerrainTester : MonoBehaviour
{
    [SerializeField] public WorldGenerator worldGenerator;
    [SerializeField] public TerrainSettings settings;

    void Start()
    {
        
    }
}

[CustomEditor(typeof(TerrainTester))]
public class TerrainTesterUI : Editor
{
    public float progress;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TerrainTester tester = target as TerrainTester;

        if (GUILayout.Button("Generate terrain"))
            tester.worldGenerator.GenerateTerrain(tester.settings, ref progress);


    }

}
