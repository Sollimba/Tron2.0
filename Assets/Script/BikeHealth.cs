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
        if (isDead) return;

        isDead = true;

        photonView.RPC("DieRPC", RpcTarget.All);
    }

    [PunRPC]
    void DieRPC()
    {
        isDead = true;

        GetComponent<BikeController>().enabled = false;

        // GetComponent<Collider>().enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.OnPlayerDied(this);
        }

        if (photonView.IsMine)
        {
            GameManager.Instance.SpawnSpectator(transform.position);
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
        }
    }
}