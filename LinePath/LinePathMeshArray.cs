using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class LinePathMeshArray : MonoBehaviour
{
    public Mesh mesh;
    public LinePath path;
    public Vector3 offset = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public int maxCount = 10;
    public float minIntervalDistance = 1;

#if UNITY_EDITOR
    Mesh _mesh = null;

    void Generate()
    {
        if (mesh == null)
            return;

        if (!mesh.isReadable)
        {
            Debug.LogWarning("Mesh is not readable: " + mesh.name, mesh);
            return;
        }

        if (path == null)
        {
            path = GetComponent<LinePath>();
            if (path == null)
                return;
        }

        // setup mesh
        MeshFilter filter = GetComponent<MeshFilter>();
        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.name = "LinePathMeshArray";
        }
        filter.sharedMesh = _mesh;

        MeshCollider meshcollider = GetComponent<MeshCollider>();
        if (meshcollider != null)
            meshcollider.sharedMesh = _mesh;

        CombineInstance[] instances = new CombineInstance[Mathf.Min(Mathf.RoundToInt(path.totalDistance / minIntervalDistance), maxCount)];
        Quaternion rot = Quaternion.Euler(rotation);
        for (int i = 0; i < instances.Length; ++i)
        {
            LinePathPoint point = path.GetPointAtRatio((float)i / (instances.Length - 1));
            instances[i] = new CombineInstance()
            {
                mesh = mesh,
                transform = Matrix4x4.TRS(path.transform.InverseTransformPoint(point.position) + offset, Quaternion.LookRotation(point.direction) * rot, scale)
            };
        }

        _mesh.CombineMeshes(instances, true, true, false);
        _mesh.UploadMeshData(false);
    }

    void Update()
    {
        if (UnityEditor.Selection.activeGameObject != gameObject || Application.isPlaying)
            return;

        Generate();
        System.Array.ForEach(GetComponentsInChildren<LinePathMeshArray>(), m =>
        {
            if (m != this)
                m.Generate();
        });
    }
#endif
}
