using UnityEngine;

public static class OBJExporter
{
    public static bool ExportAsOBJ(string filename, Mesh mesh)
    {
        try
        {
            System.IO.File.WriteAllText(filename, MeshToOBJ(mesh));
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
        return false;
    }

    public static string MeshToOBJ(Mesh mesh)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        Vector3[] vertices = mesh.vertices;
        sb.AppendLine("# Vertices");
        foreach (Vector3 vertex in vertices)
            sb.AppendFormat("v {0} {1} {2}\n", vertex.x, vertex.y, vertex.z);

        sb.AppendLine("\n# Normals");
        Vector3[] noramls = mesh.normals;
        foreach (Vector3 normal in noramls)
            sb.AppendFormat("vn {0} {1} {2}\n", normal.x, normal.y, normal.z);

        sb.AppendLine("\n# TexCoords");
        Vector2[] uvs = mesh.uv;
        foreach (Vector2 uv in uvs)
            sb.AppendFormat("vt {0} {1}\n", uv.x, uv.y);

        sb.AppendLine("\n# Triangles");
        int[] indices = mesh.triangles;
        for (int i = 0; i < indices.Length / 3; ++i)
            sb.AppendFormat("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", indices[i * 3 + 0] + 1, indices[i * 3 + 1] + 1, indices[i * 3 + 2] + 1);

        return sb.ToString();
    }
}
