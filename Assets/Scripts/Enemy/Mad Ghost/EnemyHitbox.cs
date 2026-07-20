using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public EnemyData data; // Tham chiếu tới ScriptableObject để lấy sát thương

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsValidDamageableTarget(collision))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(data.damage);
                Debug.Log("Gây sát thương: " + data.damage);
            }
        }
    }

    private bool IsValidDamageableTarget(Collider2D other)
    {
        // 1. Strict Tag Check
        if (!other.CompareTag("Player")) return false;

        // 2. Strict Layer Check (Player layer is index 8)
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return false;

        // 3. Ignore trigger colliders (like GroundCheck or WallCheck) to prevent false-positives
        if (other.isTrigger) return false;

        // 4. Verify presence of valid attached Rigidbody2D of Dynamic body type
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic) return false;

        // 5. Verify PlayerHealth component exists on the target
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null) return false;

        return true;
    }
}