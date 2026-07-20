using UnityEngine;
using System.Collections;

public class MadGhostController : MonoBehaviour
{
    [Header("Data")]
    public EnemyData data; // Kéo file ScriptableObject vào đây

    [Header("References")]
    public GameObject attackHitbox;
    public Animator anim;

    private Transform player;
    private bool isAttacking = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Đảm bảo hitbox tắt lúc khởi đầu
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    void Update()
    {
        if (player == null || isAttacking) return;

        // Gọi hàm flip mỗi frame để kiểm tra hướng
        Flip();

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= data.attackRange)
        {
            StartCoroutine(AttackRoutine());
        }
        else
        {
            ChasePlayer();
        }
    }

    void Flip()
    {
        // Nếu player bên phải (x > 0) thì scale x = 1 (mặc định)
        // Nếu player bên trái (x < 0) thì scale x = -1 (lật ngược)
        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void ChasePlayer()
    {
        // Di chuyển về phía người chơi
        transform.position = Vector2.MoveTowards(transform.position, player.position, data.moveSpeed * Time.deltaTime);

        anim.SetBool("isMoving", true);
        anim.SetBool("isAttack", false);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        anim.SetBool("isMoving", false);
        anim.SetBool("isAttack", true);

        // Đợi một chút rồi reset trạng thái tấn công
        yield return new WaitForSeconds(data.attackCooldown);

        anim.SetBool("isAttack", false);
        isAttacking = false;
    }

    // --- CÁC HÀM GỌI TỪ ANIMATION EVENT ---
    public void EnableHitbox()
    {
        if (attackHitbox != null) attackHitbox.SetActive(true);
    }

    public void DisableHitbox()
    {
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }
}