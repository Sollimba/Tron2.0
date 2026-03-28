using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class TrailBuilder : MonoBehaviourPun
{
    public string trailPrefabName = "TrailSegment";
    public float minDistance = 0.1f;
    public float colliderDelay = 0.4f;
    public int maxSegments = 200;

    private List<GameObject> segments = new List<GameObject>();
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

        lastPoint = transform.position;
        lastDirection = transform.forward;

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

    public void EnableTrail() => canBuild = true;

    void OnDisable()
    {
        ClearMyTrails();
    }

    void StartNewSegment()
    {
        GameObject prefab = Resources.Load<GameObject>(trailPrefabName);
        if (prefab == null) return;

        currentSegment = PhotonNetwork.Instantiate(trailPrefabName, transform.position, Quaternion.identity);
        segments.Add(currentSegment);

        if (segments.Count > maxSegments)
        {
            PhotonNetwork.Destroy(segments[0]);
            segments.RemoveAt(0);
        }

        UpdateSegment();

        Collider col = currentSegment.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            StartCoroutine(EnableColliderDelayed(col));
        }

        lastPoint = transform.position;
    }

    void UpdateSegment()
    {
        Vector3 direction = transform.position - lastPoint;
        Vector3 center = lastPoint + direction / 2f;

        currentSegment.transform.position = center;
        currentSegment.transform.rotation = Quaternion.LookRotation(direction);

        Vector3 scale = currentSegment.transform.localScale;
        scale.z = Mathf.Max(direction.magnitude, 0.01f);
        currentSegment.transform.localScale = scale;
    }

    IEnumerator EnableColliderDelayed(Collider col)
    {
        yield return new WaitForSeconds(colliderDelay);
        if (col != null)
            col.enabled = true;
    }

    // 🔹 Очистка всех сегментов игрока
    public void ClearMyTrails()
    {
        foreach (var seg in segments)
        {
            if (seg != null)
                PhotonNetwork.Destroy(seg);
        }
        segments.Clear();
        currentSegment = null;
    }
}