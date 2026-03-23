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

        Debug.Log(gameObject.name + " IsMine = " + photonView.IsMine);
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // 👉 ЧИТАЕМ КЛАВИАТУРУ НАПРЯМУЮ
        float turn = 0f;

        if (Keyboard.current.aKey.isPressed)
            turn = -1f;

        if (Keyboard.current.dKey.isPressed)
            turn = 1f;

        turnInput = turn;
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        if (!canMove) return;

        Vector3 forwardMove = transform.forward * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);

        Quaternion turn = Quaternion.Euler(0, turnInput * turnSpeed * Time.fixedDeltaTime, 0);
        rb.MoveRotation(rb.rotation * turn);
    }

    public void EnableMovement()
    {
        if (!photonView.IsMine) return;
        canMove = true;
    }
}