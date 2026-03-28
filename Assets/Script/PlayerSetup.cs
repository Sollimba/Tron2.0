using UnityEngine;
using Photon.Pun;

public class PlayerSetup : MonoBehaviourPun
{
    public BikeController bikeController;
    public Camera playerCamera;

    void Start()
    {
        if (photonView.IsMine)
        {
            playerCamera.gameObject.SetActive(true);

            if (playerCamera.GetComponent<AudioListener>() == null)
                playerCamera.gameObject.AddComponent<AudioListener>();
        }
        else
        {
            bikeController.enabled = false;

            // ❌ отключаем чужую камеру
            playerCamera.gameObject.SetActive(false);
        }
    }
}