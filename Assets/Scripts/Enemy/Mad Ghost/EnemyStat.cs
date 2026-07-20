using UnityEngine;

[CreateAssetMenu(fileName = "NewGhostData", menuName = "Enemy/Ghost Data")]
public class EnemyData : ScriptableObject
{
    public float moveSpeed = 3f;
    public float attackRange = 1.2f;
    public int damage = 10;
    public float attackCooldown = 1.5f;
}