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
        if (photonView.IsMine)
        {
            Debug.Log("YOU LOSE");

            GameManager.Instance.ShowLoseScreen();

            // ❗ ВАЖНО: НЕ удаляем объект сразу
            GetComponent<BikeController>().enabled = false;
            GetComponent<Collider>().enabled = false;

            // можно спрятать модель
            transform.GetChild(0).gameObject.SetActive(false);
            isDead = true;
        }

        GameManager.Instance.OnPlayerDied(this);
    }
}