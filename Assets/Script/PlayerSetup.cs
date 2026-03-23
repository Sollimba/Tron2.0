using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    public BikeController bikeController;


    public void IsLocalPlayer()
    {
        bikeController.enabled = true;
    }
}
