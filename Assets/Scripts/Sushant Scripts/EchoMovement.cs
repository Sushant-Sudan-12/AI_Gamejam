using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SonarVision : MonoBehaviour
{
    [Header("Sonar Settings")]
    public float maxSonarDistance = 30f;
    public float sonarSpeed = 15f;
    public float sonarDuration = 3f;
    public float cooldown = 1.5f;
    public int numberOfRays = 300;

    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color enemyColor = Color.red;
    public float lineThickness = 0.05f;
    public float lineFadeSpeed = 0.5f;

    [Header("References")]
    public Transform sonarOrigin;
    public Material sonarLineMaterial;
    
    private bool canScan = true;
    private List<SonarLine> activeLines = new List<SonarLine>();
    private Dictionary<GameObject, float> highlightedObjects = new Dictionary<GameObject, float>();
    
    // Class to track each sonar line
    private class SonarLine
    {
        public LineRenderer lineRenderer;
        public Vector3 direction;
        public float distance;
        public float progress;
        public float maxDistance;
        public bool hitEnemy;
        public float alpha;
        
        public SonarLine(LineRenderer lr, Vector3 dir, float maxDist)
        {
            lineRenderer = lr;
            direction = dir;
            maxDistance = maxDist;
            progress = 0f;
            distance = maxDist;
            hitEnemy = false;
            alpha = 1f;
        }
    }

    void Update()
    {
        // Input to trigger sonar
        if (Input.GetKeyDown(KeyCode.Q) && canScan)
        {
            StartCoroutine(EmitSonar());
        }
        
        // Update all active lines
        UpdateSonarLines();
        
        // Update highlighted objects
        UpdateHighlightedObjects();
    }
    
    IEnumerator EmitSonar()
    {
        canScan = false;
        
        // Generate directions for the rays
        List<Vector3> directions = GenerateDirections(numberOfRays);
        
        // Create line renderers for each direction
        foreach (Vector3 direction in directions)
        {
            // Create a new GameObject for the line
            GameObject lineObj = new GameObject("SonarLine");
            lineObj.transform.SetParent(transform);
            
            // Add LineRenderer
            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(sonarLineMaterial);
            lineRenderer.startWidth = lineThickness;
            lineRenderer.endWidth = lineThickness;
            lineRenderer.positionCount = 2;
            
            // Set initial positions
            Vector3 startPos = sonarOrigin != null ? sonarOrigin.position : transform.position;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, startPos);
            
            // Set initial color
            lineRenderer.startColor = normalColor;
            lineRenderer.endColor = normalColor;
            
            // Create and track the sonar line
            SonarLine sonarLine = new SonarLine(lineRenderer, direction, maxSonarDistance);
            activeLines.Add(sonarLine);
        }
        
        // Wait for the sonar effect to complete
        yield return new WaitForSeconds(sonarDuration + cooldown);
        
        canScan = true;
    }
    
    void UpdateSonarLines()
    {
        List<SonarLine> linesToRemove = new List<SonarLine>();
        
        foreach (SonarLine line in activeLines)
        {
            // Update line position based on progress
            if (line.progress < 1f)
            {
                line.progress += Time.deltaTime * sonarSpeed / line.maxDistance;
                
                Vector3 startPos = sonarOrigin != null ? sonarOrigin.position : transform.position;
                
                // Calculate the current position along the ray
                float currentDistance = line.progress * line.distance;
                Vector3 currentPos = startPos + line.direction * currentDistance;
                
                // Perform an actual raycast to detect objects
                RaycastHit hit;
                if (Physics.Raycast(startPos, line.direction, out hit, currentDistance))
                {
                    // Check if we hit an enemy
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        line.hitEnemy = true;
                        line.lineRenderer.startColor = line.lineRenderer.endColor = enemyColor;
                        
                        // Add enemy to highlighted objects
                        if (!highlightedObjects.ContainsKey(hit.collider.gameObject))
                        {
                            highlightedObjects.Add(hit.collider.gameObject, sonarDuration);
                        }
                    }
                    
                    // Update the line end position to the hit point
                    line.lineRenderer.SetPosition(1, hit.point);
                    
                    // Adjust distance to hit position
                    if (line.distance > hit.distance)
                    {
                        line.distance = hit.distance;
                    }
                }
                else
                {
                    // No hit, extend line to current calculated position
                    line.lineRenderer.SetPosition(1, currentPos);
                }
            }
            else
            {
                // Fade out line after it's completed
                line.alpha -= Time.deltaTime * lineFadeSpeed;
                
                Color startColor = line.hitEnemy ? enemyColor : normalColor;
                Color endColor = startColor;
                startColor.a = line.alpha;
                endColor.a = line.alpha;
                
                line.lineRenderer.startColor = startColor;
                line.lineRenderer.endColor = endColor;
                
                // Remove line when completely faded
                if (line.alpha <= 0)
                {
                    linesToRemove.Add(line);
                }
            }
        }
        
        // Clean up faded lines
        foreach (SonarLine line in linesToRemove)
        {
            if (line.lineRenderer != null)
            {
                Destroy(line.lineRenderer.gameObject);
            }
            activeLines.Remove(line);
        }
    }
    
    void UpdateHighlightedObjects()
    {
        List<GameObject> objectsToRemove = new List<GameObject>();
        
        foreach (var kvp in highlightedObjects)
        {
            GameObject obj = kvp.Key;
            float timeRemaining = kvp.Value - Time.deltaTime;
            
            if (timeRemaining <= 0 || obj == null)
            {
                objectsToRemove.Add(obj);
            }
            else
            {
                highlightedObjects[obj] = timeRemaining;
                
                // Here you would apply a highlight shader or effect to the object
                // For example:
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Apply highlight effect
                    // This is a placeholder - implement your own highlight method
                    ApplyHighlight(renderer, timeRemaining / sonarDuration);
                }
            }
        }
        
        foreach (GameObject obj in objectsToRemove)
        {
            highlightedObjects.Remove(obj);
            
            // Remove highlight effect
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    RemoveHighlight(renderer);
                }
            }
        }
    }
    
    List<Vector3> GenerateDirections(int count)
    {
        List<Vector3> directions = new List<Vector3>();
        
        // Golden spiral method for even distribution over a sphere
        float phi = Mathf.PI * (3 - Mathf.Sqrt(5)); // Golden angle in radians
        
        for (int i = 0; i < count; i++)
        {
            float y = 1 - (i / (float)(count - 1)) * 2; // y goes from 1 to -1
            float radius = Mathf.Sqrt(1 - y * y); // Radius at y
            
            float theta = phi * i; // Golden angle increment
            
            float x = Mathf.Cos(theta) * radius;
            float z = Mathf.Sin(theta) * radius;
            
            Vector3 direction = new Vector3(x, y, z).normalized;
            directions.Add(direction);
        }
        
        return directions;
    }
    
    void ApplyHighlight(Renderer renderer, float intensity)
    {
        // Placeholder for highlight effect
        // You might want to use a special material or shader property
        if (renderer.material.HasProperty("_EmissionColor"))
        {
            renderer.material.SetColor("_EmissionColor", enemyColor * intensity);
            renderer.material.EnableKeyword("_EMISSION");
        }
    }
    
    void RemoveHighlight(Renderer renderer)
    {
        // Remove highlight effect
        if (renderer.material.HasProperty("_EmissionColor"))
        {
            renderer.material.SetColor("_EmissionColor", Color.black);
            renderer.material.DisableKeyword("_EMISSION");
        }
    }
}