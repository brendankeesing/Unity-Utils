using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(Decal))]
[CanEditMultipleObjects]
public class DecalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
            
        if (GUILayout.Button("Build"))
            GUI.changed = true;

        if (targets.Length == 1)
            EditorGUILayout.HelpBox("Left Ctrl + Left Mouse Button - put decal on surface", MessageType.Info);

        if (GUI.changed)
        {
            foreach (Object t in targets)
                BuildAndSetDirty((Decal)t);
        }
    }

    void OnSceneGUI()
    {
        Decal decal = (Decal)target;

        if (Event.current.control)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if (Event.current.control && Event.current.type == EventType.MouseDown)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 50))
            {
                decal.transform.position = hit.point;
                decal.transform.forward = -hit.normal;
            }
        }

        if (decal.transform.hasChanged)
        {
            decal.transform.hasChanged = false;
            BuildAndSetDirty(decal);
        }
    }

    static void BuildAndSetDirty(Decal decal)
    {
        decal.Build();
        if (decal.gameObject.scene.IsValid())
        {
            if (!EditorApplication.isPlaying)
                EditorSceneManager.MarkSceneDirty(decal.gameObject.scene);
        }
        else
        {
            EditorUtility.SetDirty(decal.gameObject);
        }
    }

    [MenuItem("Custom/Rebuild All Decals")]
    public static void RemoveAllLocalProfilesEditor()
    {
        Decal[] decals = FindObjectsOfType<Decal>();
        foreach (Decal decal in decals)
            BuildAndSetDirty(decal);
    }
}
