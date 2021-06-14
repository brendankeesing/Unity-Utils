using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(LinePathObjectPlacer))]
public class LinePathObjectPlacerEditor : Editor
{
    bool _autoUpdate = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Separator();
        _autoUpdate = EditorGUILayout.Toggle("Auto Update", _autoUpdate);
        if (_autoUpdate || GUILayout.Button("Force Update"))
        {
            foreach (Object t in targets)
            {
                LinePathObjectPlacer bop = (LinePathObjectPlacer)t;
                if (bop.isActiveAndEnabled)
                    bop.Apply();
            }
        }
    }
}
