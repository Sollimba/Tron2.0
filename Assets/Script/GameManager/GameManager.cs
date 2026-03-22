using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject winText;

    private List<BikeHealth> players = new List<BikeHealth>();
    private bool gameEnded = false;

    [Header("UI")]
    public TextMeshProUGUI countdownText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        var allPlayers = FindObjectsByType<BikeHealth>(FindObjectsSortMode.None);

        foreach (var p in allPlayers)
        {
            players.Add(p);
        }

        // 🔥 запускаем игру через отсчёт
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSeconds(1f);

        countdownText.text = "2";
        yield return new WaitForSeconds(1f);

        countdownText.text = "1";
        yield return new WaitForSeconds(1f);

        countdownText.text = "GO!";

        EnablePlayers();

        yield return new WaitForSeconds(1f);

        countdownText.gameObject.SetActive(false);
    }

    void EnablePlayers()
    {
        var bikes = FindObjectsByType<BikeController>(FindObjectsSortMode.None);
        var trails = FindObjectsByType<TrailBuilder>(FindObjectsSortMode.None);

        foreach (var bike in bikes)
        {
            bike.EnableMovement();
        }

        foreach (var trail in trails)
        {
            trail.EnableTrail();
        }
    }

    public void OnPlayerDied(BikeHealth deadPlayer)
    {
        if (gameEnded) return;

        int aliveCount = 0;
        BikeHealth lastAlive = null;

        foreach (var p in players)
        {
            if (p != null && p.IsAlive())
            {
                aliveCount++;
                lastAlive = p;
            }
        }

        if (aliveCount <= 1)
        {
            gameEnded = true;

            if (lastAlive != null)
            {
                Debug.Log("WINNER: " + lastAlive.gameObject.name);

                if (winText != null)
                    winText.SetActive(true);
            }
            else
                Debug.Log("DRAW");

            StartCoroutine(RestartGame());
        }
    }

    IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}