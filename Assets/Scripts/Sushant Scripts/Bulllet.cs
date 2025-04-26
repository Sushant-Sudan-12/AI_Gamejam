using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    public float lifetime = 5f; // Bullet auto destroys after some seconds

    void Start()
    {
        Destroy(gameObject, lifetime); // Prevent bullets living forever
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    public void SetDirection(Vector3 dir, float spd)
    {
        direction = dir;
        speed = spd;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Bullet hit: " + other.name); // See what it hits

        if (other.CompareTag("Player"))
        {
            FirstPersonController player = other.GetComponent<FirstPersonController>();
            if (player != null)
            {
                player.Die(); // Call die
            }
            Destroy(gameObject); // Destroy bullet after hitting player
        }
        else if (!other.CompareTag("Enemy")) // So bullet doesn't break when touching enemy
        {
            Destroy(gameObject);
        }
    }

}
