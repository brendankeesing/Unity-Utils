using UnityEngine;

public class ObjectPainter : MonoBehaviour
{
    public GameObject prefab;
    public LayerMask layerMask = -1;
    public float minScale = 1;
    public float maxScale = 1;
    public float minHorizontalScale = 1;
    public float maxHorizontalScale = 1;
    public float minVerticalScale = 1;
    public float maxVerticalScale = 1;
    public float leanVariation = 0;
}
