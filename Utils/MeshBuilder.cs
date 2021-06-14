using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder
{
    List<Vector3> _positions = new List<Vector3>();
    List<Vector2> _uvs = new List<Vector2>();
    List<Vector3> _normals = new List<Vector3>();
    public int vertexCount => _positions.Count;
    List<int> _indices = new List<int>();
    public int indexCound => _indices.Count;

    public void AddVertex(Vector3 pos, Vector2 uv)
    {
        AddVertex(pos, uv, Vector3.zero);
    }

    public void AddVertex(Vector3 pos, Vector2 uv, Vector3 normal)
    {
        _positions.Add(pos);
        _uvs.Add(uv);
        _normals.Add(normal);
    }

    public void AddTriangle(int a, int b, int c)
    {
        _indices.Add(a);
        _indices.Add(b);
        _indices.Add(c);
    }

    public void AddPlane(Vector3 center, Vector2 size, Vector3 normal, Vector3 up, Vector2 uvscale)
    {
        AddTriangle(vertexCount + 0, vertexCount + 3, vertexCount + 2);
        AddTriangle(vertexCount + 0, vertexCount + 2, vertexCount + 1);

        size *= 0.5f;
        up = up.normalized * size.y;
        Vector3 right = Vector3.Cross(normal, up).normalized * size.x;

        AddVertex(center - right - up, new Vector2(0, 0), normal);
        AddVertex(center + right - up, new Vector2(uvscale.x, 0), normal);
        AddVertex(center + right + up, new Vector2(uvscale.x, uvscale.y), normal);
        AddVertex(center - right + up, new Vector2(0, uvscale.y), normal);
    }

    public void AddCube(Vector3 center, Vector3 size)
    {
        Vector3 uvsize = size;
        size *= 0.5f;
        AddPlane(new Vector3(center.x - size.x, center.y, center.z), new Vector2(uvsize.z, uvsize.y), Vector3.left, Vector3.up, Vector2.one);
        AddPlane(new Vector3(center.x + size.x, center.y, center.z), new Vector2(uvsize.z, uvsize.y), Vector3.right, Vector3.up, Vector2.one);
        AddPlane(new Vector3(center.x, center.y - size.y, center.z), new Vector2(uvsize.x, uvsize.z), Vector3.down, Vector3.forward, Vector2.one);
        AddPlane(new Vector3(center.x, center.y + size.y, center.z), new Vector2(uvsize.x, uvsize.z), Vector3.up, Vector3.forward, Vector2.one);
        AddPlane(new Vector3(center.x, center.y, center.z - size.z), new Vector2(uvsize.x, uvsize.y), Vector3.back, Vector3.up, Vector2.one);
        AddPlane(new Vector3(center.x, center.y, center.z + size.z), new Vector2(uvsize.x, uvsize.y), Vector3.forward, Vector3.up, Vector2.one);
    }

    public Mesh GenerateMesh(bool recalculatenormals, bool marknolongerreadable, Mesh mesh = null)
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "MeshBuilder";
        }

        mesh.SetVertices(_positions);
        mesh.SetUVs(0, _uvs);
        mesh.SetNormals(_normals);
        mesh.SetIndices(_indices, MeshTopology.Triangles, 0);
        if (recalculatenormals)
            mesh.RecalculateNormals();
        mesh.Optimize();
        mesh.UploadMeshData(marknolongerreadable);
        return mesh;
    }
}