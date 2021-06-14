using System.Collections.Generic;
using UnityEngine;

public class DecalBuilder
{
    class DecalVertex
    {
        public Vector3 position;
        public Color color;
        public Vector3 normal;

        public DecalVertex(Vector3 position, Color color, Vector3 normal)
        {
            this.position = position;
            this.color = color;
            this.normal = normal;
        }
    }

    static readonly Plane[] planes =
    {
        new Plane(Vector3.right, 0.5f),
        new Plane(Vector3.left, 0.5f),
        new Plane(Vector3.up, 0.5f),
        new Plane(Vector3.down, 0.5f),
        new Plane(Vector3.forward, 0.5f),
        new Plane(Vector3.back, 0.5f)
    };

    List<Vector3> _vertices = new List<Vector3>();
    List<Color> _colors = new List<Color>();
    List<Vector3> _originalNormals = new List<Vector3>();
    List<Vector3> _normals = new List<Vector3>();
    List<Vector2> _texCoords = new List<Vector2>();
    List<Vector2> _texCoords2 = new List<Vector2>();
    List<int> _indices = new List<int>();

    List<DecalVertex> _polygonClipBuffer1 = new List<DecalVertex>();
    List<DecalVertex> _polygonClipBuffer2 = new List<DecalVertex>();

    public void Clear()
    {
        _vertices.Clear();
        _colors.Clear();
        _originalNormals.Clear();
        _normals.Clear();
        _texCoords.Clear();
        _texCoords2.Clear();
        _indices.Clear();
    }

    DecalVertex[] ClipPolygonByLocalBoundingBox(params DecalVertex[] poly)
    {
        _polygonClipBuffer1.Clear();
        _polygonClipBuffer1.AddRange(poly);

        for (int p = 0; p < planes.Length; ++p)
        {
            Plane plane = planes[p];

            _polygonClipBuffer2.Clear();
            for (int v = 0; v < _polygonClipBuffer1.Count; v++)
            {
                int next = (v + 1) % _polygonClipBuffer1.Count;
                DecalVertex v1 = _polygonClipBuffer1[v];
                DecalVertex v2 = _polygonClipBuffer1[next];

                if (plane.GetSide(v1.position))
                    _polygonClipBuffer2.Add(v1);

                if (plane.GetSide(v1.position) != plane.GetSide(v2.position))
                {
                    // raycast plane
                    Ray ray = new Ray(v1.position, v2.position - v1.position);
                    plane.Raycast(ray, out float distance);
                    float t = distance / Vector3.Distance(v1.position, v2.position);

                    Vector3 position = Vector3.LerpUnclamped(v1.position, v2.position, t);
                    Color color = Color.LerpUnclamped(v1.color, v2.color, t);
                    Vector3 normal = Vector3.SlerpUnclamped(v1.normal, v2.normal, t);
                    _polygonClipBuffer2.Add(new DecalVertex(position, color, normal));
                }
            }
            CsUtils.Swap(ref _polygonClipBuffer1, ref _polygonClipBuffer2);
        }
        return _polygonClipBuffer1.ToArray();
    }

    public void Build(Transform transform, float maxangle, float verteccoloramount, float facetnormalamount, IEnumerable<MeshFilter> meshfilters)
    {
        foreach (MeshFilter meshfilter in meshfilters)
            Build(transform, maxangle, verteccoloramount, facetnormalamount, meshfilter);
    }

    public void Build(Transform transform, float maxangle, float vertexcoloramount, float facetnormalamount, MeshFilter meshfilter)
    {
        Matrix4x4 objToDecalMatrix = transform.worldToLocalMatrix * meshfilter.transform.localToWorldMatrix;

        Mesh mesh = meshfilter.sharedMesh;
        Vector3[] positions = mesh.vertices;
        Color[] colors = mesh.colors;
        if (colors.Length == 0)
            colors = null;
        Vector3[] normals = mesh.normals;
        int[] triangles = mesh.triangles;
        int startindex = _texCoords.Count;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i1 = triangles[i];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            DecalVertex v1 = new DecalVertex(objToDecalMatrix.MultiplyPoint(positions[i1]), colors == null ? Color.white : colors[i1], normals[i1]);
            DecalVertex v2 = new DecalVertex(objToDecalMatrix.MultiplyPoint(positions[i2]), colors == null ? Color.white : colors[i2], normals[i2]);
            DecalVertex v3 = new DecalVertex(objToDecalMatrix.MultiplyPoint(positions[i3]), colors == null ? Color.white : colors[i3], normals[i3]);

            AddTriangle(maxangle, v1, v2, v3, facetnormalamount);
        }

        Vector3 texscale = transform.localScale;
        for (int i = startindex; i < _texCoords.Count; ++i)
        {
            Vector2 texcoord = _texCoords[i];
            _texCoords2.Add(texcoord);
            texcoord.x *= texscale.x;
            texcoord.y *= texscale.y;
            _texCoords[i] = texcoord;
        }

        for (int i = 0; i < _colors.Count; ++i)
        {
            _colors[i] = Color.LerpUnclamped(Color.white, _colors[i], vertexcoloramount);
        }
    }
        
    void AddTriangle(float maxangle, DecalVertex v1, DecalVertex v2, DecalVertex v3, float facetnormalamount)
    {
        Rect uvRect = new Rect(Vector2.zero, Vector2.one);
        Vector3 normal = Vector3.Cross(v2.position - v1.position, v3.position - v1.position).normalized;

        if (Vector3.Angle(Vector3.forward, -normal) > maxangle)
            return;

        DecalVertex[] poly = ClipPolygonByLocalBoundingBox(v1, v2, v3);
        if (poly.Length == 0)
            return;

        AddPolygon(poly, normal, facetnormalamount, uvRect);
    }

    void AddPolygon(DecalVertex[] poly, Vector3 normal, float facetnormalamount, Rect uvRect)
    {
        int ind1 = AddVertex(poly[0], normal, facetnormalamount, uvRect);

        for (int i = 1; i < poly.Length - 1; i++)
        {
            int ind2 = AddVertex(poly[i], normal, facetnormalamount, uvRect);
            int ind3 = AddVertex(poly[i + 1], normal, facetnormalamount, uvRect);

            _indices.Add(ind1);
            _indices.Add(ind2);
            _indices.Add(ind3);
        }
    }

    int AddVertex(DecalVertex vertex, Vector3 normal, float facetnormalamount, Rect uvRect)
    {
        int index = FindVertex(vertex);
        if (index == -1)
        {
            _vertices.Add(vertex.position);
            _colors.Add(vertex.color);
            _originalNormals.Add(vertex.normal);
            _normals.Add(Vector3.SlerpUnclamped(vertex.normal, normal, facetnormalamount));
            AddTexCoord(vertex.position, uvRect);
            return _vertices.Count - 1;
        }
        else
        {
            _normals[index] = (_normals[index] + normal).normalized;
            return index;
        }
    }

    int FindVertex(DecalVertex vertex)
    {
        for (int i = 0; i < _vertices.Count; i++)
        {
            const float mindistance = 0.0001f;
            if ((_vertices[i] - vertex.position).sqrMagnitude > mindistance * mindistance)
                continue;

            //if (Vector3.Dot(_originalNormals[i], vertex.normal) < 0.8f)
            //    continue;

            const float mincolordiff = 0.01f;
            if (UnityUtils.ColorDistanceSqr(_colors[i], vertex.color) > mincolordiff * mincolordiff)
                continue;

            return i;
        }
        return -1;
    }

    void AddTexCoord(Vector3 ver, Rect uvRect)
    {
        float u = Mathf.Lerp(uvRect.xMin, uvRect.xMax, ver.x + 0.5f);
        float v = Mathf.Lerp(uvRect.yMin, uvRect.yMax, ver.y + 0.5f);
        _texCoords.Add(new Vector2(u, v));
    }

    public void MoveAlongNormals(float distance)
    {
        for (int i = 0; i < _vertices.Count; i++)
        {
            _vertices[i] += _normals[i] * distance;// new Vector3(0, 0, distance);
        }
    }

    public void ConvertToLocalTexCoords()
    {
        for (int i = 0; i < _texCoords.Count; i++)
            _texCoords[i] = _texCoords2[i];
    }

    public void TransformTexCoords(Vector2 tiling, Vector2 offset)
    {
        for (int i = 0; i < _texCoords.Count; i++)
            _texCoords[i] = new Vector2(_texCoords[i].x * tiling.x, _texCoords[i].y * tiling.y) + offset;
    }

    public void MultiplyColor(Color color)
    {
        for (int i = 0; i < _colors.Count; ++i)
            _colors[i] *= color;
    }

    public void ToMesh(Mesh mesh)
    {
        mesh.Clear(true);
        if (_indices.Count == 0)
            return;

        mesh.vertices = _vertices.ToArray();
        mesh.colors = _colors.ToArray();
        mesh.normals = _normals.ToArray();
        mesh.uv = _texCoords.ToArray();
        mesh.uv2 = _texCoords2.ToArray();
        mesh.triangles = _indices.ToArray();
    }
}
