using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LinePathPoint
{
    public Vector3 position;
    public Vector3 direction;
    public float distance;
    public float ratio;
    public float segment;
    public int segmentIndex => Mathf.FloorToInt(segment);
    public float segmentRatio => segment - Mathf.FloorToInt(segment);

    public LinePathPoint Copy(LinePathPoint output = null)
    {
        if (output == null)
            output = new LinePathPoint();

        output.position = position;
        output.direction = direction;
        output.distance = distance;
        output.ratio = ratio;
        output.segment = segment;
        return output;
    }

    public static LinePathPoint Lerp(LinePathPoint a, LinePathPoint b, float t, LinePathPoint output = null)
    {
        if (output == null)
            output = new LinePathPoint();

        output.position = Vector3.LerpUnclamped(a.position, b.position, t);
        output.direction = Vector3.SlerpUnclamped(a.direction, b.direction, t);
        output.distance = Mathf.LerpUnclamped(a.distance, b.distance, t);
        output.ratio = Mathf.LerpUnclamped(a.ratio, b.ratio, t);
        output.segment = Mathf.LerpUnclamped(a.segment, b.segment, t);
        return output;
    }
}

public static class BezierPathTools
{
    // Calculates 2 control points that will create a smooth bezier curve.
    // http://www.antigrain.com/research/bezier_interpolation/
    static void SmoothBezierControlPoints(Vector3 a, Vector3 b, Vector3 c, out Vector3 outab, out Vector3 outbc, float curvature)
    {
        Vector3 ab = (a + b) * 0.5f;
        Vector3 bc = (c + b) * 0.5f;
        float abmagnitude = (a - b).magnitude;
        float bcmagnitude = (b - c).magnitude;
        Vector3 midpoint = Vector3.Lerp(ab, bc, abmagnitude / (abmagnitude + bcmagnitude));
        outab = b + (ab - midpoint) * curvature;
        outbc = b + (bc - midpoint) * curvature;
    }

    // Clamps an index to 0 to (total - 1), where overflow will circle around to the start/end.
    static int CircularClamp(int v, int total)
    {
        if (v >= total)
            return v % total;
        else if (v < 0)
            return total + (v % total);
        return v;
    }

    public static Vector3[] GenerateSmoothControlPoints(List<Vector3> bezierpoints, float curvature, bool loop)
    {
        int segmentcount = loop ? bezierpoints.Count : bezierpoints.Count - 1;
        Vector3[] cp = new Vector3[segmentcount * 2];

        // treat the first and last points differently
        int start = 0;
        int end = bezierpoints.Count;
        if (!loop)
        {
            start = 1;
            end = bezierpoints.Count - 1;
        }

        // do the middle points
        for (int i = start; i < end; ++i)
        {
            Vector3 a = bezierpoints[CircularClamp(i - 1, bezierpoints.Count)];
            Vector3 b = bezierpoints[i];
            Vector3 c = bezierpoints[(i + 1) % bezierpoints.Count];
            SmoothBezierControlPoints(a, b, c, out cp[(i - start) * 2 + start], out cp[(i - start) * 2 + start + 1], curvature);
        }

        // treat the first and last points differently
        if (!loop)
        {
            cp[0] = bezierpoints[0] + (cp[1] - bezierpoints[0]).normalized * Vector3.Distance(cp[1], bezierpoints[1]);
            cp[cp.Length - 1] = bezierpoints[bezierpoints.Count - 1] + (cp[cp.Length - 2] - bezierpoints[bezierpoints.Count - 1]).normalized * Vector3.Distance(cp[cp.Length - 2], bezierpoints[bezierpoints.Count - 2]);
        }

        return cp;
    }

    // Returns a point on a bezier curve where b is a control point
    public static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return Vector3.LerpUnclamped(Vector3.LerpUnclamped(a, b, t), Vector3.LerpUnclamped(b, c, t), t);
    }

    // Returns a point on a bezier curve where b and c are control points
    public static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        Vector3 ab = Vector3.LerpUnclamped(a, b, t);
        Vector3 bc = Vector3.LerpUnclamped(b, c, t);
        Vector3 cd = Vector3.LerpUnclamped(c, d, t);
        return Bezier(ab, bc, cd, t);
    }

    static float PointLineDistance(Vector3 point, Vector3 start, Vector3 end)
    {
        if (start == end)
            return Vector3.Distance(point, start);

        Vector3 dir = (end - start).normalized;
        float d = Vector3.Dot(point - start, dir);
        return Vector3.Distance(point, start + dir * d);
    }

    // Simplifies points in a polyline
    static List<LinePathPoint> DouglasPeucker(List<LinePathPoint> points, int startIndex, int lastIndex, float epsilon)
    {
        float dmax = 0f;
        int index = startIndex;

        for (int i = index + 1; i < lastIndex; ++i)
        {
            float d = PointLineDistance(points[i].position, points[startIndex].position, points[lastIndex].position);
            if (d > dmax)
            {
                index = i;
                dmax = d;
            }
        }

        if (dmax > epsilon)
        {
            var res1 = DouglasPeucker(points, startIndex, index, epsilon);
            var res2 = DouglasPeucker(points, index, lastIndex, epsilon);

            var finalRes = new List<LinePathPoint>();
            for (int i = 0; i < res1.Count - 1; ++i)
                finalRes.Add(res1[i]);

            for (int i = 0; i < res2.Count; ++i)
                finalRes.Add(res2[i]);

            return finalRes;
        }

        return new List<LinePathPoint>(new LinePathPoint[] { points[startIndex], points[lastIndex] });
    }

    public static List<LinePathPoint> OptimiseLinearPoints(List<LinePathPoint> points, float optimiserange)
    {
        return DouglasPeucker(points, 0, points.Count - 1, optimiserange);
    }

    public static void CalculateDirections(List<LinePathPoint> points, bool loop)
    {
        for (int i = 0; i < points.Count; ++i)
        {
            if (i == 0)
            {
                if (loop)
                    points[i].direction = Vector3.SlerpUnclamped(points[0].position - points[points.Count - 1].position, points[1].position - points[0].position, 0.5f).normalized;
                else
                    points[i].direction = (points[1].position - points[0].position).normalized;
            }
            else if (i == points.Count - 1)
            {
                if (loop)
                    points[i].direction = Vector3.SlerpUnclamped(points[points.Count - 1].position - points[points.Count - 2].position, points[0].position - points[points.Count - 1].position, 0.5f).normalized;
                else
                    points[i].direction = (points[0].position - points[points.Count - 1].position).normalized;
            }
            else
                points[i].direction = Vector3.SlerpUnclamped(points[i].position - points[i - 1].position, points[i + 1].position - points[i].position, 0.5f).normalized;
        }
    }

    public static float GetClosestRatioPointOnLineSegment(Vector3 A, Vector3 B, Vector3 P)
    {
        Vector3 AP = P - A;
        Vector3 AB = B - A;
        float magnitudeAB = AB.sqrMagnitude;
        float ABAPproduct = Vector3.Dot(AP, AB);
        return Mathf.Clamp01(ABAPproduct / magnitudeAB);
    }
}

public enum LinePathInterpolation
{
    Linear,
    Bezier
}

/*
    Jargon:
        Position: World vector position of a point on the bezier curve
        Segment: The distance along the path where the start is 0 and the end is points.Count
        Ratio: The distance along the path where the start is 0 and the end is 1
        Distance: The distance along the where the start is 0 and the end is the approximated world distance to the end
        Fraction: The distance along the path where the start is 0 and the end is cachedPoints.Count
*/
public class LinePath : MonoBehaviour
{
    List<LinePathPoint> _cachedPoints = null;
    public List<LinePathPoint> cachedPoints
    {
        get
        {
            if (_cachedPoints == null)
                Rebuild();
            return _cachedPoints;
        }
    }
    public float totalDistance => cachedPoints.Count > 0 ? cachedPoints[cachedPoints.Count - 1].distance : 0;
    public int fractionCount => _cachedPoints.Count - 1;
    public int segmentCount => loop ? points.Count : points.Count - 1;
    public LinePathPoint finalPoint => cachedPoints[cachedPoints.Count - 1];

    [Header("Generation")]
    public List<Vector3> points = new List<Vector3>();
    public bool loop = false;
    public LinePathInterpolation interpolation = LinePathInterpolation.Linear;
    [Range(0, 1)]
    public float smoothness = 1;
    public int pointsPerSegment = 1;
    [Range(0.0f, 5.0f)]
    public float optimiseRange = 0;

    [Header("Display")]
    public Color displayLineColor = Color.red;
    public float displayPointSize = 0.2f;

    public Bounds bounds
    {
        get
        {
            Bounds bounds = new Bounds(cachedPoints[0].position, Vector3.zero);
            for (int i = 1; i < cachedPoints.Count; ++i)
                bounds.Encapsulate(cachedPoints[i].position);
            return bounds;
        }
    }

    //////////////// BUILDING METHODS ////////////////

    public void Rebuild()
    {
        if (points.Count <= 1)
        {
            _cachedPoints = new List<LinePathPoint>();
            return;
        }
        else if (points.Count == 2)
        {
            _cachedPoints = new List<LinePathPoint>();

            LinePathPoint first = new LinePathPoint()
            {
                position = points[0],
                direction = (points[1] - points[0]).normalized,
                distance = 0,
                ratio = 0,
                segment = 0
            };
            _cachedPoints.Add(first);

            LinePathPoint second = new LinePathPoint()
            {
                position = points[1],
                direction = first.direction,
                distance = Vector3.Distance(first.position, points[1]),
                ratio = 1,
                segment = 1
            };
            _cachedPoints.Add(second);

            return;
        }

        // create linear points
        if (_cachedPoints == null)
            _cachedPoints = new List<LinePathPoint>(points.Count);
        else
            _cachedPoints.Clear();

        for (int i = 0; i < segmentCount; ++i)
        {
            Vector3 a = points[i];
            Vector3 b = points[(i + 1) % points.Count];

            for (int p = 0; p < pointsPerSegment; ++p)
            {
                float t = (float)p / pointsPerSegment;
                _cachedPoints.Add(new LinePathPoint()
                {
                    position = Vector3.LerpUnclamped(a, b, t),
                    segment = i + t
                });
            }
        }
        _cachedPoints.Add(new LinePathPoint()
        {
            position = points[loop ? 0 : points.Count - 1],
            segment = loop ? points.Count : points.Count - 1
        });

        // add curvature
        if (interpolation == LinePathInterpolation.Bezier)
        {
            Vector3[] cps = BezierPathTools.GenerateSmoothControlPoints(points, smoothness, loop);
            int cpoffset = loop ? 1 : 0;

            foreach (LinePathPoint point in _cachedPoints)
            {
                int i = point.segmentIndex % points.Count;
                Vector3 p0 = points[i];
                Vector3 cp0 = cps[(i * 2 + 0 + cpoffset) % cps.Length];
                Vector3 cp1 = cps[(i * 2 + 1 + cpoffset) % cps.Length];
                Vector3 p1 = points[(i + 1) % points.Count];

                point.position = BezierPathTools.Bezier(p0, cp0, cp1, p1, point.segmentRatio);
            }
        }

        // optimise points
        _cachedPoints = BezierPathTools.OptimiseLinearPoints(_cachedPoints, optimiseRange);
        BezierPathTools.CalculateDirections(_cachedPoints, loop);

        // calcaulte lengths
        _cachedPoints[0].distance = 0;
        for (int i = 1; i < _cachedPoints.Count; ++i)
            _cachedPoints[i].distance = _cachedPoints[i - 1].distance + Vector3.Distance(_cachedPoints[i - 1].position, _cachedPoints[i].position);

        // calcaulte ratios
        for (int i = 0; i < _cachedPoints.Count; ++i)
            _cachedPoints[i].ratio = _cachedPoints[i].distance / totalDistance;

        // calculate directions
        LinePathPoint cachedpoint = new LinePathPoint();
        for (int i = 0; i < _cachedPoints.Count; ++i)
        {
            Vector3 a = SamplePoint(i - 0.001f, cachedpoint).position;
            Vector3 b = SamplePoint(i + 0.001f, cachedpoint).position;
            _cachedPoints[i].direction = (b - a).normalized;
        }
    }

    //////////////// GET SEGMENT METHODS ////////////////

    // Finds fraction that contains the point at distance (TODO: optimise by using a better search algorithm)
    float GetFractionAtDistance(float distance)
    {
        if (distance < 0)
        {
            if (!loop)
                return 0;
            distance = (distance % totalDistance) + totalDistance;
        }
        else if (distance >= totalDistance)
        {
            if (!loop)
                return cachedPoints.Count - 1;
            distance %= totalDistance;
        }

        for (int i = 1; i < cachedPoints.Count; ++i)
        {
            if (cachedPoints[i].distance > distance)
                return (i - 1) + Mathf.InverseLerp(cachedPoints[i - 1].distance, cachedPoints[i].distance, distance);
        }
        return cachedPoints.Count - 1;
    }

    // Finds fraction that contains the point at ratio (TODO: optimise by using a better search algorithm)
    float GetFractionAtRatio(float ratio)
    {
        if (ratio < 0)
        {
            if (!loop)
                return 0;
            ratio = (ratio % 1) + 1;
        }
        else if (ratio >= 1)
        {
            if (!loop)
                return cachedPoints.Count - 1;
            ratio %= 1;
        }

        for (int i = 1; i < cachedPoints.Count; ++i)
        {
            if (cachedPoints[i].ratio > ratio)
                return (i - 1) + Mathf.InverseLerp(cachedPoints[i - 1].ratio, cachedPoints[i].ratio, ratio);
        }
        return cachedPoints.Count - 1;
    }

    // Finds fraction that contains the point at segment (TODO: optimise by using a better search algorithm)
    float GetFractionAtSegment(float segment)
    {
        if (segment < 0)
        {
            if (!loop)
                return 0;
            segment = (segment % segmentCount) + segmentCount;
        }
        else if (segment >= segmentCount)
        {
            if (!loop)
                return cachedPoints.Count - 1;
            segment %= segmentCount;
        }

        for (int i = 1; i < cachedPoints.Count; ++i)
        {
            if (cachedPoints[i].segment > segment)
                return (i - 1) + Mathf.InverseLerp(cachedPoints[i - 1].segment, cachedPoints[i].segment, segment);
        }
        return cachedPoints.Count - 1;
    }

    // Finds the nearest fraction to the specified world position
    float GetNearestFractionAtPosition(Vector3 p, float refdist = float.NaN, float furthestdistance = float.NaN)
    {
        bool isnearestwithindistance = false;
        float nearestsegment = 0;
        float nearestdistsqr = float.MaxValue;
        for (int i = 0; i < cachedPoints.Count - 1; ++i)
        {
            float subratio = BezierPathTools.GetClosestRatioPointOnLineSegment(cachedPoints[i].position, cachedPoints[i + 1].position, p);
            Vector3 point = Vector3.LerpUnclamped(cachedPoints[i].position, cachedPoints[i + 1].position, subratio);

            bool iswithindistance = true;
            if (furthestdistance != float.NaN)
            {
                float dist = Mathf.LerpUnclamped(cachedPoints[i].distance, cachedPoints[i + 1].distance, subratio);
                dist = Mathf.Abs(refdist - dist);
                if (dist > totalDistance * 0.5f)
                    dist = totalDistance - dist;

                iswithindistance = dist < furthestdistance;
                if (!iswithindistance && isnearestwithindistance)
                    continue;
            }

            float distsqr = (p - point).sqrMagnitude;
            if (iswithindistance == isnearestwithindistance && distsqr > nearestdistsqr)
                continue;

            isnearestwithindistance = iswithindistance;
            nearestsegment = i + subratio;
            nearestdistsqr = distsqr;
        }
        return nearestsegment;
    }

    //////////////// GET POINT METHODS ////////////////

    // Finds BezierPathPoint that contains the point at distance (TODO: optimise by using a better search algorithm)
    public LinePathPoint GetPointAtDistance(float distance, LinePathPoint output = null)
    {
        return SamplePoint(GetFractionAtDistance(distance), output);
    }

    // Finds BezierPathPoint that contains the point at ratio (TODO: optimise by using a better search algorithm)
    public LinePathPoint GetPointAtRatio(float ratio, LinePathPoint output = null)
    {
        return SamplePoint(GetFractionAtRatio(ratio), output);
    }

    // Finds BezierPathPoint that contains the point at bezierratio (TODO: optimise by using a better search algorithm)
    public LinePathPoint GetPointFromSegment(float segment, LinePathPoint output = null)
    {
        return SamplePoint(GetFractionAtSegment(segment), output);
    }

    // Finds the nearest segment to the specified world position
    public LinePathPoint GetNearestPointAtPosition(Vector3 p, LinePathPoint output = null)
    {
        return SamplePoint(GetNearestFractionAtPosition(p), output);
    }

    // Finds the nearest segment to the specified world position
    public LinePathPoint GetNearestPointAtPosition(Vector3 p, float refdist, float furthestdistance, LinePathPoint output = null)
    {
        return SamplePoint(GetNearestFractionAtPosition(p, refdist, furthestdistance), output);
    }

    //////////////// HELPER METHODS ////////////////

    // Finds the lerped SmoothBezierPoint that contains the point at distance
    LinePathPoint SamplePoint(float fraction, LinePathPoint output = null)
    {
        if (cachedPoints.Count == 0)
        {
            if (points.Count >= 2)
                Rebuild();
            if (cachedPoints.Count == 0)
            {
                Debug.LogError("The bezier path does not have any points!", this);
                return null;
            }
        }
        if (fraction < 0)
        {
            if (!loop)
                return cachedPoints[0].Copy(output);
            fraction = (fraction % fractionCount) + fractionCount;
        }
        else if (fraction >= fractionCount)
        {
            if (!loop)
                return cachedPoints[cachedPoints.Count - 1].Copy(output);
            fraction %= fractionCount;
        }

        int idx = Mathf.FloorToInt(fraction);
        return LinePathPoint.Lerp(cachedPoints[idx], cachedPoints[idx + 1], fraction - idx, output);
    }
}
