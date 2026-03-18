using UnityEngine;
using UnityEngine.InputSystem;

public class BikeController : MonoBehaviour
{
    public float speed = 10f;
    public float turnSpeed = 120f;

    private float turnInput = 0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // движение вперед
        Vector3 forwardMove = transform.forward * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);

        // поворот
        Quaternion turn = Quaternion.Euler(0, turnInput * turnSpeed * Time.fixedDeltaTime, 0);
        rb.MoveRotation(rb.rotation * turn);
    }

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        turnInput = input.x;
    }
}