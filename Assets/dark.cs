using UnityEngine;

public class DarkVisionController : MonoBehaviour
{
    [Header("Dark Vision Settings")]
    public float defaultBrightness = 0.05f;
    public float scanBrightness = 0.2f;
    
    [Header("Post-Processing References")]
    public Material postProcessMaterial;
    
    private bool scanActive = false;
    
    private void Start()
    {
        // Set default brightness
        UpdateBrightness(defaultBrightness);
    }
    
    public void SetScanActive(bool active)
    {
        scanActive = active;
        UpdateBrightness(active ? scanBrightness : defaultBrightness);
    }
    
    private void UpdateBrightness(float value)
    {
        if (postProcessMaterial != null)
        {
            postProcessMaterial.SetFloat("_Brightness", value);
        }
    }
    
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Apply post-processing effect if material exists
        if (postProcessMaterial != null)
        {
            Graphics.Blit(source, destination, postProcessMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}