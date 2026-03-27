using UnityEngine;
using Photon.Pun;

public class BikeHealth : MonoBehaviourPun
{
    private bool isDead = false;

    public bool IsAlive() => !isDead;

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
        if (!photonView.IsMine) return;

        photonView.RPC("DieRPC", RpcTarget.All);
    }

    [PunRPC]
    void DieRPC()
    {
        if (isDead) return;
        isDead = true;

        if (photonView.IsMine)
        {
            Debug.Log("YOU LOSE");
        }

        // Временное скрытие объекта, фактически он будет удален GameManager'ом
        gameObject.SetActive(false);

        GameManager.Instance.OnPlayerDied(this);
    }
}