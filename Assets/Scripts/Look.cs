using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Look : MonoBehaviourPunCallbacks
{
    #region Variables
    public Transform player;
    public Transform cams;
    public Transform weapon;

    public float xSensitifity;
    public float ySensitifity;
    public float maxAngle;
    public static bool cursorLock = true;

    private Quaternion camCenter;
    #endregion

    #region Monobehavior Callbacks
    private void Awake()
    {
        // this lock da framerate
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    void Start()
    {
        camCenter = cams.localRotation; // origin of cam
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;
        if (Pause.paused) return;
        Sety();
        Setx();
        UpdateCursorLock();
    }
    #endregion

    #region Private Methods

    void Sety()
    {
        float t_input = Input.GetAxisRaw("Mouse Y") * ySensitifity * Time.deltaTime;
        Quaternion t_adj = Quaternion.AngleAxis(t_input, -Vector3.right);
        Quaternion t_delta = cams.localRotation * t_adj;
        if (Quaternion.Angle(camCenter, t_delta) < maxAngle)
        {
            cams.localRotation = t_delta;
        }
        if(weapon != null)
        {

            weapon.rotation = cams.rotation;
        }
    }
    void Setx()
    {
        float t_input = Input.GetAxisRaw("Mouse X") * xSensitifity * Time.deltaTime;
        Quaternion t_adj = Quaternion.AngleAxis(t_input, Vector3.up);
        Quaternion t_delta = player.localRotation * t_adj;
        player.localRotation = t_delta;
    }
    void UpdateCursorLock()
    {
        if (cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLock = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLock = true;
            }
        }
    }
    #endregion
}
