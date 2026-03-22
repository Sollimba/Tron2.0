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
        if (!photonView.IsMine)
        {
            cameraTransform.gameObject.SetActive(false);
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