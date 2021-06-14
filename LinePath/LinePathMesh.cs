using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class LinePathMesh : MonoBehaviour
{
    public LinePath[] paths;
    public Vector2[] shapePoints = { new Vector2(0, 0), new Vector2(0, 1) };
    public bool loopShapePoints = false;
    public float shapeRotation = 0;
    public float scale = 1;
    public float scaleRandomness = 0;
    public int randomSeed = 0;
    public bool doubleSided = false;
    public bool finishAtZero = false;
    public bool smoothNormals = false;
    public bool rotateUVs = false;
    public Vector2 uvOffset = Vector2.zero;
    public Vector2 uvScale = Vector2.one;

    [Header("Vertex Color")]
    [Range(0, 1)]
    public float minSaturation = 0;
    [Range(0, 1)]
    public float maxSaturation = 0;
    [Range(0, 1)]
    public float minLightness = 1;
    [Range(0, 1)]
    public float maxLightness = 1;
    [Range(0, 1)]
    public float minAlpha = 1;
    [Range(0, 1)]
    public float maxAlpha = 1;

#if UNITY_EDITOR
    Mesh _mesh = null;

    struct Vertex
    {
        public Vector3 position;
        public Vector2 texCoord;
        public Vector3 smoothNormal;
        public Vector3 normal;
        public Vector3 smoothForward;
        public Vector3 forward;
        public Vector3 binormal => forward == normal ? Vector3.right : Vector3.Cross(forward, normal);
        public Color color;
        public int index;
    }

    static int Repeat(int i, int count, bool loop)
    {
        if (loop)
        {
            i %= count;
            return i < 0 ? i + count : i;
        }
        else
            return Mathf.Clamp(i, 0, count - 1);
    }

    List<Vertex> GenerateBaseVertices(LinePath path, float shapelength)
    {
        List<Vertex> verts = new List<Vertex>();

        float uvdistance = 0;
        for (int i = 0; i < path.cachedPoints.Count; ++i)
        {
            int ibefore = Repeat(i - 1, path.cachedPoints.Count, path.loop);
            int icurrent = Repeat(i, path.cachedPoints.Count, path.loop);
            int iafter = Repeat(i + 1, path.cachedPoints.Count, path.loop);

            Random.InitState(randomSeed + icurrent);

            Vertex v = new Vertex();
            v.position = path.cachedPoints[icurrent].position;
            v.texCoord = new Vector2(uvdistance, 0);
            v.index = icurrent;

            // texcoord
            uvdistance += Vector3.Distance(path.cachedPoints[icurrent].position, path.cachedPoints[iafter].position) * uvScale.x / shapelength;
            if (finishAtZero)
                uvdistance = Mathf.Round(uvdistance);

            // normal
            Vector3 forward0 = (path.cachedPoints[icurrent].position - path.cachedPoints[ibefore].position).normalized;
            Vector3 forward1 = (path.cachedPoints[iafter].position - path.cachedPoints[icurrent].position).normalized;
            Vector3 normal0 = Vector3.Dot(Vector3.up, forward0) > 0.9999f ? Vector3.right : Vector3.Cross(Vector3.up, forward0).normalized;
            Vector3 normal1 = Vector3.Dot(Vector3.up, forward1) > 0.9999f ? Vector3.right : Vector3.Cross(Vector3.up, forward1).normalized;
            v.normal = v.smoothNormal =  (normal0 + normal1).normalized;
            v.forward = v.smoothForward = (forward0 + forward1).normalized;

            // color
            v.color = Color.HSVToRGB(Random.value, Random.Range(minSaturation, maxSaturation), Random.Range(minLightness, maxLightness));
            v.color.a = Random.Range(minAlpha, maxAlpha);

            verts.Add(v);
        }

        return verts;
    }
    
    List<int> GenerateIndices(List<Vertex> verts)
    {
        List<int> indices = new List<int>();
        if (smoothNormals)
        {
            for (int i = 0; i < verts.Count - 1; ++i)
            {
                indices.Add(i);
                indices.Add(i + 1);
            }
        }
        else
        {
            // split base verts
            List<Vertex> newverts = new List<Vertex>();
            for (int i = 0; i < verts.Count - 1; ++i)
            {
                Vertex a = verts[i];
                Vertex b = verts[i + 1];

                Vector3 dir = b.position - a.position;
                a.normal = b.normal = Vector3.Cross(Vector3.up, dir).normalized;
                a.forward = b.forward = dir.normalized;

                newverts.Add(a);
                newverts.Add(b);
            }
            verts.Clear();
            verts.AddRange(newverts);

            for (int i = 0; i < verts.Count; ++i)
                indices.Add(i);
        }
        return indices;
    }

    void Extrude(List<Vertex> verts, List<int> indices)
    {
        int shapesegments = loopShapePoints ? shapePoints.Length + 1 : shapePoints.Length;

        // rotate shape points
        List<Vector2> shapepoints = new List<Vector2>(shapePoints);
        for (int i = 0; i < shapepoints.Count; ++i)
            shapepoints[i] = Quaternion.AngleAxis(shapeRotation, Vector3.forward) * shapepoints[i];

        // get all of the angles for the shape points (in CW degrees)
        List<float> shapeangles = new List<float>();
        for (int s = 0; s < shapepoints.Count; ++s)
        {
            int sbefore = Repeat(s - 1, shapepoints.Count, loopShapePoints);
            int safter = Repeat(s + 1, shapepoints.Count, loopShapePoints);

            Vector2 normal0 = (shapepoints[s] - shapepoints[sbefore]).Rotate(90 * Mathf.Deg2Rad).normalized;
            Vector2 normal1 = (shapepoints[safter] - shapepoints[s]).Rotate(90 * Mathf.Deg2Rad).normalized;
            Vector2 normal = (normal0 + normal1) * 0.5f;
            shapeangles.Add(Vector2.SignedAngle(normal, Vector2.right));
        }

        // vertices
        List<Vertex> newverts = new List<Vertex>();
        for (int i = 0; i < verts.Count; ++i)
        {
            Vertex basevert = verts[i];
            basevert.texCoord += uvOffset;
            
            Random.InitState(randomSeed + verts[i].index);
            float pointheight = scale + Random.Range(-scaleRandomness, scaleRandomness);
            for (int s = 0; s < shapesegments; ++s)
            {
                int sidx = s % shapepoints.Count;

                float t = (float)s / (shapesegments - 1);
                Vertex vert = basevert;
                vert.position += vert.binormal * (shapepoints[sidx].y * pointheight);
                vert.position += basevert.smoothNormal * shapepoints[sidx].x * pointheight;
                vert.texCoord.y += t * uvScale.y;
                vert.normal = Quaternion.AngleAxis(-shapeangles[sidx], vert.forward) * vert.normal;
                newverts.Add(vert);
            }
        }
        verts.Clear();
        verts.AddRange(newverts);

        // indices
        List<int> newindices = new List<int>();
        for (int i = 0; i < indices.Count / 2; ++i)
        {
            for (int s = 0; s < shapesegments - 1; ++s)
            {
                int i0 = indices[i * 2 + 0] * shapesegments + s + 0;
                int i1 = indices[i * 2 + 0] * shapesegments + s + 1;
                int i2 = indices[i * 2 + 1] * shapesegments + s + 0;
                int i3 = indices[i * 2 + 1] * shapesegments + s + 1;

                newindices.Add(i0);
                newindices.Add(i1);
                newindices.Add(i2);

                newindices.Add(i1);
                newindices.Add(i3);
                newindices.Add(i2);
            }
        }
        indices.Clear();
        indices.AddRange(newindices);
    }

    void ApplyDoubleSided(List<Vertex> verts, List<int> indices)
    {
        int vertcount = verts.Count;
        for (int i = 0; i < vertcount; ++i)
        {
            Vertex v = verts[i];
            v.normal = -v.normal;
            verts.Add(v);
        }

        int triangles = indices.Count / 3;
        for (int i = 0; i < triangles; ++i)
        {
            indices.Add(vertcount + indices[i * 3 + 0]);
            indices.Add(vertcount + indices[i * 3 + 2]);
            indices.Add(vertcount + indices[i * 3 + 1]);
        }
    }

    void RotateUVs(List<Vertex> verts)
    {
        for (int i = 0; i < verts.Count; ++i)
        {
            Vertex vert = verts[i];
            vert.texCoord = new Vector2(vert.texCoord.y, vert.texCoord.x);
            verts[i] = vert;
        }
    }

    void TransformToLocal(LinePath path, List<Vertex> verts)
    {
        Matrix4x4 matrix = path.transform.worldToLocalMatrix;
        for (int i = 0; i < verts.Count; ++i)
        {
            Vertex vert = verts[i];
            vert.position = matrix.MultiplyPoint3x4(vert.position);
            vert.normal = matrix.MultiplyVector(vert.normal);
            verts[i] = vert;
        }
    }

    void ApplyToMesh(List<Vertex> verts, List<int> indices)
    {
        // setup mesh
        MeshFilter filter = GetComponent<MeshFilter>();
        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.name = "LinePathMesh";
        }
        filter.sharedMesh = _mesh;

        MeshCollider meshcollider = GetComponent<MeshCollider>();
        if (meshcollider != null)
            meshcollider.sharedMesh = _mesh;

        // split vertex data
        List<Vector3> positions = new List<Vector3>();
        List<Vector2> texcoords = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<Color> colors = new List<Color>();
        for (int i = 0; i < verts.Count; ++i)
        {
            positions.Add(verts[i].position);
            texcoords.Add(verts[i].texCoord);
            normals.Add(verts[i].normal);
            colors.Add(verts[i].color);
        }

        // fill mesh
        _mesh.Clear();
        _mesh.SetVertices(positions);
        _mesh.SetUVs(0, texcoords);
        _mesh.SetNormals(normals);
        _mesh.SetColors(colors);
        _mesh.triangles = indices.ToArray();
        _mesh.UploadMeshData(false);
    }

    void Generate()
    {
        float shapelength = 0;
        for (int i = 0; i < shapePoints.Length - 1; ++i)
            shapelength += Vector2.Distance(shapePoints[i], shapePoints[i + 1]);
        shapelength *= scale;

        List<Vertex> finalverts = new List<Vertex>();
        List<int> finalindices = new List<int>();
        foreach (LinePath path in paths)
        {
            List<Vertex> verts = GenerateBaseVertices(path, shapelength);
            List<int> indices = GenerateIndices(verts);
            Extrude(verts, indices);
            TransformToLocal(path, verts);
            if (rotateUVs)
                RotateUVs(verts);
            if (doubleSided)
                ApplyDoubleSided(verts, indices);

            for (int i = 0; i < indices.Count; ++i)
                indices[i] += finalverts.Count;
            finalverts.AddRange(verts);
            finalindices.AddRange(indices);
        }
        ApplyToMesh(finalverts, finalindices);
    }

    [ContextMenu("Export As OBJ")]
    void ExportAsOBJ()
    {
        string filename = UnityEditor.EditorUtility.SaveFilePanel("Export As OBJ", null, gameObject.name, "obj");
        if (!string.IsNullOrEmpty(filename))
            OBJExporter.ExportAsOBJ(filename, GetComponent<MeshFilter>().sharedMesh);
    }

    void Update()
    {
        if (UnityEditor.Selection.activeGameObject != gameObject || Application.isPlaying)
            return;
        
        Generate();
        System.Array.ForEach(GetComponentsInChildren<LinePathMesh>(), m =>
        {
            if (m != this)
                m.Generate();
        });
    }
#endif
}
