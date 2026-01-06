using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;



public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    
    #region Variables
    public enum PlayerStates
    {
        Standing,
        Walking,
        Running,
        Sliding
    }
    public PlayerStates playerState;
    private Animator animator;
    public float maxHealth;
    private float currentHealth;
    public float slideModifier;
    public float speed;
    private Rigidbody rb;
    public float sprintModifier;
    public float aimModifier;
    public Camera normalCam;
    public Camera camera;
    private float baseFov = 60;
    private float sprintFOVModifier = 1.2f;
    public Transform weaponParent;
    public float jumpForce;
    public float checkRaduis;
    public LayerMask ground;
    public Transform groundCheck;
    private Vector3 weaponParentOrigin;
    public float movementCounter;
    private float idleCounter;
    private Vector3 targetBobWeaponPosition;
    public Manager manager;
    private bool sliding;
    private float Slide_Time;
    public float lengthOfSlide;
    private Vector3 Slide_direction;
    private Vector3 camOrigin;
    private Vector3 weaponParentCurrentPos;
    private Weapon weapon;
    public GameObject GFX;
    private Transform arrow;
    private Image arrowGFX;
    private bool hasBeenAttacked = false;
    private Player lastPlayerToAttackMe;
    private bool arrowIsFadingOut;
    private bool arrowIsFadingIn;
    public float fadeTime = 1;

    private Transform body;

    public bool disableMovement;

    private Animator hurtOverlay;

    private Transform UI_HealthBar;
    private Text Ui_Ammo;
    private float gunRot;

    public Classes thisClass;
    public bool Initiated = false;
    #endregion

    #region Photon Callbacks
    public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_messageinfo)
    {
        if (p_stream.IsWriting)
        {
            p_stream.SendNext(camera.transform.localRotation.x);
            //if (weapon.GetComponent<Weapon>().loadout[weapon.GetComponent<Weapon>().currentIndex].instance != null)
            //{
            //    p_stream.SendNext(weapon.GetComponent<Weapon>().loadout[weapon.GetComponent<Weapon>().currentIndex].instance.transform.position);
            //}
        }
        else if (p_stream.IsReading)
        {
            gunRot = (float)p_stream.ReceiveNext();
            //if(weapon.GetComponent<Weapon>().loadout[weapon.GetComponent<Weapon>().currentIndex].instance != null)
            //{
            //    weapon.GetComponent<Weapon>().loadout[weapon.GetComponent<Weapon>().currentIndex].instance.transform.position = (Vector3)p_stream.ReceiveNext();
            //}
        }
        
    }
    #endregion
    //standing base m1 = 0.25
    //standing base Pistol = 0.2x

    //Pistol Fire scale = 1;
    //M1 Firce scale = 5;
    #region Monobehaviour Callbacks
    public void CallBackChangeClass(int newClass)
    {
        photonView.RPC("ChangeClassRPC", RpcTarget.All, newClass, photonView.ViewID);
    }
    [PunRPC]
    private void ChangeClassRPC(int newClass, int view)
    {
        Initiated = false;
        ChangeClass(newClass, view);
        //manager.StartCoroutine(manager.IESpawn());
    }
    
    public void ChangeClass(int newClass, int View)
    {


        //Debug.LogErrorFormat(View.ToString());

        //Check if player is the right one to make gfx
        //GameObject t_playerObj = null;

        PhotonView thisView = PhotonNetwork.GetPhotonView(View);

        Player t_player = thisView.GetComponent<Player>();

        if (t_player.Initiated)
            return;

        //t_playerObj = thisView.gameObject;

        //t_player = t_playerObj.GetComponent<Player>();



        Classes[] selectableClasses = t_player.transform.parent.GetComponent<ParentPlayer>().classes;

        //Debug.LogErrorFormat("Changing Class");
        //Debug.LogErrorFormat(photonView + "  " + selectableClasses[newClass].GFXPref.name);

        t_player.thisClass = selectableClasses[newClass];


        t_player.weapon.loadout = selectableClasses[newClass].loudout;

        if (selectableClasses[newClass].isBackwards)
        {
            t_player.GFX.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            t_player.GFX.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        //t_player.weapon.EquipCallBack(0);




        t_player.weapon.StartCoroutine(t_player.weapon.Equip(0));


        if (photonView.IsMine)
        {

            t_player.Initiated = true;

            foreach (Item a in t_player.weapon.loadout) a.Initialize();

            return;
        }

        if (t_player.transform.Find("GFX/Base Model Apllied") != null)
        {
            Destroy(t_player.transform.Find("GFX/Base Model Apllied").gameObject);
        }

        

        GameObject t_GFX = Instantiate(selectableClasses[newClass].GFXPref, t_player.GFX.transform);

        t_GFX.name = "Base Model Apllied";



        

        t_player.animator.Rebind();

        t_player.body = t_player.transform.Find("GFX/Base Model Apllied/Armature/Body 1");
        t_player.normalCam.transform.parent.localPosition = new Vector3(t_player.normalCam.transform.parent.localPosition.x, t_player.thisClass.cameraHieght, t_player.normalCam.transform.parent.localPosition.x);
        t_player.weaponParentOrigin = t_player.normalCam.transform.parent.localPosition;
        t_player.weapon.transform.localPosition = t_player.normalCam.transform.parent.localPosition;
        t_player.weaponParentCurrentPos = t_player.normalCam.transform.parent.localPosition;
        t_player.Initiated = true;

        
    }
    private void Awake()
    {
        transform.parent.GetComponent<ParentPlayer>().player = this;

        animator = GetComponent<Animator>();
        weapon = GetComponent<Weapon>();

    }
    private void Start()
    {
        //Technicall stuff


        //PhotonNetwork.OfflineMode = true;
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;
        
        camera.enabled = photonView.IsMine;
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        arrow = GameObject.Find("HUD/Aiming Reticle/Arrow").transform;
        arrowGFX = GameObject.Find("HUD/Aiming Reticle/Arrow/GFX").GetComponent<Image>();
        UI_HealthBar = GameObject.Find("HUD/Health/Bar").transform;
        
        if (photonView.IsMine)
        {

            hurtOverlay = GameObject.Find("HUD/Panel").GetComponent<Animator>();
            Ui_Ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
            RefreshHealthBar();
            if(GFX != null)
            {
                GFX.SetActive(false);
            }
            
        }
        rb = GetComponent<Rigidbody>();
        

        camOrigin = normalCam.transform.localPosition;
        if (!photonView.IsMine)
        {
            weaponParent.localRotation = Quaternion.Euler(0, 180, 0);
            gameObject.layer = 11;
        }
        // i dunno what the frick this is
        //Explanation for past me, it is disabling the map camera. 
        //Camera.main.enabled = false;
        baseFov = normalCam.fieldOfView;
        weaponParentOrigin = weaponParent.localPosition;
        RefreshHealthBar();
        weaponParentCurrentPos = weaponParentOrigin;


    }

    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, checkRaduis);
    }
    public void Update()
    {
        if (!Initiated)
            return;


        if (!photonView.IsMine)
        {
            //if(weapon.currentWeapon != null /*&& weapon.transform.rotation != Quaternion.Euler(weapon.transform.rotation.x, 180, weapon.transform.rotation.z*/)
            //{
            //weapon.transform.rotation = Quaternion.Euler(weapon.transform.rotation.x, 180, weapon.transform.rotation.z);
            //weapon.transform.rotation = Quaternion.Euler(weapon.transform.rotation.x, 180, weapon.transform.rotation.z);
            //weapon.currentWeapon.GetComponent<Sway>().GFX.SetActive(false);
            //}
            if (weapon.currentWeapon != null)
            {

                weapon.currentWeapon.GetComponent<Sway>().RotateGunX(gunRot);
            }
            float bodyRot = gunRot * 180;



            if (!thisClass.isBackwards)
            {
                bodyRot = -bodyRot + 90;

            }
            else
            {
                bodyRot += 90;
            }

            float t_lerpTime = 0.8f;

            bodyRot = bodyRot / thisClass.bodyDamper;
            if(Initiated)
                body.localRotation = Quaternion.Lerp(Quaternion.Euler(bodyRot, 0, 0), body.localRotation, t_lerpTime);
            return;
        }
        //Handle Arrow
        if(hasBeenAttacked && lastPlayerToAttackMe == null)
        {
            hasBeenAttacked = false;
        }
        if (hasBeenAttacked)
        {
            Vector3 myForward = transform.forward;
            myForward.y = 0.0f;
            Vector3 toAttackingPlayer = lastPlayerToAttackMe.transform.position - transform.position;
            toAttackingPlayer.y = 0;
            toAttackingPlayer.Normalize();

            float dot = Vector3.Dot(toAttackingPlayer, myForward);


            float rotation = Mathf.Acos(dot) * Mathf.Rad2Deg;

            Vector3 cross = Vector3.Cross(toAttackingPlayer, myForward);
            if (cross.y < 0.0f)
            {
                rotation *= -1.0f;
            }
            rotation -= 90.0f;
            arrow.localRotation = Quaternion.Euler(Vector3.forward * rotation);
        }
        if (arrowIsFadingOut)
        {
            fadeTime -= 0.01f;
            arrowGFX.color = new Color(1, 0, 0, fadeTime);
        }
        else if (arrowIsFadingIn)
        {
            fadeTime += 0.1f;
            arrowGFX.color = new Color(1, 0, 0, fadeTime);
        }
        else
        {
            fadeTime = 1;
        }
        //UI Refreshes
        RefreshHealthBar();
        //dis brocken
        weapon.RefreshAmmo(Ui_Ammo);
        if (Pause.paused || disableMovement) return;
        //Axes
        float t_hmove = Input.GetAxisRaw("Horizontal");
        float t_vmove = Input.GetAxisRaw("Vertical");
        /*if(t_hmove >= 0.5f||t_vmove >= 0.5f|| t_hmove <= -0.5f || t_vmove <= -0.5f)
        {
            animator.SetBool("isRunning", true);
        }
        else if(t_hmove = 0 ||)
        {
            animator.SetBool("isRunning", false);
        }*/

        //Controls
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool pause = Input.GetKeyDown(KeyCode.Escape);
        //States
        bool isGrounded = Physics.CheckSphere(groundCheck.position, checkRaduis, ground);
        bool isJumping = jump;
        bool isSprinting = sprint && t_vmove < 0 && !isJumping && isGrounded && weapon.currentCooldown <= 0;

        

        //Animations
        if (t_hmove == 0 || t_vmove == 0 || t_hmove == 0 || t_vmove == 0)
        {
            playerState = PlayerStates.Standing;
        }


        if (t_hmove == 1 || t_vmove == 1 || t_hmove == -1 || t_vmove == -1)
        {
            animator.SetBool("isRunning", true);
            if (isSprinting)
            {
                playerState = PlayerStates.Running;
            }
            else
            {
                playerState = PlayerStates.Walking;
            }
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
        if (isSprinting)
        {
            animator.SetBool("isSprinting", true);
        }
        else
        {
            animator.SetBool("isSprinting", false);
        }
        if (sliding)
        {
            playerState = PlayerStates.Sliding;
            animator.SetBool("isSliding", true);
        }
        else
        {
            animator.SetBool("isSliding", false);
        }
        if (isGrounded)
        {
            animator.SetBool("Fall", false);
        }
        else if(!isGrounded)
        {
            animator.SetBool("Fall", true);
        }
        
        //animator.SetFloat("Look", camera.transform.localRotation.x);
        //pause
        if (pause)
        {
            GameObject.Find("Pause").GetComponent<Pause>().togglePause();
        }
        
        if (Pause.paused || disableMovement)
        {
            t_vmove = 0;
            t_hmove = 0;
            sprint = false;
            jump = false;
            pause = false;
            isGrounded = false;
            isJumping = false;
            isSprinting = false;
        }
        //Jumping
        if (isJumping && isGrounded)
        {
            rb.AddRelativeForce(Vector3.up * jumpForce * 100);
        }
        //Head Bob

        if (sliding)
        {
            HeadBob(movementCounter, 0.15f, 0.075f);
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetBobWeaponPosition, Time.deltaTime * 10f);
        }
        else if(t_hmove == 0 && t_vmove == 0)
        {
                HeadBob(idleCounter, 0.025f, 0.025f);
                idleCounter += Time.deltaTime;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetBobWeaponPosition, Time.deltaTime * 2f);
        }
        else if(!isSprinting)
        {
                HeadBob(movementCounter, 0.035f, 0.035f);
                movementCounter += Time.deltaTime * 3f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetBobWeaponPosition, Time.deltaTime * 6f);
        }
        else
        {
                HeadBob(movementCounter, 0.08f, 0.08f);
                movementCounter += Time.deltaTime * 5f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetBobWeaponPosition, Time.deltaTime * 10f);
        }
        
    }
    void FixedUpdate()
    {
        //Physics stuff (movement etc)

        if (!photonView.IsMine || !Initiated) return;
        if (Pause.paused || disableMovement) return;
        //Axes
        float t_hmove = Input.GetAxisRaw("Horizontal");
        float t_vmove = Input.GetAxisRaw("Vertical");

        //Controls
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool slide = Input.GetKey(KeyCode.LeftAlt);

        //States
        bool isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, .3f, ground);
        bool isJumping = jump;
        bool isSprinting = sprint && t_vmove < 0 && !isJumping && isGrounded && weapon.currentCooldown <= 0;
        bool isSliding = isSprinting && slide && !sliding;

        //Movement
        Vector3 t_Direction = Vector3.zero;
        float t_adjustedSpeed = speed;


        if (!sliding)
        {
            t_Direction = new Vector3(t_hmove, 0, t_vmove).normalized;
            t_Direction = transform.TransformDirection(t_Direction);

            if (isSprinting) t_adjustedSpeed *= sprintModifier;

            if (weapon.isAiming) t_adjustedSpeed *= weapon.loadout[weapon.currentIndex].aimModifier;
        }
        else
        {
            t_Direction = Slide_direction;
            t_adjustedSpeed *= slideModifier;
            Slide_Time = Mathf.Max(0, Slide_Time - Time.deltaTime);
            if (Slide_Time <= 0)
            {
                sliding = false;
                weaponParentCurrentPos += Vector3.up * 0.5f;
            }
        }
        //Camera Stuff
        if (sliding)
        {
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFov * sprintFOVModifier * 1.25f, Time.fixedDeltaTime * 8);
            normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, camOrigin + Vector3.down * .5f, Time.deltaTime * 6f);
        }
        else if (isSprinting)
        {
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFov * sprintFOVModifier, Time.fixedDeltaTime * 8);
        }
        else
        {
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFov, Time.fixedDeltaTime * 8);
            normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, camOrigin, Time.deltaTime * 6f);
        }
        Vector3 t_targetVelocity = t_Direction* t_adjustedSpeed * Time.fixedDeltaTime;
        t_targetVelocity.y = rb.velocity.y;
        rb.velocity = t_targetVelocity;

        // sliding
        if (isSliding)
        {
            sliding = true;
            Slide_direction = t_Direction;
            Slide_Time = lengthOfSlide;
            weaponParentCurrentPos += Vector3.down * 0.5f;
        }
    }
    #endregion

    #region Private Method
    void HeadBob(float p_z, float p_x_intensity, float p_y_intensity)
    {

        float t_aim_adjust = 1f;
        if (weapon.isAiming)
        {
            t_aim_adjust = .1f;
        }
        targetBobWeaponPosition = weaponParentCurrentPos + new Vector3(Mathf.Cos(p_z)
        * p_x_intensity * t_aim_adjust, Mathf.Sin(p_z * 2) * p_y_intensity * t_aim_adjust, 0);
    }
    #endregion

    #region Public Methods

    void RefreshHealthBar()
    {
        if (!Initiated)
        {
            float t_health_ratio = (float)currentHealth / (float)maxHealth;
            UI_HealthBar.localScale = Vector3.Lerp(UI_HealthBar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 8f);
            return;
        }

        //Make easier, by having a value in the player that is filled out by other objects, and a bool to see if it needs to be displayed, instead of hardcoding it to be only for the drone.
        if (weapon.loadout[weapon.currentIndex].weaponType == Item.WeaponType.Drone && weapon.loadout[weapon.currentIndex].configureBool1)
        {
            float t_health_ratio = (float)(weapon.loadout[weapon.currentIndex].instance.GetComponent<Drone>().currentHealth 
                / (float)weapon.loadout[weapon.currentIndex].instance.transform.GetComponent<Drone>().maxHealth);
            UI_HealthBar.localScale = Vector3.Lerp(UI_HealthBar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 8f);
        }
        else
        {
            float t_health_ratio = (float)currentHealth / (float)maxHealth;
            UI_HealthBar.localScale = Vector3.Lerp(UI_HealthBar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 8f);
        }
    }
    public IEnumerator ArrowDisappear()
    {
        arrowIsFadingOut = true;
        yield return new WaitForSeconds(3f);
        arrowIsFadingOut = false;
        fadeTime = 1;
    }
    public IEnumerator ArrowApear()
    {
        arrowIsFadingIn = true;
        yield return new WaitForSeconds(0.5f);
        arrowIsFadingIn = false;
        fadeTime = 1;
        StartCoroutine("ArrowDisappear");
    }

    public void TakeDamage(int p_damage, int playerPhotonID, bool isDrone)
    {
        if (photonView.IsMine)
        {
            
            hurtOverlay.Play("Hurt", 0, 0);
            if (arrowIsFadingOut)
            {
                StopCoroutine("ArrowDisappear");
                StartCoroutine("ArrowDisappear");
                fadeTime = 1;
            }
            hasBeenAttacked = true;
            PhotonView attackingPlayerView = PhotonNetwork.GetPhotonView(playerPhotonID);
            Player attackingPlayer = null;
            if (photonView != null)
            {
                attackingPlayer = attackingPlayerView.gameObject.GetComponent<Player>();
                lastPlayerToAttackMe = attackingPlayer;

                StartCoroutine("ArrowApear");
            }

            if (isDrone)
            {


                weapon.loadout[weapon.currentIndex].instance.transform.GetComponent<Drone>().currentHealth -= p_damage;

                float t_currentHealth = weapon.loadout[weapon.currentIndex].instance.transform.GetComponent<Drone>().currentHealth;

                if (t_currentHealth <= 0)
                {
                    arrowGFX.color = new Color(1, 0, 0, 0);
                    StopCoroutine("ArrowApear");
                    StopCoroutine("ArrowDisappear");

                    /*if(weapon.aimingRetical != null)
                    {
                        weapon.aimingRetical.StopCoroutine(weapon.aimingRetical.tickCor);
                    }*/

                    weapon.aimingRetical.tickCor = null;
                    weapon.aimingRetical.ReticleSet(false);
                    weapon.destroyDrone();

                }
            }
            else
            {
                if (weapon.loadout[weapon.currentIndex].weaponType == Item.WeaponType.Drone && weapon.loadout[weapon.currentIndex].configureBool1)
                {
                    weapon.destroyDrone();
                }
                currentHealth -= p_damage;

                if (currentHealth <= 0)
                {
                    arrowGFX.color = new Color(1, 0, 0, 0);
                    StopCoroutine("ArrowApear");
                    StopCoroutine("ArrowDisappear");
                    manager.StartCoroutine("IESpawn");

                    /*if(weapon.aimingRetical != null)
                    {
                        weapon.aimingRetical.StopCoroutine(weapon.aimingRetical.tickCor);
                    }*/

                    weapon.aimingRetical.tickCor = null;
                    weapon.aimingRetical.ReticleSet(false);
                    PhotonNetwork.Destroy(gameObject);

                }
            }
        }

        


        }
    public IEnumerator playerSpawn()
    {
        speed = 0;

        gameObject.SetActive(false);
        //PhotonNetwork.Destroy(gameObject);
        yield return new WaitForSeconds(5f);
        
        manager.Spawn(transform.parent.gameObject);
    }
}
    #endregion
