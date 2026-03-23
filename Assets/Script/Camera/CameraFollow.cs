using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -6);

    public float positionSmooth = 10f;
    public float rotationSmooth = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // 🔥 ПЛАВНАЯ ПОЗИЦИЯ
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            positionSmooth * Time.deltaTime
        );

        // 🔥 ПЛАВНЫЙ ПОВОРОТ
        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            desiredRotation,
            rotationSmooth * Time.deltaTime
        );
    }
}