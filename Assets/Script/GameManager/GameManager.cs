using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public GameObject winText;
    public GameObject drawText;
    public TextMeshProUGUI countdownText;

    public GameObject spectatorPrefab;
    public Transform spectatorSpawnPoint;

    private HashSet<int> readyPlayers = new HashSet<int>();

    enum GameState
    {
        Waiting,
        Starting,
        Playing
    }

    private GameState currentState = GameState.Waiting;

    void Awake() => Instance = this;

    void Start()
    {
        // 👉 просто показываем текст ожидания
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "Waiting for players...";
        }
    }

    // ===================== SPECTATOR =====================

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

    // ===================== START GAME =====================

    [PunRPC]
    void PlayerReadyRPC(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        readyPlayers.Add(actorNumber);

        // ждём минимум 2 игроков
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            return;

        if (readyPlayers.Count >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            readyPlayers.Clear();

            photonView.RPC("StartCountdownRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    void StartCountdownRPC()
    {
        if (currentState != GameState.Waiting) return;

        currentState = GameState.Starting;
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        winText?.SetActive(false);
        drawText?.SetActive(false);

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
        currentState = GameState.Playing;

        var bikes = FindObjectsByType<BikeController>(FindObjectsSortMode.None);
        var trails = FindObjectsByType<TrailBuilder>(FindObjectsSortMode.None);

        foreach (var bike in bikes)
            bike.EnableMovement();

        foreach (var trail in trails)
            trail.EnableTrail();
    }

    // ===================== GAME LOGIC =====================

    public void OnPlayerDied(BikeHealth deadPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (currentState != GameState.Playing) return;

        var players = FindObjectsByType<BikeHealth>(FindObjectsSortMode.None);

        List<BikeHealth> alive = new List<BikeHealth>();

        foreach (var p in players)
        {
            if (p != null && p.IsAlive())
                alive.Add(p);
        }

        // все умерли
        if (alive.Count == 0)
        {
            photonView.RPC("ShowDrawRPC", RpcTarget.All);
            StartCoroutine(RestartRound());
            return;
        }

        // один победитель
        if (alive.Count == 1)
        {
            int winnerID = alive[0].photonView.ViewID;

            photonView.RPC("ShowWinnerRPC", RpcTarget.All, winnerID);
            StartCoroutine(RestartRound());
        }
    }

    // ===================== UI =====================

    [PunRPC]
    void ShowWinnerRPC(int winnerID)
    {
        PhotonView winnerPV = PhotonView.Find(winnerID);

        if (winnerPV != null && winnerPV.IsMine)
        {
            winText.SetActive(true);
            winText.GetComponent<TextMeshProUGUI>().text = "YOU WIN";
        }
        else
        {
            drawText.SetActive(true);
            drawText.GetComponent<TextMeshProUGUI>().text = "YOU DRAW";
        }
    }

    [PunRPC]
    void ShowDrawRPC()
    {
        drawText?.SetActive(true);
        winText?.SetActive(false);
    }

    [PunRPC]
    void ResetUIRPC()
    {
        winText?.SetActive(false);
        drawText?.SetActive(false);
    }

    // ===================== RESTART =====================

    IEnumerator RestartRound()
    {
        yield return new WaitForSeconds(3f);

        if (!PhotonNetwork.IsMasterClient) yield break;

        currentState = GameState.Waiting;

        photonView.RPC("ResetUIRPC", RpcTarget.All);
        photonView.RPC("DestroyAllSpectatorsRPC", RpcTarget.All);
        photonView.RPC("DestroyAllTrailsRPC", RpcTarget.All);
        photonView.RPC("DestroyAllPlayersRPC", RpcTarget.All);

        yield return new WaitForSeconds(1f);

        readyPlayers.Clear();

        photonView.RPC("SpawnPlayersRPC", RpcTarget.All);
    }

    // ===================== CLEAN =====================

    [PunRPC]
    void DestroyAllSpectatorsRPC()
    {
        var spectators = GameObject.FindGameObjectsWithTag("Spectator");

        foreach (var s in spectators)
            if (s != null)
                Destroy(s);
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
                PhotonNetwork.Destroy(b.gameObject);
        }
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
                PhotonNetwork.Destroy(t);
        }
    }

    [PunRPC]
    void SpawnPlayersRPC()
    {
        var networkManager = FindFirstObjectByType<NetworkManager>();
        networkManager?.SpawnPlayer();
    }
}