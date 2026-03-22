using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
public class TrailBuilder : MonoBehaviour
{
    public GameObject trailPrefab;
    public float minDistance = 0.05f;
    public float lifeTime = 5f;
    public float colliderDelay = 0.4f;

    private GameObject currentSegment;
    private Vector3 lastPoint;
    private Vector3 lastDirection;

    private bool canBuild = false;

    private List<GameObject> segments = new List<GameObject>();

    public GameObject owner;

    void Start()
    {
        if (!GetComponent<PhotonView>().IsMine)
        {
            enabled = false;
            return;
        }
        owner = gameObject;
        lastDirection = transform.forward;
        lastPoint = transform.position;
        StartNewSegment();
    }

    void Update()
    {
        if (!canBuild) return;
        Vector3 currentDirection = transform.forward;
        float distance = Vector3.Distance(transform.position, lastPoint);

        // новый сегмент при повороте или расстоянии
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
        canBuild = true;
    }

    void StartNewSegment()
    {
        currentSegment = Instantiate(trailPrefab, transform.position, Quaternion.identity);
        segments.Add(currentSegment);

        // передаем owner
        TrailSegment segmentScript = currentSegment.GetComponent<TrailSegment>();
        if (segmentScript != null)
        {
            segmentScript.owner = owner;
        }

        // расчет позиции
        Vector3 direction = transform.position - lastPoint;
        Vector3 center = lastPoint + direction / 2f;

        currentSegment.transform.position = center;
        currentSegment.transform.rotation = Quaternion.LookRotation(direction);

        // масштаб
        Vector3 scale = currentSegment.transform.localScale;
        scale.z = Mathf.Max(direction.magnitude, 0.01f) + 0.2f; // небольшой overlap
        currentSegment.transform.localScale = scale;

        // коллайдер
        Collider col = currentSegment.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            StartCoroutine(EnableColliderDelayed(col));
        }

        Destroy(currentSegment, lifeTime);

        lastPoint = transform.position;
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