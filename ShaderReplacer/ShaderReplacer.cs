using UnityEngine;
using UnityEditor;

public class ShaderReplacer : EditorWindow
{
    Shader _old;
    Shader _new;

    void OnGUI()
    {
        _old = (Shader)EditorGUILayout.ObjectField("Old", _old, typeof(Shader), false);
        _new = (Shader)EditorGUILayout.ObjectField("New", _new, typeof(Shader), false);
        if (GUILayout.Button("Replace"))
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Material"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material.shader != _old)
                    continue;

                material.shader = _new;
                EditorUtility.SetDirty(material);
                Debug.LogWarning("Replaced shader on material: " + path, material);
            }
        }
    }

    [MenuItem("Custom/Shader Replacer")]
    public static void OpenWindow()
    {
        CreateWindow<ShaderReplacer>().Show();
    }
}
