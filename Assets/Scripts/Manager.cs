using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Manager : MonoBehaviour
{
    public string player_prefab;
    public Transform[] spawn_points;
    public Camera currentCam;
    void Start()
    {
        Spawn(null);
    }
    public void Spawn(GameObject thisParent)
    {
        if(thisParent == null)
        {
            Transform t_spawn = spawn_points[Random.Range(0, spawn_points.Length)];
            PhotonNetwork.Instantiate(player_prefab, t_spawn.position, t_spawn.rotation);
        }
        else
        {
            Transform t_spawn = spawn_points[Random.Range(0, spawn_points.Length)];
            PhotonNetwork.Instantiate(player_prefab, t_spawn.position, t_spawn.rotation);
        }
    }

    public IEnumerator IESpawn()
    {
        yield return new WaitForSeconds(5f);
        Transform t_spawn = spawn_points[Random.Range(0, spawn_points.Length)];
        PhotonNetwork.Instantiate(player_prefab, t_spawn.position, t_spawn.rotation);
    }
}
