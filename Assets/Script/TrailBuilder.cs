using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrailBuilder : MonoBehaviour
{
    public GameObject trailPrefab;
    public float minDistance = 0.05f;     // очень маленькое расстояние для нового сегмента
    public float lifeTime = 5f;
    public float colliderDelay = 0.3f;   // задержка включения коллайдера

    private GameObject currentSegment;
    private Vector3 lastPoint;
    private Vector3 lastDirection;

    private List<GameObject> segments = new List<GameObject>();

    void Start()
    {
        lastDirection = transform.forward;
        lastPoint = transform.position;
        StartNewSegment();
    }

    void Update()
    {
        Vector3 currentDirection = transform.forward;
        float distance = Vector3.Distance(transform.position, lastPoint);

        // Создаём новый сегмент при повороте или при прохождении minDistance
        if (Vector3.Angle(lastDirection, currentDirection) > 5f || distance >= minDistance)
        {
            StartNewSegment();
            lastDirection = currentDirection;
        }

        // Обновляем текущий сегмент
        if (currentSegment != null)
            UpdateSegment();
    }

    void StartNewSegment()
    {
        currentSegment = Instantiate(trailPrefab, transform.position, Quaternion.identity);
        segments.Add(currentSegment);

        // Обновляем lastPoint только после создания сегмента
        Vector3 direction = transform.position - lastPoint;
        Vector3 center = lastPoint + direction / 2f;

        currentSegment.transform.position = center;
        currentSegment.transform.rotation = Quaternion.LookRotation(direction);

        // Масштаб сегмента по длине
        Vector3 scale = currentSegment.transform.localScale;
        scale.z = Mathf.Max(direction.magnitude, 0.01f); // минимальная длина, чтобы не ломалось
        currentSegment.transform.localScale = scale;

        // коллайдер
        Collider col = currentSegment.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            StartCoroutine(EnableColliderDelayed(col));
        }

        Destroy(currentSegment, lifeTime);
        lastPoint = transform.position; // обновляем lastPoint
    }

    IEnumerator EnableColliderDelayed(Collider col)
    {
        yield return new WaitForSeconds(colliderDelay);
        if (col != null)
            col.enabled = true;
    }

    void UpdateSegment()
    {
        // Поддерживаем текущий сегмент растянутым до текущей позиции
        Vector3 direction = transform.position - lastPoint;
        Vector3 center = lastPoint + direction / 2f;

        currentSegment.transform.position = center;
        currentSegment.transform.rotation = Quaternion.LookRotation(direction);

        Vector3 scale = currentSegment.transform.localScale;
        scale.z = Mathf.Max(direction.magnitude, 0.01f);
        currentSegment.transform.localScale = scale;
    }
}