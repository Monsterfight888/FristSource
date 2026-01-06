using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ParentPlayer : MonoBehaviour
{
    public Classes[] classes;

    public int thisClass;

    public Player player;

    public Manager manager;

    private void Start()
    {
        
        //Init();
    }

    private void Update()
    {
        if (player == null)
            return;
        if (!player.photonView.IsMine)
            return;

        //if (Input.GetKeyDown(KeyCode.L))
          //  Init();

        //temporary changing class system

        if (Input.GetKeyDown(KeyCode.K))
        {
            thisClass++;

            if (classes.Length == thisClass)
                thisClass = 0;

            player.Initiated = false;

            

            player.CallBackChangeClass(thisClass);
        }
    }


    public void Init()
    {
        if (player.photonView.IsMine)
            player.CallBackChangeClass(thisClass);
    }
}
