using UnityEngine;


[CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
public class Item : ScriptableObject
{

    
    public string name;
    public enum WeaponType
    {
        Gun,
        Drone
    }
    public WeaponType weaponType;
    public GameObject prefab;
    /*[Header("Bloom Variables")]
    public float timeBeforeBloomDecreases;
    public float standingBloom;
    public float walkingBloom;
    public float standingAimBloom;
    public float aimWalkingBloom;
    public float runningBloom;
    public float slidingBloom;*/
    //[Header("Bloom General Variables")]
    [Header("Bloom Base Variables (0 being pixel perfect and 1 is least accurate possible)")]
    [Range(0.0f, 1.0f)]
    public float standingAimBaseBloom = 0.05f;
    [Range(0.0f, 1.0f)]
    public float standingBaseBloom = 0.1f;
    [Range(0.0f, 1.0f)]
    public float WalkingAimBaseBloom = .15f;
    [Range(0.0f, 1.0f)]
    public float walkingBaseBloom = 0.2f;
    [Range(0.0f, 1.0f)]
    public float runningBaseBloom = 0.3f;
    [Range(0.0f, 1.0f)]
    public float slidingBaseBloom = 0.4f;
    [Header("Lerp Values, 0 to 1, 0 being instant and 1 being so slow it never happens")]
    [Range(0.0f, 1.0f)]
    public float bloomSwitchSpeedLerp = 0.95f;
    [Header("Fire accuracy (Does stack)")]
    public float reticleFireScale = 0.5f;
    public float AccuracyAfterShootingMultiplier = 1;
    [Range(0.0f, 1.0f)]
    public float fireResetSpeedLerp = 0.9f;
    //public float jumpingReticleBloom;
    //currently unused
    /*[Header("Bloom Speed Variables")]
    public float standingBloomSpeed;
    public float walkingBloomSpeed;
    public float standingAimBloomSpeed;
    public float walkingAimBloomSpeed;
    public float runningBloomSpeed;
    public float slidingBloomSpeed;*/
    [Header("Accuracy Multipliers (in case you would like the accuracy to be different from the reticle)")]
    public float standingAccuracyMultiplier = 1;
    public float walkingAccuracyMultiplier = 1;
    public float standingAimAccuracyMultiplier = 1;
    public float walkingAimAccuracyMultiplier = 1;
    public float runningAccuracyMultiplier = 1;
    public float slidingAccuracyMultiplier = 1;
    /*[Header("Bloom Time Before Lerp Variables")]
    public float standingBloomTimeBeforeLerp;
    public float walkingBloomTimeBeforeLerp;
    public float standingAimBloomTimeBeforeLerp;
    public float walkingAimBloomTimeBeforeLerp;
    public float runningBloomTimeBeforeLerp;
    public float slidingBloomTimeBeforeLerp;*/
    [Header("Cosmetic")]
    public float recoil = 10;
    public float kickBack = 0.1f;
    public float reticleHitTime = 0.5f;
    [Range(0.0f, 1.0f)]
    public float CostmeticResetSpeedLerp = 0.9f;

    [Header("Specials")]
    [Header("If its not at zero, then it makes it so when you ads you have a diffrent fire rate")]
    public float ADSfireRate = 0;
    
    
    [Header("Essentials")]
    public float fireRate = 0.25f;
    [Range(0.0f, 1.0f)]
    public float aimSpeedLerp = 0.9f;
    public int damage = 300;
    public int ammo = 50;
    public int clipSize = 3;
    public float reloadTime = 0.5f;
    public int burst; // 0 semi, 1 auto, 2+ burst fire
    public float equipTime = 0.35f;
    public float unEquipTime = 0.3f;
    public float aimModifier = 0.5f;
    public float bulletSpeed;
    //public int gunInt;
    [Header("Special Pref (instantiated instiade of bullet (can be a special bullet, or something completely diffrent like a grenade.) just leave null if you don't want a special bullet)")]
    public GameObject specialPref;

    private int stach; // curent ammmo
    private int clip; // current clip

    [System.NonSerialized]
    public bool configureBool1;
    private bool configureBool2;
    [System.NonSerialized]
    public float fireAccuracyStack; //Every time you fire you get more and more inacurate

    [System.NonSerialized]
    public GameObject instance;

    public void Initialize()
    {
        stach = ammo;
        clip = clipSize;
    }
    public bool FireBullet()
    {
        if (clip > 0)
        {
            clip -= 1;
            return true;
        }
        else return false;
    }
    public void Reload()
    {
        stach += clip;
        clip = Mathf.Min(clipSize, stach);
        stach -= clip;
    }

    public int GetStach()
    {
        return stach;
    }
    public int getClip()
    {
        return clip;
    }
}
