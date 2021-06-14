using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class RingUI : MaskableGraphic
{
    public Texture texture;
    public override Texture mainTexture
    {
        get
        {
            return texture == null ? s_WhiteTexture : texture;
        }
    }

    public int circleSegments = 16;
    public float thickness = 0.1f;
    [Range(0, 1)]
    public float thicknessCenter = 0.5f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        vh.Clear();

        Vector2 size = rectTransform.rect.size;
        Vector2 radius = rectTransform.rect.size * 0.5f;
        float radius1thickness = 1 - thickness * thicknessCenter;
        Vector2 radius1 = radius * radius1thickness;
        Vector2 radius2 = radius * (radius1thickness + thickness);
        Vector2 pivot = rectTransform.pivot;

        Vector2 min = -pivot * size;
        Vector2 max = size + min;
        Vector2 center = (min + max) * 0.5f;

        // vertices
        float anglepersegment = 360f / circleSegments;
        for (int i = 0; i < circleSegments + 1; ++i)
        {
            float angle = i * anglepersegment * Mathf.Deg2Rad;
            float uvx = (float)i / circleSegments;

            Vector3 pos1 = new Vector3(Mathf.Sin(angle) * radius1.x + center.x, Mathf.Cos(angle) * radius1.y + center.y);
            vh.AddVert(pos1, color, new Vector2(uvx, 0));

            Vector3 pos2 = new Vector3(Mathf.Sin(angle) * radius2.x + center.x, Mathf.Cos(angle) * radius2.y + center.y);
            vh.AddVert(pos2, color, new Vector2(uvx, 1));
        }

        // indices
        for (int i = 0; i < circleSegments; ++i)
        {
            int idx = i * 2;
            vh.AddTriangle(idx + 0, idx + 2, idx + 3);
            vh.AddTriangle(idx + 0, idx + 3, idx + 1);
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetVerticesDirty();
        SetMaterialDirty();
    }
}
