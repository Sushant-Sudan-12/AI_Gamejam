using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public Transform player;
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float shootInterval = 2f;
    public float rotationSpeed = 5f;
    public float bulletSpeed = 10f;
    public float shootingRange = 15f; // NEW: max distance to shoot

    private float shootTimer;

    void Update()
    {
        if (player == null) return;

        // Rotate towards player
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        // Check distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= shootingRange)
        {
            // Handle shooting
            shootTimer += Time.deltaTime;
            if (shootTimer >= shootInterval)
            {
                Shoot();
                shootTimer = 0f;
            }
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection((player.position - shootPoint.position).normalized, bulletSpeed);
        }
    }
}
