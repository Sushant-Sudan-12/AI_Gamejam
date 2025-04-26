using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int width = 20;
    public int height = 20;
    public float tileSize = 3f;  // Size of one prefab (3x3 units)

    [Header("Prefabs")]
    public GameObject wallUnit0;   // Normal rotation
    public GameObject wallUnit90;  // 90 degree rotated version

    [Header("Tile Options")]
    [Range(0f, 1f)]
    public float wallProbability = 0.8f; // 80% chance wall, 20% corridor (empty)

    [Header("Random Seed (Optional)")]
    public bool useRandomSeed = true;
    public int seed = 12345;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        if (!useRandomSeed)
            Random.InitState(seed);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 spawnPosition = new Vector3(x * tileSize, 0, z * tileSize);

                if (Random.value > wallProbability)
                {
                    // Leave empty for corridor
                    continue;
                }

                // Randomly pick 0 or 90 degree wall
                GameObject selectedPrefab = (Random.value < 0.5f) ? wallUnit0 : wallUnit90;

                Instantiate(selectedPrefab, spawnPosition, Quaternion.identity, transform);
            }
        }
    }
}
