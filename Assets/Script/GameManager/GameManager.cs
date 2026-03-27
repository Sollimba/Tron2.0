using UnityEngine;
using System.Collections;
using Photon.Pun;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public GameObject winText;
    public GameObject loseText;
    public GameObject drawText;
    public TextMeshProUGUI countdownText;

    private bool gameEnded = false;
    private bool gameStarted = false;

    void Awake() => Instance = this;

    void Start()
    {
        if (countdownText == null)
            countdownText = FindFirstObjectByType<TextMeshProUGUI>();

        StartCoroutine(WaitForPlayers());
    }

    IEnumerator WaitForPlayers()
    {
        countdownText.gameObject.SetActive(true);

        while (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            countdownText.text = "Waiting for players...";
            yield return null;
        }

        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("StartCountdownRPC", RpcTarget.All);
    }

    IEnumerator StartCountdown()
    {
        if (winText != null) winText.SetActive(false);
        if (loseText != null) loseText.SetActive(false);
        if (drawText != null) drawText.SetActive(false);

        countdownText.text = "3"; yield return new WaitForSeconds(1f);
        countdownText.text = "2"; yield return new WaitForSeconds(1f);
        countdownText.text = "1"; yield return new WaitForSeconds(1f);
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
            bike.EnableMovement();

        foreach (var trail in trails)
            trail.EnableTrail();
    }

    public void OnPlayerDied(BikeHealth deadPlayer)
    {
        if (!PhotonNetwork.IsMasterClient || gameEnded) return;

        int aliveCount = 0;
        BikeHealth lastAlive = null;

        var allPlayers = FindObjectsByType<BikeHealth>(FindObjectsSortMode.None);
        foreach (var p in allPlayers)
        {
            if (p != null && p.IsAlive())
            {
                aliveCount++;
                lastAlive = p;
            }
        }

        if (aliveCount == 0)
        {
            gameEnded = true;
            photonView.RPC("ShowDrawRPC", RpcTarget.All);
            StartCoroutine(RestartRound());
        }
        else if (aliveCount == 1)
        {
            gameEnded = true;
            int winnerID = lastAlive.photonView.ViewID;
            photonView.RPC("ShowWinnerRPC", RpcTarget.All, winnerID, deadPlayer.photonView.ViewID);
            StartCoroutine(RestartRound());
        }
    }

    IEnumerator RestartRound()
    {
        yield return new WaitForSeconds(3f);

        if (!PhotonNetwork.IsMasterClient) yield break;

        // Очистка всех Trail сегментов
        var trails = FindObjectsByType<TrailBuilder>(FindObjectsSortMode.None);
        foreach (var t in trails)
            photonView.RPC("ClearTrailsRPC", RpcTarget.All);

        // Удаляем всех игроков
        var bikes = FindObjectsByType<BikeController>(FindObjectsSortMode.None);
        foreach (var b in bikes)
            PhotonNetwork.Destroy(b.gameObject);

        // Скрываем UI
        if (winText != null) winText.SetActive(false);
        if (loseText != null) loseText.SetActive(false);
        if (drawText != null) drawText.SetActive(false);

        gameEnded = false;
        gameStarted = false;

        // Спавним новых игроков
        var networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager != null)
            networkManager.SpawnPlayer();

        // Запускаем отсчёт для всех
        photonView.RPC("StartCountdownRPC", RpcTarget.All);
    }

    [PunRPC]
    void StartCountdownRPC()
    {
        if (gameStarted) return;
        gameStarted = true;
        StartCoroutine(StartCountdown());
    }

    [PunRPC]
    void ShowDrawRPC()
    {
        if (drawText != null)
            drawText.SetActive(true);

        if (winText != null) winText.SetActive(false);
        if (loseText != null) loseText.SetActive(false);
    }

    [PunRPC]
    void ShowWinnerRPC(int winnerID, int loserID)
    {
        PhotonView winnerPV = PhotonView.Find(winnerID);
        PhotonView loserPV = PhotonView.Find(loserID);

        if (winnerPV != null && winText != null)
        {
            winText.SetActive(true);
            winText.GetComponent<TextMeshProUGUI>().text = winnerPV.gameObject.name + " WINS!";
        }

        if (loserPV != null && loseText != null)
        {
            loseText.SetActive(true);
            loseText.GetComponent<TextMeshProUGUI>().text = "YOU LOSE";
        }

        if (drawText != null)
            drawText.SetActive(false);
    }

    [PunRPC]
    void ClearTrailsRPC()
    {
        var trails = FindObjectsByType<TrailBuilder>(FindObjectsSortMode.None);
        foreach (var t in trails)
            if (t.photonView.IsMine)
                t.ClearMyTrails();
    }
}