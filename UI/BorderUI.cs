using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(CanvasRenderer))]
public class BorderUI : MaskableGraphic
{
    public float thickness = 10;
    [Range(-1, 1)]
    public float pivot = 1;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        vh.Clear();

        Rect rect = GetComponent<RectTransform>().rect;

        thickness = Mathf.Max(thickness, 0);
        float max = Mathf.LerpUnclamped(thickness * 0.5f, thickness, pivot);
        float min = max - thickness;

        // left
        vh.AddVert(new Vector3(rect.xMin - min, rect.yMin - min, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMin - max, rect.yMin - max, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMin - max, rect.yMax + max, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMin - min, rect.yMax + min, 0), color, Vector2.zero);
        
        // right
        vh.AddVert(new Vector3(rect.xMax + min, rect.yMax + min, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax + max, rect.yMax + max, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax + max, rect.yMin - max, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax + min, rect.yMin - min, 0), color, Vector2.zero);
        
        // bottom
        vh.AddVert(new Vector3(rect.xMax + min, rect.yMin - min, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax + max, rect.yMin - max, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMin - max, rect.yMin - max, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMin - min, rect.yMin - min, 0), color, Vector2.zero);

        // top
        vh.AddVert(new Vector3(rect.xMin - min, rect.yMax + min, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMin - max, rect.yMax + max, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax + max, rect.yMax + max, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax + min, rect.yMax + min, 0), color, Vector2.zero);

        for (int t = 0; t < 4; ++t)
        {
            int i = t * 4;
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }
    }
}
