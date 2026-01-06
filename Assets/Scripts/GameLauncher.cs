using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameLauncher : MonoBehaviourPunCallbacks
{

    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        Connect();
        Application.targetFrameRate = 60;
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected AAAAA");

        base.OnConnectedToMaster();
    }
    public void Connect()
    {
        Debug.Log("Trying to connect boi");
        PhotonNetwork.GameVersion = "0.0.0";
        PhotonNetwork.ConnectUsingSettings();
    }
    public void Join()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinedRoom()
    {
        StartGame();

        base.OnJoinedRoom();
    }
    public void Create()
    {
        PhotonNetwork.CreateRoom("");
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Create();

        base.OnJoinRandomFailed(returnCode, message);
    }
    public void StartGame()
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }
}
