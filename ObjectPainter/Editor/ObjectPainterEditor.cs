using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectPainter))]
public class ObjectPainterEditor : Editor
{
    ObjectPainter _painter { get { return (ObjectPainter)target; } }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.HelpBox("C - Paint", MessageType.Info);

        if (_painter.prefab != null && GUILayout.Button("Modify"))
        {
            foreach (Transform child in _painter.transform)
            {
                if (PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject) == _painter.prefab)
                    Modify(child);
            }
        }
    }

    void OnSceneGUI()
    {
        if (_painter.prefab != null && Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.C)
                Paint();
        }
    }

    void Paint()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _painter.layerMask, QueryTriggerInteraction.Ignore))
            return;
        
        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(_painter.prefab);
        obj.name = _painter.prefab.name;
        obj.transform.parent = _painter.transform;
        obj.transform.position = hit.point;
        Modify(obj.transform);

        Undo.RegisterCreatedObjectUndo(obj, "Painted Object");
        EditorUtility.SetDirty(obj);
    }

    void Modify(Transform obj)
    {
        obj.rotation = Quaternion.Euler(Random.Range(-_painter.leanVariation, _painter.leanVariation), Random.Range(0, 360), Random.Range(-_painter.leanVariation, _painter.leanVariation));

        float horizontal = Random.Range(_painter.minHorizontalScale, _painter.maxHorizontalScale);
        obj.localScale = new Vector3(horizontal, Random.Range(_painter.minVerticalScale, _painter.maxVerticalScale), horizontal) * Random.Range(_painter.minScale, _painter.maxScale);
    }
}
