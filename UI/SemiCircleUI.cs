using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SemiCircleUI : MaskableGraphic
{
    public float minAngle = 0;
    public float maxAngle = 180;
    public int circleSegments = 16;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        vh.Clear();

        Vector2 size = rectTransform.rect.size;
        Vector2 radius = rectTransform.rect.size * 0.5f;
        Vector2 pivot = rectTransform.pivot;

        Vector2 min = -pivot * size;
        Vector2 max = size + min;
        Vector2 center = (min + max) * 0.5f;

        float minangle = Mathf.Min(minAngle, maxAngle);
        float maxangle = Mathf.Max(minAngle, maxAngle);
        float difference = Mathf.Min(maxangle - minangle, 360);
        int circlesegments = Mathf.RoundToInt(circleSegments * difference / 360);

        // vertices
        float anglepersegment = difference / circlesegments;
        vh.AddVert(center, color, Vector2.zero);
        for (int i = 0; i < circlesegments + 1; ++i)
        {
            float angle = (minangle + i * anglepersegment) * Mathf.Deg2Rad;
            vh.AddVert(new Vector3(Mathf.Sin(angle) * radius.x + center.x, Mathf.Cos(angle) * radius.y + center.y), color, Vector2.zero);
        }

        // indices
        for (int i = 0; i < circlesegments; ++i)
            vh.AddTriangle(0, i + 1, i + 2);
    }
}
