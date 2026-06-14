using UnityEngine;

[System.Serializable]
public struct LayerCullSetting
{
    public string layerName;
    [Tooltip("Bu mesafeden uzaktaki objeler çizilmez")]
    public float distance;
}

[RequireComponent(typeof(Camera))]
public class PerLayerCulling : MonoBehaviour
{
    public LayerCullSetting[] settings;
    public bool sphericalCulling = true;

    void Start()
    {
        Apply();
    }

    public void Apply()
    {
        Camera cam = GetComponent<Camera>();
        float[] distances = new float[32];

        foreach (var s in settings)
        {
            int layer = LayerMask.NameToLayer(s.layerName);
            if (layer != -1)
                distances[layer] = s.distance;
        }

        cam.layerCullDistances = distances;
        cam.layerCullSpherical = sphericalCulling;
    }
}