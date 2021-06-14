using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class RotateUVs : BaseMeshEffect
{
    public float angle = 0;

    public override void ModifyMesh(VertexHelper vh)
    {
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.back);

        UIVertex vertex = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; ++i)
        {
            vh.PopulateUIVertex(ref vertex, i);
            Vector2 uv = vertex.uv0;
            uv.x -= 0.5f;
            uv.y -= 0.5f;
            uv = rotation * uv;
            uv.x += 0.5f;
            uv.y += 0.5f;
            vertex.uv0 = uv;
            vh.SetUIVertex(vertex, i);
        }
    }
}
