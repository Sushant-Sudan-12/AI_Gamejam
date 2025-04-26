using UnityEngine;
using System.Collections.Generic;

public class EchoLocationPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseSpeed = 10f;
    public float pulseMaxRadius = 50f;
    public float pulseWidth = 1.5f;
    public float cooldownTime = 2f;
    public KeyCode pulseKey = KeyCode.Q;
    
    [Header("Visual Settings")]
    public Material pulseMaterial;
    public Color normalColor = Color.white;
    public Color enemyColor = Color.red;
    
    private bool isPulsing = false;
    private float pulseRadius = 0f;
    private float cooldownTimer = 0f;
    private List<GameObject> detectedEnemies = new List<GameObject>();
    
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Set up camera for depth texture
        mainCamera.depthTextureMode = DepthTextureMode.Depth;
        
        // Initialize shader properties
        pulseMaterial.SetFloat("_PulseRadius", 0);
        pulseMaterial.SetFloat("_PulseWidth", pulseWidth);
        pulseMaterial.SetColor("_PulseColor", normalColor);
        pulseMaterial.SetColor("_EnemyColor", enemyColor);
    }
    
    void Update()
    {
        // Handle cooldown
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        
        // Start a new pulse when Q is pressed
        if (Input.GetKeyDown(pulseKey) && cooldownTimer <= 0 && !isPulsing)
        {
            StartPulse();
        }
        
        // Update pulse progress
        if (isPulsing)
        {
            UpdatePulse();
        }
    }
    
    void StartPulse()
    {
        isPulsing = true;
        pulseRadius = 0f;
        detectedEnemies.Clear();
        
        // Set player position in shader
        pulseMaterial.SetVector("_PlayerPosition", transform.position);
    }
    
    void UpdatePulse()
    {
        // Expand pulse radius
        pulseRadius += pulseSpeed * Time.deltaTime;
        pulseMaterial.SetFloat("_PulseRadius", pulseRadius);
        
        // Detect enemies hit by the pulse
        DetectEnemies();
        
        // End pulse when it reaches max radius
        if (pulseRadius >= pulseMaxRadius)
        {
            isPulsing = false;
            pulseRadius = 0f;
            pulseMaterial.SetFloat("_PulseRadius", 0);
            cooldownTimer = cooldownTime;
        }
    }
    
    void DetectEnemies()
    {
        // Get all enemies in the scene
        float innerRadius = pulseRadius - pulseWidth;
        float outerRadius = pulseRadius;
        
        if (innerRadius < 0) innerRadius = 0;
        
        // Find all objects with "Enemy" tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            
            // Check if enemy is in the pulse wave
            if (distance >= innerRadius && distance <= outerRadius)
            {
                if (!detectedEnemies.Contains(enemy))
                {
                    detectedEnemies.Add(enemy);
                    
                    // Update enemy position array in shader
                    UpdateEnemyPositionsInShader();
                }
            }
        }
    }
    
    void UpdateEnemyPositionsInShader()
    {
        // Maximum number of enemies we'll track in the shader
        const int MAX_ENEMIES = 20;
        
        Vector4[] enemyPositions = new Vector4[MAX_ENEMIES];
        
        // Fill array with detected enemy positions
        for (int i = 0; i < MAX_ENEMIES; i++)
        {
            if (i < detectedEnemies.Count)
            {
                enemyPositions[i] = detectedEnemies[i].transform.position;
            }
            else
            {
                // Use a far away position for unused slots
                enemyPositions[i] = new Vector4(10000, 10000, 10000, 0);
            }
        }
        
        // Set enemy positions and count in shader
        pulseMaterial.SetVectorArray("_EnemyPositions", enemyPositions);
        pulseMaterial.SetInt("_EnemyCount", Mathf.Min(detectedEnemies.Count, MAX_ENEMIES));
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (isPulsing)
        {
            // Apply pulse effect
            Graphics.Blit(source, destination, pulseMaterial);
        }
        else
        {
            // Just render the dark environment
            Graphics.Blit(source, destination);
        }
    }
}