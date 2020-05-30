using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField NicknameInput;
    public GameObject DisconnectPanel;
    public GameObject RespawnPanel;
    public Transform[] SpawnPoints;

    void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
    }

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = NicknameInput.text;
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 6 }, null);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnectPanel.SetActive(true);
        RespawnPanel.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        DisconnectPanel.SetActive(false);
        StartCoroutine("DestroyBullet");
        Spawn();
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject GO in GameObject.FindGameObjectsWithTag("Bullet")) GO.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
    }

    public void Spawn()
    {
        int ranSpawnPoint = Random.Range(0, 5);
        PhotonNetwork.Instantiate("Player", SpawnPoints[ranSpawnPoint].position, SpawnPoints[ranSpawnPoint].rotation);
        RespawnPanel.SetActive(false);
    }
}
