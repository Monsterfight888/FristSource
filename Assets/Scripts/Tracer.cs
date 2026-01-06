using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracer : MonoBehaviour
{
    private float speed;
    private Weapon myWeapon;
    private bool isMine;
    private int damage;


    public BulletTypes thisType = BulletTypes.Normal;
    public enum BulletTypes
    {
        Normal,
        DeflectDirection

    }
    public bool smartAim;
    public float smartAimRaduis;

    public void Init(float t_speed, int t_damage, Weapon t_weapon, bool t_isMine)
    {
        speed = t_speed;
        damage = t_damage;
        myWeapon = t_weapon;
        isMine = t_isMine;
    }

    public void curve(Vector3 point)
    {
        if(Mathf.Sign(Mathf.Abs(transform.position.x) - Mathf.Abs(point.x)) == 1)
        {
            transform.Rotate(new Vector3(0, -90, 0));
        }
        else
        {
            transform.Rotate(new Vector3(0, 90, 0));
        }
        smartAim = true;
        //0.5 bullet speed
    }
    void Start()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
            /*Destroy(gameObject);
            if (collision.collider.gameObject.layer == 13 && isMine)
            {
            //activate the thing that makes it so it goes diffrent direction

            Debug.Log("Shot");
            }
            //Destroy(t_newhole, 5f);
            Debug.Log(collision.gameObject.name);*/
    }
    // Update is called once per frame
    void Update()
    {
        RaycastHit t_hit = new RaycastHit();
        if (Physics.Raycast(transform.position, transform.forward * speed, out t_hit, speed, myWeapon.canBeShot))
        {
            Destroy(gameObject);
            GameObject t_newhole = Instantiate(myWeapon.bulletHolePrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity) as GameObject;
            t_newhole.transform.LookAt(t_hit.point + t_hit.normal);
            Destroy(t_newhole, 5f);

            if (t_hit.collider.gameObject.layer == 11 && isMine)
            {
                if (t_hit.collider.GetComponent<Drone>())
                {
                    myWeapon.FirePlayer(t_hit.collider.gameObject, true, damage);
                }
                else
                {
                    myWeapon.FirePlayer(t_hit.collider.gameObject, false, damage);
                }
            }
            if (t_hit.collider.gameObject.GetComponent<Marching>())
            {
                t_hit.collider.gameObject.GetComponent<Marching>().SetVoxel(t_hit.point, 1);
            }
        }

        LayerMask tracers = LayerMask.GetMask("Tracer");

        if (Physics.Raycast(transform.position, transform.forward * speed, out t_hit, speed, tracers))
        {
            Destroy(gameObject);
            Debug.Log("Shot");
            if(t_hit.collider.GetComponent<Tracer>().thisType == BulletTypes.DeflectDirection)
            {
                t_hit.collider.GetComponent<Tracer>().curve(t_hit.point);
            }
            

        }
        if (smartAim)
        {
            Collider[] Objs = Physics.OverlapSphere(transform.position, smartAimRaduis, myWeapon.canBeShot);

            for (int i = 0; i < Objs.Length; i++)
            {
                if (Objs[i].gameObject.GetComponent<Player>())
                {
                    //damage multiplier (probably in due of replacing with more dynamic system)

                    damage *= 2;

                    myWeapon.FirePlayer(Objs[i].gameObject, true, damage);
                    Destroy(gameObject);

                }
                else if(Objs[i].gameObject.GetComponent<Drone>())
                {
                    damage *= 2;
                    myWeapon.FirePlayer(Objs[i].gameObject, false, damage);
                    Destroy(gameObject);
                }
            }
        }
        transform.position = (transform.position + (transform.forward * speed));
        //transform.Translate(goForward * speed);
    }
}
