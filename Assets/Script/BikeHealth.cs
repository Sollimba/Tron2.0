using UnityEngine;
using Photon.Pun;

public class BikeHealth : MonoBehaviourPun
{
    private bool isDead = false;

    public bool IsAlive()
    {
        return !isDead;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if (isDead) return;

        if (other.CompareTag("Trail"))
        {
            Die();
        }
    }

    void Die()
    {
        if (!photonView.IsMine) return; // ❗ КРИТИЧЕСКОЕ УСЛОВИЕ

        photonView.RPC("DieRPC", RpcTarget.All);
    }

    [PunRPC]
    void DieRPC()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("DIED ON: " + gameObject.name);

        gameObject.SetActive(false);

        GameManager.Instance.OnPlayerDied(this);
    }
}