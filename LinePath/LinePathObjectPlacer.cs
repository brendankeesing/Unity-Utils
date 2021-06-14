using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LinePathObjectPlacer : MonoBehaviour
{
#if UNITY_EDITOR
    public int seed = 0;
    public LinePath[] paths;

    [Header("Position")]
    [Range(0, 1)]
    public float startNormOffset = 0;
    public float spacing = 1;
    public bool fenceMode = false;
    public bool forceEndPoint = false;
    public bool onlyAtPoints = false;
    public float minWidth = 0;
    public float maxWidth = 0;
    [Range(0, 1)]
    public float offsetRandomness = 0;
    public float groundOffset = 0;
    public Vector3 worldOffset = Vector3.zero;

    [Header("Scale")]
    public float objectSizeMultiplier = 1;
    public Vector3 scaleMultiplier = Vector3.one;
    [Range(0, 1)]
    public float scaleRandomness = 0;

    [Header("Rotation")]
    public bool normalUp = true;
    public bool randomRotation = false;
    [Range(0, 90)]
    public float randomSlant = 0;
    public Vector3 extraRotation = Vector3.zero;

    [Header("Color")]
    public Color minColor = Color.white;
    public Color maxColor = Color.white;
    [Range(0, 1)]
    public float copyColorAmount = 1;
    public MeshInstanceRenderer copyColor = null;

    [Header("Raycast")]
    public bool raycast = true;
    public float maxRaycastDistance = 100;
    public LayerMask raycastLayerMask = -1;

    [Header("Objects")]
    public GameObject[] prefabs;
    public MeshInstanceRenderer meshInstanceRenderer;

    const float _forwardCheck = 0.01f;

    public void Apply()
    {
        if (Application.isPlaying)
            return;

        // checks
        spacing = Mathf.Clamp(spacing, 0.1f, 100);

        // destroy old objects
        foreach (MeshFilter oldobjs in GetComponentsInChildren<MeshFilter>())
            DestroyImmediate(oldobjs.gameObject);
        if (meshInstanceRenderer != null)
            meshInstanceRenderer.instances.Clear();

        if (paths == null || paths.Length == 0)
            paths = GetComponentsInChildren<LinePath>();
        foreach (LinePath path in paths)
        {
            if (path.cachedPoints.Count >= 2)
                AddFromPath(path);
        }

        if (meshInstanceRenderer != null)
            meshInstanceRenderer.Refresh();
        foreach (Decal decal in GetComponentsInChildren<Decal>())
            decal.Build();
    }

    bool AddAtDistance(LinePath path, float distance)
    {
        Vector3 position = path.GetPointAtDistance(distance).position;

        Vector3 forward;
        if (fenceMode)
        {
            if (distance + spacing + _forwardCheck > path.totalDistance)
                return true;

            Vector3 endpos = path.GetPointAtDistance(distance + spacing).position;
            position = (position + endpos) * 0.5f;
            forward = endpos - position;
        }
        else
            forward = (path.GetPointAtDistance(distance + _forwardCheck).position - position).normalized;

        // calculate random perpendicular offset
        Vector3 perp = Vector3.Cross(forward, Vector3.up);
        position += perp * Random.Range(minWidth, maxWidth);

        // snap to ground
        Vector3 up = Vector3.up;
        if (raycast)
        {
            float raycastsafecheck = 2;
            if (!Physics.Raycast(position + Vector3.up * raycastsafecheck, Vector3.down, out RaycastHit hit, maxRaycastDistance + raycastsafecheck, raycastLayerMask))
                return false;
            position = hit.point + hit.normal * groundOffset;
            if (normalUp)
                up = hit.normal;
        }
        else
            position.y += groundOffset;

        position += worldOffset;

        // rotation
        Vector3 extraangle = extraRotation;
        if (randomRotation)
            extraangle.y += Random.Range(0, 360);
        Quaternion rotation = Quaternion.LookRotation(forward, up) * Quaternion.Euler(extraangle);
        rotation *= Quaternion.Euler(Random.Range(-randomSlant, randomSlant), Random.Range(-randomSlant, randomSlant), 0);

        // scale
        Vector3 scale = scaleMultiplier * (objectSizeMultiplier * (1 + Random.Range(0, scaleRandomness)));

        // color
        Color color = UnityUtils.ColorLerp(minColor, maxColor, Random.value);
        if (copyColor != null && copyColor.instances.Count > 0)
        {
            float closestdist = float.MaxValue;
            Color closestcolor = Color.white;
            for (int i = 0; i < copyColor.instances.Count; ++i)
            {
                float dist = (position - copyColor.instances[i].position).sqrMagnitude;
                if (dist > closestdist)
                    continue;

                closestdist = dist;
                closestcolor = copyColor.instances[i].color;
            }
            color = UnityUtils.ColorLerp(color, color * closestcolor, copyColorAmount);
        }

        // instantiate
        if (prefabs != null && prefabs.Length > 0)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            if (prefab == null)
                return false;

            Transform objtransform = ((GameObject)PrefabUtility.InstantiatePrefab(prefab)).transform;
            objtransform.parent = transform;
            objtransform.SetPositionAndRotation(position, rotation);
            objtransform.localScale = objtransform.localScale.Multiply(scale);
        }
        if (meshInstanceRenderer != null)
        {
            meshInstanceRenderer.instances.Add(new MeshInstance()
            {
                position = position,
                rotation = rotation,
                scale = scale,
                color = color
            });
        }

        return false;
    }

    void AddFromPath(LinePath path)
    {
        Random.InitState(seed);

        if (onlyAtPoints)
        {
            for (int i = 0; i < path.cachedPoints.Count; ++i)
            {
                if (AddAtDistance(path, path.cachedPoints[i].distance))
                    break;
            }
        }
        else
        {
            for (float distance = startNormOffset * path.totalDistance; distance + _forwardCheck < path.totalDistance; distance += spacing)
            {
                // caculate point on line
                distance += Random.Range(-spacing, spacing) * 0.5f * offsetRandomness;
                distance = Mathf.Clamp(distance, 0, path.totalDistance);

                if (AddAtDistance(path, distance))
                    break;
            }

            if (forceEndPoint)
                AddAtDistance(path, path.totalDistance);
        }
    }
#endif
}
