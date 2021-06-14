using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class BarChartUI : MaskableGraphic
{
    public float minValue = 0;
    public float maxValue = 10;
    public float[] values;
    [Range(0, 1)]
    public float gap = 0;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (values == null || values.Length < 1)
            return;

        base.OnPopulateMesh(vh);
        vh.Clear();

        Rect rect = GetComponent<RectTransform>().rect;

        float valuewidth = rect.width / values.Length;
        float gapwidth = valuewidth * gap * 0.5f;
        for (int i = 0; i < values.Length; ++i)
        {
            float xmin = rect.xMin + valuewidth * i + gapwidth;
            float xmax = xmin + valuewidth - gapwidth * 2;
            float ymax = Mathf.LerpUnclamped(rect.yMin, rect.yMax, Mathf.InverseLerp(minValue, maxValue, values[i]));

            vh.AddVert(new Vector3(xmin, rect.yMin, 0), color, Vector2.zero);
            vh.AddVert(new Vector3(xmax, rect.yMin, 0), color, Vector2.zero);
            vh.AddVert(new Vector3(xmax, ymax, 0), color, Vector2.zero);
            vh.AddVert(new Vector3(xmin, ymax, 0), color, Vector2.zero);
        }

        for (int t = 0; t < values.Length; ++t)
        {
            int i = t * 4;
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }
    }
}
