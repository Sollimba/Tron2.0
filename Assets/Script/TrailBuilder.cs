using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class TrailBuilder : MonoBehaviourPun
{
    public string trailPrefabName = "TrailSegment";

    public float minDistance = 0.05f;
    public float lifeTime = 5f;
    public float colliderDelay = 0.4f;

    private GameObject currentSegment;
    private Vector3 lastPoint;
    private Vector3 lastDirection;

    private bool canBuild = false;

    void Start()
    {
        if (!photonView.IsMine)
        {
            enabled = false;
            return;
        }

        lastDirection = transform.forward;
        lastPoint = transform.position;

        StartNewSegment();
    }

    void Update()
    {
        if (!canBuild) return;

        Vector3 currentDirection = transform.forward;
        float distance = Vector3.Distance(transform.position, lastPoint);

        if (Vector3.Angle(lastDirection, currentDirection) > 5f || distance >= minDistance)
        {
            StartNewSegment();
            lastDirection = currentDirection;
        }

        if (currentSegment != null)
            UpdateSegment();
    }

    public void EnableTrail()
    {
        if (!photonView.IsMine) return;
        canBuild = true;
    }

    void StartNewSegment()
    {
        currentSegment = PhotonNetwork.Instantiate(
            trailPrefabName,
            transform.position,
            Quaternion.identity
        );

        // назначаем владельца
        TrailSegment segment = currentSegment.GetComponent<TrailSegment>();
        if (segment != null)
            segment.owner = gameObject;

        Vector3 direction = transform.position - lastPoint;
        Vector3 center = lastPoint + direction / 2f;

        currentSegment.transform.position = center;
        currentSegment.transform.rotation = Quaternion.LookRotation(direction);

        Vector3 scale = currentSegment.transform.localScale;
        scale.z = Mathf.Max(direction.magnitude, 0.01f) + 0.2f;
        currentSegment.transform.localScale = scale;

        Collider col = currentSegment.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            StartCoroutine(EnableColliderDelayed(col));
        }

        // 🔥 удаление через Photon
        StartCoroutine(DestroyAfterTime(currentSegment));

        lastPoint = transform.position;
    }

    IEnumerator DestroyAfterTime(GameObject obj)
    {
        yield return new WaitForSeconds(lifeTime);
        PhotonView pv = obj.GetComponent<PhotonView>();

        if (pv != null && pv.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    IEnumerator EnableColliderDelayed(Collider col)
    {
        yield return new WaitForSeconds(colliderDelay);

        if (col != null)
            col.enabled = true;
    }

    void UpdateSegment()
    {
        Vector3 direction = transform.position - lastPoint;
        Vector3 center = lastPoint + direction / 2f;

        currentSegment.transform.position = center;
        currentSegment.transform.rotation = Quaternion.LookRotation(direction);

        Vector3 scale = currentSegment.transform.localScale;
        scale.z = Mathf.Max(direction.magnitude, 0.01f) + 0.2f;
        currentSegment.transform.localScale = scale;
    }
}