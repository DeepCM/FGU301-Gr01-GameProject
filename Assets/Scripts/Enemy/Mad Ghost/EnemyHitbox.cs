using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public EnemyData data; // Tham chiếu tới ScriptableObject để lấy sát thương

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Thay "PlayerHealth" bằng tên script máu của người chơi
            // collision.GetComponent<PlayerHealth>().TakeDamage(data.damage);
            Debug.Log("Gây sát thương: " + data.damage);
        }
    }
}