using UnityEngine;
using Photon.Pun;

public class CameraFollow : MonoBehaviourPun
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -6);
    public float smoothSpeed = 5f;

    public Transform cameraTransform;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        AudioListener listener = cameraTransform.GetComponent<AudioListener>();

        if (!photonView.IsMine)
        {
            cameraTransform.gameObject.SetActive(false);

            if (listener != null)
                listener.enabled = false;
        }
        else
        {
            if (listener != null)
                listener.enabled = true;
        }
    }

    void LateUpdate()
    {
        if (!photonView.IsMine) return; // ❗ важно

        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.LookAt(target);
    }
}