using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

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

    public override void OnJoinedRoom() => SpawnPlayer();

    public void SpawnPlayer()
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        if (playerIndex >= spawnPoints.Length)
            playerIndex = Random.Range(0, spawnPoints.Length);

        Transform spawnPoint = spawnPoints[playerIndex];

        GameObject player = PhotonNetwork.Instantiate(
            playerPrefab.name,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Debug.Log("Spawned: " + player.name + " | IsMine: " + player.GetComponent<PhotonView>().IsMine);

    }

}