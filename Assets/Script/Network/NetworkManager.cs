using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;

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
    }

    void SpawnPlayer()
    {
        Vector3 spawnPos = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));

        PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);
    }
}