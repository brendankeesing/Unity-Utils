using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LinePath))]
[CanEditMultipleObjects]
public class LinePathEditor : Editor
{
    int selectedPoint = -1;
    Vector3 lastPointPos;

    LayerMask raycastLayerMask = -1;

    bool CheckValidPath(LinePath path)
    {
        if (path.pointsPerSegment < 1)
            path.pointsPerSegment = 1;

        if (path.points == null)
            path.points = new List<Vector3>();

        if (selectedPoint < -1 || selectedPoint >= path.points.Count)
            selectedPoint = -1;

        return path.cachedPoints != null && path.cachedPoints.Count > 1;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (targets.Length == 1)
        {
            LinePath path = (LinePath)target;
            bool isvalidpath = CheckValidPath(path);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("C - Adds point to end");
            sb.AppendLine("X - Start line again at new point");
            sb.AppendLine("I - Insert point at");
            sb.AppendLine("M - Move selected point");

            sb.AppendLine("\nSTATS");
            if (isvalidpath)
            {
                sb.AppendLine("Total Distance: " + path.totalDistance.ToString());
                sb.AppendLine("Cached Points: " + path.cachedPoints.Count.ToString());
            }
            sb.Append("Selected: " + selectedPoint.ToString());

            EditorGUILayout.HelpBox(sb.ToString(), MessageType.None);

            raycastLayerMask = UnityEditorUtils.LayerMaskField("Raycast Layers", raycastLayerMask);

            if (GUILayout.Button("Reverse"))
                path.points.Reverse();
        }

        if (GUI.changed)
        {
            foreach (Object target in targets)
                ((LinePath)target).Rebuild();
            EditorUtility.SetDirty(target);
        }
    }

    bool Raycast(Ray ray, out RaycastHit hit)
    {
        List<RaycastHit> hits = new List<RaycastHit>(Physics.RaycastAll(ray, 9999, raycastLayerMask));
        hits.RemoveAll(h => h.collider.isTrigger);
        hits.Sort((a, b) => a.distance < b.distance ? -1 : 1);
        if (hits.Count == 0)
        {
            hit = new RaycastHit();
            return false;
        }

        hit = hits[0];
        return true;
    }

    void OnSceneGUI()
    {
        LinePath path = (LinePath)target;
        bool isvalidpath = CheckValidPath(path);

        if (Event.current.type == EventType.MouseDown && selectedPoint != -1)
        {
            // record position
            lastPointPos = path.points[selectedPoint];
        }
        if (Event.current.type == EventType.MouseUp && selectedPoint != -1 && path.points[selectedPoint] != lastPointPos)
        {
            // put them back, register undo
            Vector3 newPos = path.points[selectedPoint];
            path.points[selectedPoint] = lastPointPos;
            Undo.RegisterCompleteObjectUndo(path, "Move Bezier Point");
            path.points[selectedPoint] = newPos;
            EditorUtility.SetDirty(path);
        }

        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.C)
            {
                // add point to end
                Ray ray = GetViewportRay(Event.current.mousePosition);
                if (Raycast(ray, out RaycastHit hit))
                {
                    Undo.RegisterCompleteObjectUndo(path, "Append Bezier Point");
                    path.points.Add(hit.point);
                    EditorUtility.SetDirty(path);
                    selectedPoint = path.points.Count - 1;
                    path.Rebuild();
                }
            }
            else if (Event.current.keyCode == KeyCode.I && isvalidpath)
            {
                // insert point
                Ray ray = GetViewportRay(Event.current.mousePosition);
                if (Raycast(ray, out RaycastHit hit))
                {
                    int segment = path.GetNearestPointAtPosition(hit.point).segmentIndex + 1;

                    Undo.RegisterCompleteObjectUndo(path, "Insert Bezier Point");
                    path.points.Insert(segment, hit.point);
                    EditorUtility.SetDirty(path);
                    selectedPoint = segment;
                    path.Rebuild();
                }
            }
            else if (Event.current.keyCode == KeyCode.X)
            {
                // clear and add point
                Ray ray = GetViewportRay(Event.current.mousePosition);
                if (Raycast(ray, out RaycastHit hit))
                {
                    Undo.RegisterCompleteObjectUndo(path, "Insert Bezier Point");
                    path.points.Clear();
                    path.points.Add(hit.point);
                    EditorUtility.SetDirty(path);
                    selectedPoint = 0;
                    path.Rebuild();
                }
            }
            else if (selectedPoint != -1 && Event.current.keyCode == KeyCode.Backspace)
            {
                // remove selected point
                Undo.RegisterCompleteObjectUndo(path, "Remove Bezier Point");
                path.points.RemoveAt(selectedPoint);
                EditorUtility.SetDirty(path);
                selectedPoint = -1;
                path.Rebuild();
            }
            if (selectedPoint != -1 && Event.current.keyCode == KeyCode.M)
            {
                // move point
                Ray ray = GetViewportRay(Event.current.mousePosition);
                if (Raycast(ray, out RaycastHit hit))
                {
                    path.points[selectedPoint] = hit.point;
                    EditorUtility.SetDirty(path);
                    path.Rebuild();
                }
            }
        }

        // disable transform tool if selected
        if (Selection.activeGameObject == path.gameObject)
        {
            Tools.current = Tool.None;
            if (selectedPoint == -1 && path.points.Count > 0)
                selectedPoint = 0;
        }

        // draw the handles for the points
        Handles.color = new Color(1, 1, 1, 0.5f);
        for (int i = 0; i < path.points.Count; ++i)
        {
            if (i == selectedPoint)
            {
                Handles.Button(path.points[i], Quaternion.identity, path.displayPointSize, path.displayPointSize, Handles.CubeHandleCap);
                Handles.CubeHandleCap(0, path.points[i], Quaternion.identity, path.displayPointSize, EventType.MouseDown);
                path.points[i] = Handles.PositionHandle(path.points[i], Quaternion.identity);
            }
            else
            {
                // draw button
                if (Handles.Button(path.points[i], Quaternion.identity, path.displayPointSize, path.displayPointSize, Handles.CubeHandleCap))
                    selectedPoint = i;
            }
        }

        // draw the lines
        if (isvalidpath)
        {
            Handles.color = path.displayLineColor;
            for (int i = 0; i < path.fractionCount; i++)
                Handles.DrawLine(path.cachedPoints[i].position, path.cachedPoints[(i + 1) % path.cachedPoints.Count].position);
        }

        if (GUI.changed)
        {
            path.Rebuild();
            EditorUtility.SetDirty(target);
        }
    }

    static Ray GetViewportRay(Vector2 mousepos)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition); ;

        Vector3 viewportpos = Camera.current.WorldToViewportPoint(ray.origin);
        viewportpos.z = 0;
        ray.origin = Camera.current.ViewportToWorldPoint(viewportpos);
        return ray;
        //return HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
    }

    [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
    static void DrawPathGizmos(LinePath path, GizmoType gizmoType)
    {
        // don't draw if selected
        if (Selection.activeGameObject == path.gameObject || path.cachedPoints == null)
            return;

        for (int i = 0; i < path.points.Count; ++i)
        {
            // draw point
            Gizmos.color = path.displayLineColor;
            Gizmos.DrawCube(path.points[i], Vector3.one * path.displayPointSize);
        }

        Gizmos.color = path.displayLineColor;
        for (int i = 0; i < path.fractionCount; i++)
            Gizmos.DrawLine(path.cachedPoints[i].position, path.cachedPoints[(i + 1) % path.cachedPoints.Count].position);
    }
}
