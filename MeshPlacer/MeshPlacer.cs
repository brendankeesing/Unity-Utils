using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshPlacer : MonoBehaviour
{
    public MeshFilter meshFilter;
    public GameObject[] prefabs;
    public int submesh = 0;
    public int count = 10;
    public int seed = 0;
    public float closestDist = 1;
    public float minScale = 0.5f;
    public float maxScale = 1.0f;
    public float minVerticalScale = 1;
    public float maxVerticalScale = 1;
    [Range(0, 1)]
    public float upAmount = 1;

    [ContextMenu("Generate")]
    void Generate()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);

        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Matrix4x4 matrix = meshFilter.transform.localToWorldMatrix;
        for (int i = 0; i <vertices.Length; ++i)
        {
            vertices[i] = matrix.MultiplyPoint3x4(vertices[i]);
            normals[i] = matrix.MultiplyVector(normals[i]);
        }
        int[] indices = mesh.GetTriangles(submesh);

        float[] sizes = new float[indices.Length / 3];
        for (int i = 0; i < sizes.Length; ++i)
            sizes[i] = AreaOfTriangle(vertices[indices[i * 3]], vertices[indices[i * 3 + 1]], vertices[indices[i * 3 + 2]]);

        float[] times = new float[sizes.Length];
        times[0] = sizes[0];
        for (int i = 1; i < sizes.Length; ++i)
            times[i] = times[i - 1] + sizes[i];

        Random.InitState(seed);

        MeshCollider collider = GetComponent<MeshCollider>();
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < count; ++i)
        {
            float t = Random.Range(0, times[times.Length - 1]);
            int idx = Mathf.Clamp(System.Array.FindIndex(times, tt => tt > t), 0, sizes.Length - 1);

            int i0 = indices[idx * 3 + 0];
            int i1= indices[idx * 3 + 1];
            int i2 = indices[idx * 3 + 2];

            Vector3 p0 = vertices[i0];
            Vector3 p1 = vertices[i1];
            Vector3 p2 = vertices[i2];

            float t1 = Random.value;
            float t2 = Random.value;

            Vector3 pos = Vector3.LerpUnclamped(p0, Vector3.LerpUnclamped(p1, p2, t1), t2);
            if (positions.Exists(p => (p - pos).sqrMagnitude < closestDist * closestDist))
                continue;

            Vector3 n0 = normals[i0];
            Vector3 n1 = normals[i1];
            Vector3 n2 = normals[i2];
            Vector3 relativeup = Vector3.SlerpUnclamped(n0, Vector3.Slerp(n1, n2, t1), t2);

            // raycast
            if (!IsSafeArea(pos, relativeup, collider))
                continue;
            positions.Add(pos);

            Transform tfm = Instantiate(prefabs[Random.Range(0, prefabs.Length)], transform).transform;
            tfm.localPosition = pos;
            tfm.localRotation = Quaternion.FromToRotation(Vector3.up, Vector3.SlerpUnclamped(relativeup, Vector3.up, upAmount)) * Quaternion.AngleAxis(Random.Range(0, 360.0f), Vector3.up);

            Vector3 scale = Vector3.one * Random.Range(minScale, maxScale);
            scale.y *= Random.Range(minVerticalScale, maxVerticalScale);
            tfm.localScale = scale;
        }
    }

    public int raycastCount = 0;
    public float raycastRadius = 1;
    public float raycastHeight = 1;
    bool IsSafeArea(Vector3 pos, Vector3 up, MeshCollider collider)
    {
        if (raycastCount == 0)
            return true;

        pos = transform.TransformPoint(pos);
        up = transform.TransformDirection(up);

        Vector3 forward = Vector3.Cross(up, Vector3.forward) * raycastRadius;
        pos += up * (raycastHeight * 0.5f);
        for (int i = 0; i < raycastCount; ++i)
        {
            Vector3 offset = Quaternion.AngleAxis(i * 360f / raycastCount, up) * forward;
            Ray ray = new Ray(pos + offset, -up);
            if (!collider.Raycast(ray, out RaycastHit hit, raycastHeight) || UnityUtils.TriangleIndexToSubmesh(collider.sharedMesh, hit.triangleIndex) != submesh)
                return false;
        }

        return true;
    }

    public float AreaOfTriangle(Vector3 pt1, Vector3 pt2, Vector3 pt3)
    {
        return Vector3.Cross(pt1 - pt2, pt1 - pt3).magnitude * 0.5f;
    }
}
