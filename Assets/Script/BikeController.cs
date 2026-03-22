using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class BikeController : MonoBehaviourPun
{
    public float speed = 10f;
    public float turnSpeed = 120f;

    private float turnInput = 0f;
    private Rigidbody rb;

    private bool canMove = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // ❗ отключаем чужих игроков
        if (!photonView.IsMine)
        {
            enabled = false;
        }
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        Vector3 forwardMove = transform.forward * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);

        Quaternion turn = Quaternion.Euler(0, turnInput * turnSpeed * Time.fixedDeltaTime, 0);
        rb.MoveRotation(rb.rotation * turn);
    }

    public void EnableMovement()
    {
        canMove = true;
    }

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        turnInput = input.x;
    }
}