using UnityEngine;
using System.Collections.Generic;

public enum DecalTexCoordType
{
    WorldPosition,
    LocalPosition
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class Decal : MonoBehaviour
{
    [Range(1, 180)]
    public float maxAngle = 90.0f;
    [Range(0.0001f, 0.1f)]
    public float pushDistance = 0.01f;
    [Range(0, 1)]
    public float vertexColorAmount = 1;
    public Color color = Color.white;
    [Range(0, 1)]
    public float facetNormalAmount = 0;
    public LayerMask affectedLayers = -1;
    public DecalTexCoordType texCoordType = DecalTexCoordType.WorldPosition;
    public Vector2 tiling = Vector2.one;
    public Vector2 offset = Vector2.zero;
    public bool lockAffectedObjects = false;
    public List<MeshFilter> affectedObjects = new List<MeshFilter>();

    void OnEnable()
    {
        if (Application.isPlaying)
            enabled = false;
    }

    void Start()
    {
        transform.hasChanged = false;
    }

    void Update()
    {
        if (transform.parent && transform.parent.hasChanged)
        {
            transform.parent.hasChanged = false; // when inspector is not allowed
            Build();
        }
    }

    static Bounds GetTransformBounds(Transform transform)
    {
        Vector3 size = transform.lossyScale;
        Vector3 min = -size * 0.5f;
        Vector3 max = size * 0.5f;

        Vector3[] vts = new Vector3[]
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(max.x, max.y, min.z),

            new Vector3(min.x, min.y, max.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, max.y, max.z),
        };

        for (int i = 0; i < vts.Length; ++i)
            vts[i] = transform.TransformDirection(vts[i]);

        min = vts[0];
        max = vts[0];
        for (int i = 1; i < vts.Length; ++i)
        {
            min = Vector3.Min(min, vts[i]);
            max = Vector3.Min(max, vts[i]);
        }

        return new Bounds(transform.position, max - min);
    }

    public List<MeshFilter> Build()
    {
        MeshFilter filter = GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();

        if (filter.sharedMesh != null && !filter.sharedMesh.isReadable)
        {
            return null;
        }

        if (!lockAffectedObjects)
        {
            affectedObjects.Clear();

            Bounds bounds = GetTransformBounds(transform);
            foreach (MeshRenderer meshrenderer in FindObjectsOfType<MeshRenderer>())
            {
                if (!UnityUtils.HasLayer(affectedLayers, meshrenderer.gameObject.layer))
                    continue;
                if (meshrenderer.gameObject.scene != gameObject.scene)
                    continue;
                if (meshrenderer.GetComponent<Decal>() != null)
                    continue;
                if (!bounds.Intersects(meshrenderer.bounds))
                    continue;

                MeshFilter meshfilter = meshrenderer.GetComponent<MeshFilter>();
                if (meshfilter == null || meshfilter.sharedMesh == null)
                    continue;

                affectedObjects.Add(meshfilter);
            }
        }

        DecalBuilder builder = new DecalBuilder();
        builder.Build(transform, maxAngle, vertexColorAmount, facetNormalAmount, affectedObjects);
        builder.MoveAlongNormals(pushDistance);
        if (texCoordType == DecalTexCoordType.LocalPosition)
            builder.ConvertToLocalTexCoords();
        builder.TransformTexCoords(tiling, offset);
        builder.MultiplyColor(color);

        string meshname = "Decal_" + filter.GetInstanceID().ToString();
        if (filter.sharedMesh == null || filter.sharedMesh.name != meshname)
        {
            filter.sharedMesh = new Mesh();
            filter.sharedMesh.name = meshname;
        }

        builder.ToMesh(filter.sharedMesh);
        builder.Clear();

        return affectedObjects;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
