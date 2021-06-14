using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MeshUI : MaskableGraphic
{
    public Mesh mesh;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale = Vector3.one;
    public bool sortTriangles = false;

    struct Triangle
    {
        public int index;
        public float distance;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        vh.Clear();

        if (mesh == null)
            return;

        Rect rect = GetComponent<RectTransform>().rect;
        Vector3 offset = new Vector3(Mathf.LerpUnclamped(rect.xMin, rect.xMax, position.x * 0.5f + 0.5f), Mathf.LerpUnclamped(rect.yMin, rect.yMax, position.y * 0.5f + 0.5f), position.z);
        Vector3 finalscale = new Vector3(scale.x * rect.width, scale.y * rect.height, scale.z * rect.height);
        Matrix4x4 matrix = Matrix4x4.TRS(offset, Quaternion.Euler(rotation), finalscale);

        int[] indices = mesh.triangles;
        Vector3[] positions = mesh.vertices;
        Color[] colors = mesh.colors;
        if (colors.Length == 0)
            colors = null;
        Vector2[] uvs = mesh.uv;
        if (uvs.Length == 0)
            uvs = null;

        if (sortTriangles)
        {
            Triangle[] triangles = new Triangle[indices.Length / 3];
            for (int i = 0; i < triangles.Length; ++i)
            {
                int index = i * 3;
                triangles[i].index = i;
                triangles[i].distance = (positions[indices[index]].z + positions[indices[index + 1]].z + positions[indices[index + 2]].z) / 3;
            }
            System.Array.Sort(triangles, (a, b) => b.distance.CompareTo(a.distance));
            int[] newindices = new int[indices.Length];
            for (int i = 0; i < triangles.Length; ++i)
            {
                int index = i * 3;
                newindices[index + 0] = indices[triangles[i].index * 3 + 0];
                newindices[index + 1] = indices[triangles[i].index * 3 + 1];
                newindices[index + 2] = indices[triangles[i].index * 3 + 2];
            }
            indices = newindices;
        }

        for (int i = 0; i < positions.Length; ++i)
        {
            vh.AddVert(matrix.MultiplyPoint3x4(positions[i]), colors != null ? colors[i] : Color.white, uvs != null ? uvs[i] : Vector2.zero);
        }

        for (int i = 0; i < indices.Length; i += 3)
            vh.AddTriangle(indices[i], indices[i + 1], indices[i + 2]);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(0, 2, 3);
    }
}
