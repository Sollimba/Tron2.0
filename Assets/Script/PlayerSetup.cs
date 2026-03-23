using UnityEngine;
using Photon.Pun;

public class PlayerSetup : MonoBehaviourPun
{
    public BikeController bikeController;
    public GameObject cameraPrefab;

    void Start()
    {
        if (photonView.IsMine)
        {
            bikeController.enabled = true;

            GameObject cam = Instantiate(cameraPrefab);

            CameraFollow follow = cam.GetComponent<CameraFollow>();

            // 🔥 ВОТ ЭТА СТРОКА КРИТИЧЕСКАЯ
            follow.target = transform;
        }
        else
        {
            bikeController.enabled = false;
        }
    }
}