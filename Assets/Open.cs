using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Open : MonoBehaviourPunCallbacks
{
    private bool open = false;
    public GameObject Door;
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 12)
        {
            Door = other.gameObject;
            if (Input.GetKeyDown(KeyCode.E) && photonView.IsMine)
            {
                photonView.RPC("ToggleOpenRPC", RpcTarget.All);
            }
        }
    }
    
    [PunRPC]
    public void ToggleOpenRPC()
    {
            ToggleOpen();
    }
    public void ToggleOpen()
    {
        open = !open;
        if (open)
        {
            Door.GetComponent<Animator>().Play("Open", 0, 0);
        }
        else
        {
            Door.GetComponent<Animator>().Play("Open Reverse", 0, 0);
        }
    }
}
