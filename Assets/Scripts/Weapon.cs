using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Weapon : MonoBehaviourPunCallbacks
{
    #region Variables
    public Item[] loadout;
    public Transform weaponParent;
    [System.NonSerialized]
    public GameObject currentWeapon;
    public int currentIndex;
    public GameObject bulletHolePrefab;
    public LayerMask canBeShot;
    [System.NonSerialized]
    public float currentCooldown;
    private bool isReloading;
    public bool isAiming = false;
    private bool isEquiping;
    private bool isUnequiping;
    private Animator animator;
    [System.NonSerialized]
    public AimingRetical aimingRetical;
    public GameObject tracer;
    public Manager manager;


    public Player player;

    public Camera myCurrentCamera;
    //private float shootBloom;
    //private bool lerpTime = false;
    #endregion

    #region Monobehaviour Callbacks
    private void Start()
    {
        player = GetComponent<Player>();
        //Debug.Log("Me " + gameObject.name);
        aimingRetical = GameObject.Find("HUD/Aiming Reticle").GetComponent<AimingRetical>();
        animator = GetComponent<Animator>();
        /*if(photonView.IsMine)
            foreach (Item a in loadout) a.Initialize();*/
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        myCurrentCamera = transform.Find("Cameras/NormalCamParent/Normal Camera").GetComponent<Camera>();


        //StartCoroutine(Equip(0));
    }
    public void destroyDrone()
    {
        PhotonNetwork.Destroy(loadout[currentIndex].instance);
        photonView.RPC("destroyDronePun", RpcTarget.All);
        GetComponent<Player>().GFX.SetActive(false);
    }
    [PunRPC]
    public void destroyDronePun()
    {
        //Destroy(loadout[currentIndex].instance);
        myCurrentCamera = transform.Find("Cameras/NormalCamParent/Normal Camera").GetComponent<Camera>();
        transform.Find("Cameras/NormalCamParent/Normal Camera").gameObject.SetActive(true);
        GetComponent<Player>().disableMovement = false;
        loadout[currentIndex].configureBool1 = false;
        
    }
    public void EquipDirectCallBack(int weapon)
    {
        //photonView.RPC("EquipRPC", RpcTarget.All, weapon);
    }

    public void QuickEquip()
    {

    }
    void Update()
    {
        if (!player.Initiated) return;
        if (Pause.paused) return;
        /*else if(photonView.IsMine && currentWeapon == null)
        {
            animator.SetInteger("Gun", 0);
            //Redical
        }*/
        if (photonView.IsMine)
        {
            if (!isReloading)
            {
                //equip/dequip
                if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1))
                {
                    if(loadout[currentIndex].weaponType == Item.WeaponType.Drone && loadout[currentIndex].configureBool1)
                    {
                        destroyDrone();
                    }
                    photonView.RPC("EquipRPC", RpcTarget.All, 0);
                }
                if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2))
                {
                    if (loadout[currentIndex].weaponType == Item.WeaponType.Drone && loadout[currentIndex].configureBool1)
                    {
                        destroyDrone();
                    }
                    photonView.RPC("EquipRPC", RpcTarget.All, 1);
                }
                if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha3))
                {
                    photonView.RPC("EquipRPC", RpcTarget.All, 2);
                }
                /*if (photonView.IsMine && Input.GetKeyDown(KeyCode.Tab) && currentWeapon != null)
                {
                    photonView.RPC("UnEquipRPC", RpcTarget.All);
                }*/
            }
            if (loadout[currentIndex].weaponType == Item.WeaponType.Drone && !loadout[currentIndex].configureBool1)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    loadout[currentIndex].configureBool1 = true;
                    GameObject drone = PhotonNetwork.Instantiate(loadout[currentIndex].specialPref.name, transform.position, Quaternion.identity);
                    GetComponent<Player>().GFX.SetActive(true);
                    int ID = drone.transform.Find("Main").gameObject.GetPhotonView().ViewID;// GetInstanceID();
                    photonView.RPC("spawnDrone", RpcTarget.All, ID);
                    transform.Find("Cameras/NormalCamParent/Normal Camera").gameObject.SetActive(false);
                }
                return;
            }
            //Bloom Base Control
            float baseAmountOfBloom = loadout[currentIndex].standingBaseBloom;
            //float bloomSpeed = loadout[currentIndex].bloomSwitchSpeedLerp;
            if (GetComponent<Player>().playerState == Player.PlayerStates.Standing)
            {
                baseAmountOfBloom = loadout[currentIndex].standingBaseBloom;
                if (isAiming)
                {
                    baseAmountOfBloom = loadout[currentIndex].standingAimBaseBloom;
                }
            }
            else if(GetComponent<Player>().playerState == Player.PlayerStates.Walking)
            {
                baseAmountOfBloom = loadout[currentIndex].walkingBaseBloom;
                if (isAiming)
                {
                    baseAmountOfBloom = loadout[currentIndex].WalkingAimBaseBloom;
                }
            }
            else if(GetComponent<Player>().playerState == Player.PlayerStates.Running)
            {
                baseAmountOfBloom = loadout[currentIndex].runningBaseBloom;
            }
            else if(GetComponent<Player>().playerState == Player.PlayerStates.Sliding)
            {
                baseAmountOfBloom = loadout[currentIndex].slidingBaseBloom;
            }
            //if firing base bloom += accuracy after shooting
            

            //setting base scale
            aimingRetical.scale = Mathf.Lerp(baseAmountOfBloom, aimingRetical.scale, loadout[currentIndex].bloomSwitchSpeedLerp);


            loadout[currentIndex].fireAccuracyStack = Mathf.Lerp(0, loadout[currentIndex].fireAccuracyStack, loadout[currentIndex].fireResetSpeedLerp);

            //shooting Scale Modifier
            aimingRetical.scaleModdifier = loadout[currentIndex].fireAccuracyStack;
            //aimingRetical.scaleModdifier = Mathf.Lerp(0, aimingRetical.scaleModdifier, loadout[currentIndex].fireResetSpeedLerp);
        }

        //Debug.Log(aimingRetical.scaleModdifier);
        
        
        //reload
        if (currentWeapon != null)
        {
            if (photonView.IsMine)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    photonView.RPC("ReloadRPC", RpcTarget.All);
                }
                if(GetComponent<Player>().playerState != Player.PlayerStates.Running || GetComponent<Player>().playerState != Player.PlayerStates.Sliding)
                {
                    photonView.RPC("Aim", RpcTarget.All, Input.GetMouseButton(1));
                }
                if (loadout[currentIndex].burst == 0)
                {
                    if (Input.GetMouseButtonDown(0) && currentCooldown <= 0 && !isReloading && !isEquiping && !isUnequiping)
                    {
                        if (loadout[currentIndex].FireBullet())
                        {
                            Shoot();
                        }
                        else
                        {
                            photonView.RPC("ReloadRPC", RpcTarget.All);
                        }
                    }
                }
                else
                {
                    if (Input.GetMouseButton(0) && currentCooldown <= 0 && !isReloading && !isEquiping && !isUnequiping)
                    {
                        if (loadout[currentIndex].FireBullet())
                        {
                            Shoot();
                        }
                        else
                        {
                            photonView.RPC("ReloadRPC", RpcTarget.All);
                            //StartCoroutine(Reload(loadout[currentIndex].reloadTime));
                        }
                    }
                }
                //cooldown
                if (currentCooldown > 0)
                {
                    currentCooldown -= Time.deltaTime;
                }

            }
            // weapoion position elasticity
            currentWeapon.transform.localPosition = Vector3.Lerp(Vector3.zero, currentWeapon.transform.localPosition, loadout[currentIndex].CostmeticResetSpeedLerp);
            currentWeapon.transform.localRotation = Quaternion.Lerp(Quaternion.identity, currentWeapon.transform.localRotation, loadout[currentIndex].CostmeticResetSpeedLerp);


        }
            
        
    }
    #endregion

    #region Private Methods
    
    [PunRPC]
    private void ReloadRPC()
    {
        
            StartCoroutine(Reload(loadout[currentIndex].reloadTime));
    }

    IEnumerator Reload(float p_wait)
    {
        if(currentWeapon != null && photonView.IsMine )
        {
            if (!isReloading && !isEquiping && !isUnequiping)
            {
                isReloading = true;

                currentWeapon.GetComponent<Animator>().Play("Reload", 0, 0);
                yield return new WaitForSeconds(p_wait);

                currentWeapon.SetActive(true);
                loadout[currentIndex].Reload();
                isReloading = false;
            }
            
        }
        else
        {
            currentWeapon.GetComponent<Animator>().Play("Reload", 0, 0);
        }
    }
    [PunRPC]
    void UnEquipRPC()
    {
        if(isUnequiping == false)
        {
            StartCoroutine(UnEquip());
        }
    }
    IEnumerator UnEquip()
    {
        //Now cosmetic because it just takes the gun down and then replaces it.

        isUnequiping = true;
        currentWeapon.GetComponent<Animator>().Play("UnEquip", 0, 0);
        yield return new WaitForSeconds(loadout[currentIndex].unEquipTime);
        /*if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }
        currentWeapon = null;*/
        isUnequiping = false;
    }

    [PunRPC]
    void EquipRPC(int p_ind)
    {
        if (isEquiping == false && isUnequiping == false)
        {
            StartCoroutine(UnEquip());
            /*if(currentWeapon != null && isUnequiping == false)
            {
                StartCoroutine(UnEquip());
            }*/
            StartCoroutine(Equip(p_ind));
        }
    }
    public IEnumerator Equip(int p_ind)
    {
        while (isUnequiping)
            yield return null;
        isEquiping = true;
        if (currentWeapon != null)
        {
            if (isReloading)
            {
                StopCoroutine("Reload");
            }
            Destroy(currentWeapon);
        }

        currentIndex = p_ind;


        GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        t_newWeapon.GetComponent<Animator>().Play("Equip", 0, 0);
        
        t_newWeapon.GetComponent<Sway>().isMine = photonView.IsMine;
        t_newWeapon.GetComponent<Sway>().frist = true;
        yield return new WaitForSeconds(loadout[p_ind].equipTime);
        currentCooldown = 0;
        t_newWeapon.transform.localPosition = Vector3.zero;
        t_newWeapon.transform.localEulerAngles = Vector3.zero;
        


        currentWeapon = t_newWeapon;
        isEquiping = false;
    }
    [PunRPC]
    void Aim(bool p_isAiming)
    {
        if(currentWeapon != null)
        {

            isAiming = p_isAiming;
            Transform t_achor = currentWeapon.transform.Find("Anchor");
            if(t_achor == null)
            {
                return;
            }
            Transform t_states_ads = currentWeapon.transform.Find("States/ADS");
            Transform t_states_hip = currentWeapon.transform.Find("States/Hip");
            Transform t_states_run = currentWeapon.transform.Find("States/Run");
            if(GetComponent<Player>().playerState == Player.PlayerStates.Running)
            {
                t_achor.position = Vector3.Lerp(t_states_run.position, t_achor.position, loadout[currentIndex].aimSpeedLerp);
                t_achor.rotation = Quaternion.Lerp(t_states_run.rotation, t_achor.rotation, loadout[currentIndex].aimSpeedLerp);
            }
            else
            {
                if (p_isAiming)
                {
                    //aim
                    t_achor.position = Vector3.Lerp(t_states_ads.position, t_achor.position, loadout[currentIndex].aimSpeedLerp);
                    t_achor.rotation = Quaternion.Lerp(t_states_ads.rotation, t_achor.rotation, loadout[currentIndex].aimSpeedLerp);
                }
                else
                {
                    //hip
                    t_achor.position = Vector3.Lerp(t_states_hip.position, t_achor.position, loadout[currentIndex].aimSpeedLerp);
                    t_achor.rotation = Quaternion.Lerp(t_states_hip.rotation, t_achor.rotation, loadout[currentIndex].aimSpeedLerp);
                }
            }
            
        }
        
        
    }
    /*IEnumerator TimeBeforeLerp(float time)
    {
        lerpTime = false;
        yield return new WaitForSeconds(time);
        lerpTime = true;
    }*/
    void Shoot()
    {
        //Profiler.BeginSample("Shoot");
        //Transform t_spawn = transform.Find("Cameras/NormalCamParent/Normal Camera");
        Transform t_spawn = myCurrentCamera.transform;

        //Debug.Log(gameObject.name);
        /*if(Camera.current != null)
        {
            
        }*/

        float t_thisShootingStack = 0;
        //float t_aimingReticalScale = 0f;
        //FireAccuracyChange
        if (photonView.IsMine)
        {
            loadout[currentIndex].fireAccuracyStack += loadout[currentIndex].reticleFireScale;
            t_thisShootingStack = loadout[currentIndex].fireAccuracyStack;
            //t_aimingReticalScale = aimingRetical.scale;
        }
        
        

        

        //bloom
        Vector3 t_bloom = new Vector3();
        //t_bloom = t_spawn.position + t_spawn.forward * 1000f;
        //t_bloom += Random.RandomRange
        //StandingBloom
        float t_MyBloomRange = 0;
            if (GetComponent<Player>().playerState == Player.PlayerStates.Standing)
            {
                if (isAiming)
                {
                    //Standing Aiming Accuracy

                    
                    t_MyBloomRange = t_thisShootingStack * loadout[currentIndex].standingAimAccuracyMultiplier 
                    + loadout[currentIndex].fireAccuracyStack * loadout[currentIndex].AccuracyAfterShootingMultiplier;
                    //t_bloom -= t_spawn.position.normalized;
                }
                else
                {
                    //Standing Accuracy
                    
                    t_MyBloomRange = t_thisShootingStack * loadout[currentIndex].standingAccuracyMultiplier//;
                    + loadout[currentIndex].fireAccuracyStack * loadout[currentIndex].AccuracyAfterShootingMultiplier;
                    //t_bloom -= t_spawn.position.normalized;
                    
                    
                }
            }
            else if (GetComponent<Player>().playerState == Player.PlayerStates.Walking)
            {
                if (isAiming)
                {
                //Walking Aiming Accuracy
                t_MyBloomRange = t_thisShootingStack * loadout[currentIndex].walkingAimAccuracyMultiplier
                + loadout[currentIndex].fireAccuracyStack * loadout[currentIndex].AccuracyAfterShootingMultiplier;
                    //t_bloom -= t_spawn.position.normalized;
                }
                else
                {
                //Walking Accuracy
                t_MyBloomRange = t_thisShootingStack * loadout[currentIndex].walkingAccuracyMultiplier
                + loadout[currentIndex].fireAccuracyStack * loadout[currentIndex].AccuracyAfterShootingMultiplier;
                  //  t_bloom -= t_spawn.position.normalized;
                }
            }
            else if (GetComponent<Player>().playerState == Player.PlayerStates.Running)
            {
                //Running Accuracy
                t_MyBloomRange = t_thisShootingStack * loadout[currentIndex].runningAccuracyMultiplier
                + loadout[currentIndex].fireAccuracyStack * loadout[currentIndex].AccuracyAfterShootingMultiplier;
                //t_bloom -= t_spawn.position.normalized;
            }
            else if (GetComponent<Player>().playerState == Player.PlayerStates.Sliding)
            {
                //Sliding Accuracy
                t_MyBloomRange = t_thisShootingStack * loadout[currentIndex].slidingAccuracyMultiplier
                + loadout[currentIndex].fireAccuracyStack * loadout[currentIndex].AccuracyAfterShootingMultiplier;
                //t_bloom -= t_spawn.position.normalized;
            }
        t_MyBloomRange = t_MyBloomRange / 5;

        t_bloom += Random.Range(-t_MyBloomRange, t_MyBloomRange) * Vector3.up;
        t_bloom += Random.Range(-t_MyBloomRange, t_MyBloomRange) * Vector3.right;




        //cooldown
        float t_cooldown = loadout[currentIndex].fireRate;
        if (isAiming && loadout[currentIndex].ADSfireRate != 0)
        {
            t_cooldown = loadout[currentIndex].ADSfireRate;
        }
        currentCooldown = t_cooldown;


        photonView.RPC("SpawnTracer", RpcTarget.All, t_bloom, t_spawn.position, t_spawn.rotation);


        //Profiler.EndSample();
    }
    [PunRPC]
    public void SpawnTracer(Vector3 t_bloom, Vector3 t_position, Quaternion rotation)
    {
        //Spawning Tracer
        // gun fx
        if (currentWeapon != null)
        {
            currentWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0, 0);
            currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickBack;
        }
        Transform t_particleSystObj = currentWeapon.transform.Find("Anchor/Resources/Muzzle Flash");
        if (t_particleSystObj != null)
        {
            ParticleSystem t_particleSyst = t_particleSystObj.GetComponent<ParticleSystem>();
            if (t_particleSyst != null)
            {
                t_particleSyst.Play();
            }
        }

        //t_bloom = Vector3.zero;
        GameObject t_tracer = null;
        if (loadout[currentIndex].specialPref != null)
        {
            t_tracer = Instantiate(loadout[currentIndex].specialPref, t_position, rotation);
        }
        else
        {
            t_tracer = Instantiate(tracer, t_position, rotation);
        }
        

        //4 bullet speed pistols

        //Tracer Properties Init
        Tracer t_tracerComp = t_tracer.GetComponent<Tracer>();

        //t_tracer.GetComponent<Tracer>().goForward = direction;

        t_tracerComp.Init(loadout[currentIndex].bulletSpeed, loadout[currentIndex].damage, this, photonView.IsMine);

        t_tracer.transform.rotation *= (Quaternion.Euler(t_bloom.x, t_bloom.y, 0) );

        //Debug.LogErrorFormat(t_bloom.ToString() + "   " + photonView.ViewID);
    }

    public void FirePlayer(GameObject firedPlayer, bool isDrone, int damage)
    {
        PunFirePlayer(firedPlayer, isDrone, damage);
    }
    [PunRPC]
    public void PunFirePlayer(GameObject firedPlayer, bool isDrone, int damage)
    {
        //Profiler.BeginSample("Pun");
        if (photonView.IsMine)
        {
             
             Player attackingPlayer = GetComponent<Player>();

             firedPlayer.GetPhotonView().RPC("TakeDamage", RpcTarget.All, damage, attackingPlayer.photonView.ViewID);

             if (aimingRetical.tickCor == null)
             {
                aimingRetical.tickCor = StartCoroutine(aimingRetical.ReticleHit(loadout[currentIndex].reticleHitTime));
             }
         }
    }
    [PunRPC]
    private void spawnDrone(int ID)
    {
        
        loadout[currentIndex].instance = PhotonNetwork.GetPhotonView(ID).gameObject;
        loadout[currentIndex].instance.GetComponent<Drone>().weapon = this;

        Camera newCam = loadout[currentIndex].instance.transform.Find("Par Camera/Camera").GetComponent<Camera>();

        myCurrentCamera = newCam;
        GetComponent<Player>().disableMovement = true;
    }

    [PunRPC]
    private void TakeDamage(int p_damage, int attackingPlayer)
    {
        GetComponent<Player>().TakeDamage(p_damage, attackingPlayer, false);
    }
    #endregion

    #region Public Methods
    public void RefreshAmmo(Text p_text)
    {

        int t_clip = loadout[currentIndex].getClip();
        int t_stash = loadout[currentIndex].GetStach();
        p_text.text = t_clip.ToString("D2") + "/" + t_stash.ToString("D2");
    }
    #endregion
}
/*
            if (isAiming)
            {
                BloomModifier -= loadout[currentIndex].standingBloom;
                t_bloom = t_spawn.position + t_spawn.forward * 1000f;
                t_bloom += Random.Range(-BloomModifier, BloomModifier) * t_spawn.up;
                t_bloom += Random.Range(-loadout[currentIndex].standingBloom, loadout[currentIndex].standingBloom) * t_spawn.right;
            }
            else
            {
                t_bloom = t_spawn.position + t_spawn.forward * 1000f;
                t_bloom += Random.Range(-loadout[currentIndex].standingBloom, loadout[currentIndex].standingBloom) * t_spawn.up;
                t_bloom += Random.Range(-loadout[currentIndex].standingBloom, loadout[currentIndex].standingBloom) * t_spawn.right;
            }*/
/*float baseAmountOfBloom = loadout[currentIndex].walkingBloomBase;
float amountAddedOfBloom = loadout[currentIndex].reticleAimWalkingAddedBloom;
if (aimingRetical.scale >= loadout[currentIndex].walkingBloomMax)
{
    amountAddedOfBloom = 0;
}
aimingRetical.scale += amountAddedOfBloom;*/
/*
            if (isAiming)
            {
                t_bloom = t_spawn.position + t_spawn.forward * 1000f;
                t_bloom += Random.Range(-loadout[currentIndex].walkingBloom, loadout[currentIndex].walkingBloom) * t_spawn.up;
                t_bloom += Random.Range(-loadout[currentIndex].walkingBloom, loadout[currentIndex].walkingBloom) * t_spawn.right;
            }
            else
            {
                t_bloom = t_spawn.position + t_spawn.forward * 1000f;
                t_bloom += Random.Range(-loadout[currentIndex].standingBloom, loadout[currentIndex].standingBloom) * t_spawn.up;
                t_bloom += Random.Range(-loadout[currentIndex].standingBloom, loadout[currentIndex].standingBloom) * t_spawn.right;
            }*/
/*float baseAmountOfBloom = loadout[currentIndex].walkingBloomBase;
float amountAddedOfBloom = loadout[currentIndex].reticleAimWalkingAddedBloom;
if (aimingRetical.scale >= loadout[currentIndex].walkingBloomMax)
{
    //amountAddedOfBloom = 0;
}
aimingRetical.scale += amountAddedOfBloom;*/
/*if (shootBloom >= loadout[currentIndex].standingAimBloomMax)
            {
                shootBloom = 0;
                //StartCoroutine(TimeBeforeLerp(loadout[currentIndex].standingAimBloomTimeBeforeLerp));
            }
            shootBloom += loadout[currentIndex].standingBloomSpeed;*/
