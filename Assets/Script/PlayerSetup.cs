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
            bikeController.enabled = true;

            // ✅ включаем камеру только у себя
            playerCamera.gameObject.SetActive(true);

            // ✅ включаем звук
            var listener = playerCamera.GetComponent<AudioListener>();
            if (listener != null)
                listener.enabled = true;
        }
        else
        {
            bikeController.enabled = false;

            // ❌ отключаем чужую камеру
            playerCamera.gameObject.SetActive(false);
        }
    }
}