using UnityEngine;

public class AudioListenerFix : MonoBehaviour
{
    public bool isLocalPlayer = true; // позже будет из сети

    void Start()
    {
        GetComponent<AudioListener>().enabled = isLocalPlayer;
    }
}