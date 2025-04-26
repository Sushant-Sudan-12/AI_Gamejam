using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EchoVisionController : MonoBehaviour
{
    [Header("Echo Settings")]
    public float maxEchoDistance = 20f;
    public float echoSpeed = 15f;
    public float highlightDuration = 3f;
    public float cooldown = 1.5f;
    public int numberOfRays = 300;
    
    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color enemyColor = Color.red;
    public float outlineWidth = 0.05f;
    
    [Header("References")]
    public Transform echoOrigin;
    public Material outlineMaterial;
    
    private bool canScan = true;
    private Dictionary<Renderer, MaterialData> highlightedObjects = new Dictionary<Renderer, MaterialData>();
    private List<EchoRay> activeRays = new List<EchoRay>();
    
    // Structure to store original material data
    private class MaterialData
    {
        public Material originalMaterial;
        public Material outlineMaterial;
        public float timeRemaining;
        public bool isEnemy;
        
        public MaterialData(Material original, Material outline, float time, bool enemy)
        {
            originalMaterial = original;
            outlineMaterial = outline;
            timeRemaining = time;
            isEnemy = enemy;
        }
    }
    
    // Structure to track echo rays
    private class EchoRay
    {
        public Vector3 origin;
        public Vector3 direction;
        public float distance;
        public float progress;
        
        public EchoRay(Vector3 o, Vector3 dir)
        {
            origin = o;
            direction = dir;
            distance = 0f;
            progress = 0f;
        }
    }
    
    void Update()
    {
        // Input to trigger echo
        if (Input.GetKeyDown(KeyCode.Q) && canScan)
        {
            StartCoroutine(EmitEcho());
        }
        
        // Update active echo rays
        UpdateEchoRays();
        
        // Update highlighted objects
        UpdateHighlightedObjects();
    }
    
    IEnumerator EmitEcho()
    {
        canScan = false;
        
        // Generate rays in different directions
        List<Vector3> directions = GenerateDirections(numberOfRays);
        
        // Create echo rays
        foreach (Vector3 dir in directions)
        {
            EchoRay ray = new EchoRay(
                echoOrigin != null ? echoOrigin.position : transform.position,
                dir
            );
            activeRays.Add(ray);
        }
        
        // Wait for cooldown
        yield return new WaitForSeconds(cooldown);
        
        canScan = true;
    }
    
    void UpdateEchoRays()
    {
        List<EchoRay> raysToRemove = new List<EchoRay>();
        
        foreach (EchoRay ray in activeRays)
        {
            // Update ray progress
            ray.progress += Time.deltaTime * echoSpeed;
            float currentDistance = ray.progress;
            
            // Check if ray has reached maximum distance
            if (currentDistance > maxEchoDistance)
            {
                raysToRemove.Add(ray);
                continue;
            }
            
            // Cast ray to detect objects
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, currentDistance))
            {
                // If we hit something for the first time
                if (ray.distance == 0)
                {
                    ray.distance = hit.distance;
                    ProcessHitObject(hit);
                }
                
                // We've already registered this hit, so we can remove the ray
                if (ray.distance > 0 && currentDistance > ray.distance)
                {
                    raysToRemove.Add(ray);
                }
            }
        }
        
        // Remove completed rays
        foreach (EchoRay ray in raysToRemove)
        {
            activeRays.Remove(ray);
        }
    }
    
    void ProcessHitObject(RaycastHit hit)
    {
        Renderer renderer = hit.collider.GetComponent<Renderer>();
        if (renderer == null || !renderer.enabled)
            return;
            
        // Skip if already highlighted
        if (highlightedObjects.ContainsKey(renderer))
            return;
            
        // Create outline material instance
        Material outlineMat = new Material(outlineMaterial);
        
        // Check if enemy
        bool isEnemy = hit.collider.CompareTag("Enemy");
        
        // Set color based on enemy status
        outlineMat.SetColor("_OutlineColor", isEnemy ? enemyColor : normalColor);
        
        // Store original material and create data
        MaterialData data = new MaterialData(
            renderer.material,
            outlineMat,
            highlightDuration,
            isEnemy
        );
        
        // Apply outline material and store data
        renderer.material = outlineMat;
        highlightedObjects.Add(renderer, data);
    }
    
    void UpdateHighlightedObjects()
    {
        List<Renderer> objectsToRemove = new List<Renderer>();
        
        foreach (var entry in highlightedObjects)
        {
            Renderer renderer = entry.Key;
            MaterialData data = entry.Value;
            
            // Update time remaining
            data.timeRemaining -= Time.deltaTime;
            
            // Check if highlight should be removed
            if (data.timeRemaining <= 0 || renderer == null)
            {
                objectsToRemove.Add(renderer);
                continue;
            }
            
            // Update highlight intensity based on remaining time
            float intensity = Mathf.Clamp01(data.timeRemaining / highlightDuration);
            data.outlineMaterial.SetFloat("_OutlineIntensity", intensity);
        }
        
        // Remove expired highlights
        foreach (Renderer renderer in objectsToRemove)
        {
            if (renderer != null)
            {
                // Restore original material
                renderer.material = highlightedObjects[renderer].originalMaterial;
            }
            
            highlightedObjects.Remove(renderer);
        }
    }
    
    List<Vector3> GenerateDirections(int count)
    {
        List<Vector3> directions = new List<Vector3>();
        
        // Generate directions evenly distributed over a sphere
        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;
        
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;
            
            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);
            
            directions.Add(new Vector3(x, y, z).normalized);
        }
        
        return directions;
    }
}