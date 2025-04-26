using UnityEngine;

public class DarknessController : MonoBehaviour
{
    [Header("Darkness Settings")]
    public Color ambientDarkness = Color.black;
    public float maxLightIntensity = 0.1f;
    
    [Header("References")]
    public Light mainDirectionalLight;
    
    private Color originalAmbientColor;
    private float originalLightIntensity;
    
    void Start()
    {
        // Store original lighting settings
        originalAmbientColor = RenderSettings.ambientLight;
        originalLightIntensity = mainDirectionalLight != null ? mainDirectionalLight.intensity : 0f;
        
        // Apply darkness
        EnableDarkness();
    }
    
    public void EnableDarkness()
    {
        // Set ambient lighting to near-black
        RenderSettings.ambientLight = ambientDarkness;
        
        // Dim directional light if it exists
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.intensity = maxLightIntensity;
        }
        
        // Find and reduce all other lights in the scene
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (Light light in allLights)
        {
            if (light != mainDirectionalLight)
            {
                light.intensity *= 0.1f;
                light.range *= 0.5f;
            }
        }
        
        // Set fog to enhance darkness feeling
        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fogDensity = 0.05f;
    }
    
    public void DisableDarkness()
    {
        // Restore lighting
        RenderSettings.ambientLight = originalAmbientColor;
        
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.intensity = originalLightIntensity;
        }
        
        // Disable fog
        RenderSettings.fog = false;
    }
}