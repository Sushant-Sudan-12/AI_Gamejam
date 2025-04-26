using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StylizedEchoVision : MonoBehaviour
{
    [Header("Echo Settings")]
    public float maxEchoDistance = 25f;
    public float echoSpeed = 30f;
    public float highlightDuration = 4f;
    public float cooldown = 1.2f;
    public int numberOfRays = 50;  // Fewer, more impactful rays
    
    [Header("Visual Settings")]
    public Color normalColor = new Color(0.5f, 0.8f, 1f, 1f);  // Cyan-ish blue
    public Color enemyColor = new Color(1f, 0.2f, 0.2f, 1f);   // Bright red
    public float outlineWidth = 0.08f;
    public float pulseFrequency = 3f;  // How fast outlines pulse
    public float pulseIntensity = 0.4f;  // How strong the pulse effect is
    
    [Header("Ray Visual Effects")]
    public GameObject rayPrefab;  // A stylized ray visual
    public float rayWidth = 0.1f;
    public float rayFadeSpeed = 2f;
    public Color rayColor = new Color(0.7f, 0.9f, 1f, 0.8f);
    public bool useTrails = true;
    
    [Header("References")]
    public Transform echoOrigin;
    public Material outlineMaterial;
    public AudioClip echoSound;
    
    private bool canScan = true;
    private Dictionary<Renderer, MaterialData> highlightedObjects = new Dictionary<Renderer, MaterialData>();
    private List<EchoRayVisual> activeRays = new List<EchoRayVisual>();
    
    // Structure to store material data
    private class MaterialData
    {
        public Material originalMaterial;
        public Material outlineMaterial;
        public float timeRemaining;
        public bool isEnemy;
        public float pulseOffset;
        
        public MaterialData(Material original, Material outline, float time, bool enemy)
        {
            originalMaterial = original;
            outlineMaterial = outline;
            timeRemaining = time;
            isEnemy = enemy;
            pulseOffset = Random.Range(0f, Mathf.PI * 2);  // Random start phase for the pulse
        }
    }
    
    // Structure for visual rays
    private class EchoRayVisual
    {
        public GameObject rayObject;
        public Vector3 direction;
        public float distance;
        public float speed;
        public float lifeTime;
        public TrailRenderer trail;
        
        public EchoRayVisual(GameObject obj, Vector3 dir, float spd)
        {
            rayObject = obj;
            direction = dir;
            distance = 0f;
            speed = spd;
            lifeTime = 0f;
            trail = obj.GetComponent<TrailRenderer>();
        }
    }
    
    void Update()
    {
        // Input to trigger echo
        if (Input.GetKeyDown(KeyCode.Q) && canScan)
        {
            StartCoroutine(EmitStylizedEcho());
        }
        
        // Update active visual rays
        UpdateRayVisuals();
        
        // Update highlighted objects with pulse effect
        UpdateHighlightedObjectsWithPulse();
    }
    
    IEnumerator EmitStylizedEcho()
    {
        canScan = false;
        
        // Play sound effect
        if (echoSound != null)
        {
            AudioSource.PlayClipAtPoint(echoSound, transform.position, 0.8f);
        }
        
        // Generate directions - more focused in horizontal plane for better gameplay visibility
        List<Vector3> directions = GenerateStylizedDirections(numberOfRays);
        
        // Create visual rays
        foreach (Vector3 dir in directions)
        {
            CreateVisualRay(dir);
        }
        
        // Wait for cooldown
        yield return new WaitForSeconds(cooldown);
        
        canScan = true;
    }
    
    void CreateVisualRay(Vector3 direction)
    {
        if (rayPrefab == null)
        {
            // Create a simple ray with line renderer if no prefab
            GameObject rayObj = new GameObject("EchoRay");
            rayObj.transform.position = echoOrigin.position;
            
            LineRenderer line = rayObj.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, echoOrigin.position);
            line.SetPosition(1, echoOrigin.position);
            line.startWidth = rayWidth;
            line.endWidth = rayWidth * 0.5f;
            line.material = new Material(Shader.Find("Particles/Standard Unlit"));
            line.material.SetColor("_TintColor", rayColor);
            
            TrailRenderer trail = null;
            if (useTrails)
            {
                trail = rayObj.AddComponent<TrailRenderer>();
                trail.time = 0.5f;
                trail.startWidth = rayWidth * 0.8f;
                trail.endWidth = 0f;
                trail.material = line.material;
            }
            
            // Create tracker
            EchoRayVisual rayVisual = new EchoRayVisual(rayObj, direction, echoSpeed * Random.Range(0.8f, 1.2f));
            rayVisual.trail = trail;
            activeRays.Add(rayVisual);
        }
        else
        {
            // Use the provided prefab
            GameObject rayObj = Instantiate(rayPrefab, echoOrigin.position, Quaternion.LookRotation(direction));
            rayObj.transform.parent = transform;
            
            // Get trail if it exists
            TrailRenderer trail = rayObj.GetComponent<TrailRenderer>();
            
            // Create tracker
            EchoRayVisual rayVisual = new EchoRayVisual(rayObj, direction, echoSpeed * Random.Range(0.8f, 1.2f));
            rayVisual.trail = trail;
            activeRays.Add(rayVisual);
        }
    }
    
    void UpdateRayVisuals()
    {
        List<EchoRayVisual> raysToRemove = new List<EchoRayVisual>();
        
        foreach (EchoRayVisual ray in activeRays)
        {
            // Update ray lifetime and position
            ray.lifeTime += Time.deltaTime;
            ray.distance += Time.deltaTime * ray.speed;
            
            if (ray.rayObject == null)
            {
                raysToRemove.Add(ray);
                continue;
            }
            
            // Check if ray has reached maximum distance
            if (ray.distance > maxEchoDistance || ray.lifeTime > maxEchoDistance / ray.speed * 1.5f)
            {
                // Fade out and destroy
                Destroy(ray.rayObject, 0.5f);
                raysToRemove.Add(ray);
                continue;
            }
            
            // Update ray position
            Vector3 newPosition = echoOrigin.position + ray.direction * ray.distance;
            
            // Cast ray to detect objects
            RaycastHit hit;
            if (Physics.Raycast(echoOrigin.position, ray.direction, out hit, ray.distance))
            {
                // Update ray end position to hit point
                newPosition = hit.point;
                
                // Process hit object for outline if not already
                ProcessHitObject(hit);
                
                // Stop the ray
                if (ray.trail != null)
                {
                    // Let trail continue but detach from the ray object
                    ray.trail.transform.parent = null;
                    ray.trail = null;
                }
                
                // Mark for removal
                Destroy(ray.rayObject, 0.2f);
                raysToRemove.Add(ray);
            }
            else
            {
                // Move ray forward
                ray.rayObject.transform.position = newPosition;
            }
            
            // Update line renderer if present
            LineRenderer line = ray.rayObject.GetComponent<LineRenderer>();
            if (line != null)
            {
                line.SetPosition(0, echoOrigin.position);
                line.SetPosition(1, newPosition);
                
                // Fade line alpha over time for dramatic effect
                Color currentColor = rayColor;
                currentColor.a = Mathf.Lerp(rayColor.a, 0, ray.lifeTime / (maxEchoDistance / ray.speed));
                line.material.SetColor("_TintColor", currentColor);
            }
        }
        
        // Remove completed rays
        foreach (EchoRayVisual ray in raysToRemove)
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
        Color baseColor = isEnemy ? enemyColor : normalColor;
        outlineMat.SetColor("_OutlineColor", baseColor);
        outlineMat.SetFloat("_OutlineWidth", outlineWidth);
        
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
    
    void UpdateHighlightedObjectsWithPulse()
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
            
            // Update highlight intensity based on remaining time with pulse effect
            float baseIntensity = Mathf.Clamp01(data.timeRemaining / highlightDuration);
            
            // Add pulsing effect
            float pulseValue = Mathf.Sin((Time.time * pulseFrequency) + data.pulseOffset) * pulseIntensity + 1f;
            float finalIntensity = baseIntensity * pulseValue;
            
            // Apply to material
            data.outlineMaterial.SetFloat("_OutlineIntensity", finalIntensity);
            
            // Update glow color based on pulse as well
            Color baseColor = data.isEnemy ? enemyColor : normalColor;
            Color pulseColor = baseColor * pulseValue;
            data.outlineMaterial.SetColor("_OutlineColor", pulseColor);
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
    
    List<Vector3> GenerateStylizedDirections(int count)
    {
        List<Vector3> directions = new List<Vector3>();
        
        // Generate more interesting pattern that favors horizontal spread
        // with some vertical variance for dramatic effect
        for (int i = 0; i < count; i++)
        {
            float horizontalAngle = Random.Range(0f, Mathf.PI * 2);
            float verticalAngle = Random.Range(-0.5f, 0.5f); // Mostly horizontal
            
            // Convert to Cartesian coordinates
            float x = Mathf.Cos(horizontalAngle) * Mathf.Cos(verticalAngle);
            float y = Mathf.Sin(verticalAngle);
            float z = Mathf.Sin(horizontalAngle) * Mathf.Cos(verticalAngle);
            
            directions.Add(new Vector3(x, y, z).normalized);
        }
        
        return directions;
    }
}