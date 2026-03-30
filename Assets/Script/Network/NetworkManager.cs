using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("Room1", new RoomOptions { MaxPlayers = 5 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room → Spawning player");

        SpawnPlayer();
    }
    IEnumerator SendReadyDelayed()
    {
        yield return new WaitForSeconds(0.3f);

        PhotonView gm = FindFirstObjectByType<GameManager>().photonView;

        gm.RPC("PlayerReadyRPC", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);

        Debug.Log("READY SENT");
    }

    public void SpawnPlayer()
    {
        // 💥 если уже есть наш игрок — не спавним
        var existing = FindObjectsByType<BikeController>(FindObjectsSortMode.None);

        foreach (var b in existing)
        {
            if (b.GetComponent<PhotonView>().IsMine)
                return;
        }

        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        if (playerIndex >= spawnPoints.Length)
            playerIndex = Random.Range(0, spawnPoints.Length);

        Transform spawnPoint = spawnPoints[playerIndex];

        GameObject player = PhotonNetwork.Instantiate(
            playerPrefab.name,
            spawnPoint.position,
            spawnPoint.rotation
        );

        StartCoroutine(SendReadyDelayed());

        Debug.Log("Spawned: " + player.name + " | IsMine: " + player.GetComponent<PhotonView>().IsMine);

    }



}