using UnityEngine;

public class BikeHealth : MonoBehaviour
{
    private bool isDead = false;

    public bool IsAlive()
    {
        return !isDead;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("Trail"))
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        Debug.Log(gameObject.name + " DIED");

        // уведомляем GameManager
        GameManager.Instance.OnPlayerDied(this);

        gameObject.SetActive(false);
    }
}