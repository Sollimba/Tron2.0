using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;

    public Transform[] spawnPoints; // 🔥 точки спавна

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon");

        PhotonNetwork.JoinOrCreateRoom("Room1", new RoomOptions { MaxPlayers = 5 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");

        SpawnPlayer();
        GetComponent<PlayerSetup>().IsLocalPlayer();
    }

    void SpawnPlayer()
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        // защита от выхода за массив
        if (playerIndex >= spawnPoints.Length)
        {
            playerIndex = Random.Range(0, spawnPoints.Length);
        }

        Transform spawnPoint = spawnPoints[playerIndex];

        PhotonNetwork.Instantiate(
            playerPrefab.name,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}