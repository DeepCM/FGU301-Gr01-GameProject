using UnityEngine;
using Game.Events;

public class DeathPlane : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player fell into Death Plane!");
            GameEvents.RaisePlayerDied();
        }
    }
}
