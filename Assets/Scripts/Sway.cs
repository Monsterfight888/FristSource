using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Sway : MonoBehaviourPunCallbacks
{
    #region Variables
    public float intensity;
    public float smooth;
    public bool isMine;
    public GameObject GFX;
    public bool frist = false;

    private Quaternion origin_rotation;
    #endregion

    #region Monobehaviour Callbacks
    private void Start()
    {
        origin_rotation = Quaternion.identity; // transform.rotation;
        
        
    }
    private void Update()
    {
        if (Pause.paused) return;
        UpdateSway();
    }
    #endregion

    public void RotateGunX(float rotation)
    {
        origin_rotation.x = rotation;
    }

    #region Private Methods
    private void UpdateSway()
    {
        //controls
        float t_x_mouse = Input.GetAxis("Mouse X");
        float t_y_mouse = Input.GetAxis("Mouse Y");
        if (!isMine && frist)
        {
            //transform.Rotate(new Vector3(origin_rotation.x, 180, origin_rotation.z));
            //origin_rotation = Quaternion.Euler(0, 180, 0);
            frist = false;
        }
        if (!isMine)
        {
            t_x_mouse = 0;
            t_y_mouse = 0;
        }

        //calculate target rotation
        Quaternion t_xadj = Quaternion.AngleAxis(-intensity * t_x_mouse, Vector3.up);
        Quaternion t_yadj = Quaternion.AngleAxis(intensity * t_y_mouse, Vector3.right);
        Quaternion target_rotation = origin_rotation * t_xadj * t_yadj;
        
        //rotate twords target rotation
        transform.localRotation = Quaternion.Lerp(transform.localRotation, target_rotation, Time.deltaTime * smooth);
        
    }
    #endregion

}
