using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject winText;

    private List<BikeHealth> players = new List<BikeHealth>();
    private bool gameEnded = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // находим всех игроков на сцене
        BikeHealth[] allPlayers = FindObjectsOfType<BikeHealth>();

        foreach (var p in allPlayers)
        {
            players.Add(p);
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
            {
                Debug.Log("DRAW");
            }

            StartCoroutine(RestartGame());
        }
    }

    IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}