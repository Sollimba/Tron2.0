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

        // ❗ отключаем управление
        GetComponent<BikeController>().enabled = false;

        // ❗ НЕ отключаем collider (иначе падает!)
        // GetComponent<Collider>().enabled = false;

        // скрываем модель
        transform.GetChild(0).gameObject.SetActive(false);

        // 🔥 сообщаем GameManager
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.OnPlayerDied(this);
        }

        // 🔥 локально становимся spectator
        if (photonView.IsMine)
        {
            GameManager.Instance.SpawnSpectator(transform.position);
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
        }
    }
}