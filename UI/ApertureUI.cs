using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ApertureUI : MaskableGraphic
{
    [Range(0, 1)]
    public float openAmount = 0.5f;
    public int apertureSegments = 6;
    public int circleSegments = 4;
    [Range(0, 360)]
    public float angleOffset = 0;
    [Range(0, 1)]
    public float gap = 0.0f;
    public int fillSegments = 99999;

    Vector2 AngleToPoint(Vector2 min, Vector2 max, float angle)
    {
        angle += angleOffset * Mathf.Deg2Rad;
        Vector2 pos = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        pos.x = Mathf.LerpUnclamped(min.x, max.x, pos.x * 0.5f + 0.5f);
        pos.y = Mathf.LerpUnclamped(min.y, max.y, pos.y * 0.5f + 0.5f);
        return pos;
    }

    Vector2 CalculateTarget(Vector2 min, Vector2 max, Vector2 center, Quaternion rotation, float startangle, float gapspace)
    {
        Vector2 pivotpoint = AngleToPoint(min, max, startangle);
        Vector2 target = (Vector2)(rotation * (center - pivotpoint)) + center;
        target = Vector2.LerpUnclamped(center, target, openAmount);

        // add gap
        Vector2 dir = pivotpoint - target;
        float dist = dir.magnitude;
        target = target + dir * (gapspace * 0.5f / dist);
        
        return target;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        vh.Clear();

        openAmount = Mathf.Clamp01(openAmount);
        gap = Mathf.Clamp01(gap);
        if (apertureSegments < 4)
            apertureSegments = 4;
        if (circleSegments < 1)
            circleSegments = 1;

        Vector2 size = rectTransform.rect.size;
        Vector2 pivot = rectTransform.pivot;

        Vector2 min = -pivot * size;
        Vector2 max = size + min;
        Vector2 center = (min + max) * 0.5f;

        float angleperaperturesegment = 2.0f * Mathf.PI / apertureSegments;
        float anglepersegment = (angleperaperturesegment - angleperaperturesegment * gap) / circleSegments;
        Quaternion rotation = Quaternion.Euler(0, 0, openAmount * -90 - 90);

        float gapspace = Vector2.Distance(AngleToPoint(min, max,   0), AngleToPoint(min, max, gap * angleperaperturesegment));

        int segments = Mathf.Min(apertureSegments, fillSegments);
        for (int s = 0; s < segments; ++s)
        {
            float startangle = s * angleperaperturesegment;
            float endangle = startangle + angleperaperturesegment;

            // calculate previous target point
            float previousstartangle = startangle - angleperaperturesegment;
            Vector2 previoustarget = CalculateTarget(min, max, center, rotation, previousstartangle, gapspace);

            // calculate current target point
            Vector2 startpoint = AngleToPoint(min, max, startangle);
            Vector2 endpoint = AngleToPoint(min, max, endangle);
            Vector2 target = CalculateTarget(min, max, center, rotation, startangle, gapspace);

            // apply correction
            Vector2 dir = (target - endpoint).normalized;
            float dist = UnityUtils.GetRayToLineSegmentIntersection(endpoint, dir, startpoint, previoustarget);
            target = endpoint + dir * (dist - gapspace);

            // add gap
            float gapangle = gap * angleperaperturesegment * 0.5f;
            startangle += gapangle;

            int segmentindexstart = s * (circleSegments * 3);

            for (int i = 0; i < circleSegments; ++i)
            {
                Vector2 pos1 = AngleToPoint(min, max, startangle + i * anglepersegment);
                Vector2 pos2 = AngleToPoint(min, max, startangle + (i + 1) * anglepersegment);

                vh.AddVert(target, color, Vector2.zero); // center point
                vh.AddVert(pos1, color, Vector2.zero);
                vh.AddVert(pos2, color, Vector2.zero);

                int idx = segmentindexstart + i * 3;
                vh.AddTriangle(idx + 0, idx + 1, idx + 2);
            }
        }
    }
}
