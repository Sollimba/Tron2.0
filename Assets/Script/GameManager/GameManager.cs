using UnityEngine;
using System.Collections;
using Photon.Pun;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public GameObject winText;
    public GameObject drawText;
    public TextMeshProUGUI countdownText;

    private bool gameEnded = false;
    private bool gameStarted = false;

    void Awake() => Instance = this;

    public GameObject spectatorPrefab;
    public Transform spectatorSpawnPoint;

    private int playersReady = 0;

    enum GameState
    {
        Waiting,
        Starting,
        Playing,
        Ending
    }

    private GameState currentState = GameState.Waiting;

    void Start()
    {
        if (countdownText == null)
            countdownText = FindFirstObjectByType<TextMeshProUGUI>();

        StartCoroutine(WaitForPlayers());
    }

    IEnumerator WaitForSceneClean()
    {
        while (true)
        {
            bool hasPlayers = FindObjectsByType<BikeController>(FindObjectsSortMode.None).Length > 0;
            bool hasTrails = GameObject.FindGameObjectsWithTag("Trail").Length > 0;

            if (!hasPlayers && !hasTrails)
                break;

            yield return null;
        }
    }

    IEnumerator WaitForPlayers()
    {
        countdownText.gameObject.SetActive(true);

        while (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            countdownText.text = "Waiting for players...";
            yield return null;
        }

        countdownText.text = "Players connected...";
    }

    public void SpawnSpectator(Vector3 deathPosition)
    {
        Vector3 spawnPos = spectatorSpawnPoint != null
            ? spectatorSpawnPoint.position
            : deathPosition;

        GameObject spec = Instantiate(spectatorPrefab, spawnPos, Quaternion.identity);

        Camera cam = spec.GetComponentInChildren<Camera>();
        if (cam != null)
            cam.gameObject.SetActive(true);
    }

    IEnumerator StartCountdown()
    {
        if (winText != null) winText.SetActive(false);
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
        currentState = GameState.Playing;

        var bikes = FindObjectsByType<BikeController>(FindObjectsSortMode.None);
        var trails = FindObjectsByType<TrailBuilder>(FindObjectsSortMode.None);

        foreach (var bike in bikes)
            bike.EnableMovement();

        foreach (var trail in trails)
            trail.EnableTrail();
    }

    public void OnPlayerDied(BikeHealth deadPlayer)
    {

        if (!PhotonNetwork.IsMasterClient) return;
        if (currentState != GameState.Playing) return;

        currentState = GameState.Ending;

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

        currentState = GameState.Waiting;

        photonView.RPC("ResetUIRPC", RpcTarget.All);
        photonView.RPC("DestroyAllSpectatorsRPC", RpcTarget.All);
        photonView.RPC("DestroyAllTrailsRPC", RpcTarget.All);
        photonView.RPC("DestroyAllPlayersRPC", RpcTarget.All);

        // 💥 ЖДЁМ ПОЛНУЮ ОЧИСТКУ
        yield return StartCoroutine(WaitForSceneClean());

        // 💥 небольшой буфер
        yield return new WaitForSeconds(0.3f);

        playersReady = 0;
        gameStarted = false;

        photonView.RPC("SpawnPlayersRPC", RpcTarget.All);
    }

    public void ShowLoseScreen()
    {
        if (drawText != null)
            drawText.SetActive(true);
    }

    [PunRPC]
    void PlayerReadyRPC()
    {
        playersReady++;

        if (PhotonNetwork.IsMasterClient)
        {
            // ❗ ждём пока реально есть игроки
            if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
                return;

            if (playersReady >= PhotonNetwork.CurrentRoom.PlayerCount)
            {
                playersReady = 0;

                photonView.RPC("StartCountdownRPC", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void DestroyAllSpectatorsRPC()
    {
        GameObject[] spectators;

        try
        {
            spectators = GameObject.FindGameObjectsWithTag("Spectator");
        }
        catch
        {
            return; // если тег не найден — просто выходим
        }

        foreach (var s in spectators)
        {
            if (s != null)
                Destroy(s);
        }
    }

    [PunRPC]
    void SpawnPlayersRPC()
    {
        var networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager != null)
        {
            networkManager.SpawnPlayer();
        }
    }

    [PunRPC]
    void ResetUIRPC()
    {
        gameStarted = false;
        if (winText != null) winText.SetActive(false);
        if (drawText != null) drawText.SetActive(false);
    }

    [PunRPC]
    void ShowWinnerRPC(int winnerID, int loserID)
    {
        PhotonView winnerPV = PhotonView.Find(winnerID);

        if (winnerPV != null)
        {
            if (winnerPV.IsMine)
            {
                if (winText != null)
                {
                    winText.SetActive(true);
                    winText.GetComponent<TextMeshProUGUI>().text = "YOU WIN";
                }
            }
            else
            {
                if (drawText != null)
                    drawText.SetActive(true);
            }
        }
    }

    [PunRPC]
    void StartCountdownRPC()
    {
        if (currentState != GameState.Waiting) return;

        currentState = GameState.Starting;
        StartCoroutine(StartCountdown());
    }

    [PunRPC]
    void DestroyAllPlayersRPC()
    {
        var bikes = FindObjectsByType<BikeController>(FindObjectsSortMode.None);

        foreach (var b in bikes)
        {
            if (b == null) continue;

            PhotonView pv = b.GetComponent<PhotonView>();

            if (pv != null && pv.IsMine)
            {
                PhotonNetwork.Destroy(b.gameObject);
            }
        }
    }

    [PunRPC]
    void ShowDrawRPC()
    {
        if (drawText != null)
            drawText.SetActive(true);

        if (winText != null) winText.SetActive(false);
    }

    [PunRPC]
    void DestroyAllTrailsRPC()
    {
        var trails = GameObject.FindGameObjectsWithTag("Trail");

        foreach (var t in trails)
        {
            if (t == null) continue;

            PhotonView pv = t.GetComponent<PhotonView>();

            if (pv != null && pv.IsMine)
            {
                PhotonNetwork.Destroy(t);
            }
        }
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