using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Drone : MonoBehaviour
{
    public float DistanceBeforeLegMoves;
    public float legOffset;
    //public float DistanceBeforeStopLerp;
    public float LegMoveSpeed;

    public LayerMask ground;

    //public Transform IdealLeg1L;
    //public Transform IdealLeg1R;

    //public Transform Leg1L;
    //public Transform Leg1R;

    public bool shouldLeg1L;

    [SerializeField] float stepOvershootFraction;

    public bool Moving;

    public Transform[] legsTargets;
    public Transform[] idealLegs;


    public Transform Main;

    private Rigidbody rb;

    public int speed;

    public float checkRaduis;

    public Transform groundCheck;

    public int jumpForce;

    public Weapon weapon;

    public float maxHealth = 1000;

    public float currentHealth;


    void Start()
    {
        currentHealth = maxHealth;
        rb = Main.GetComponent<Rigidbody>();
        if (!gameObject.GetPhotonView().IsMine)
        {
            GetComponent<Look>().cams.gameObject.SetActive(false);
            gameObject.layer = 11;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Legs Loop
        for (int i = 0; i < legsTargets.Length; i++)
        {
            if (DistanceBeforeLegMoves <= Vector3.Distance(legsTargets[i].position, idealLegs[i].position))
            {
                StartCoroutine(Move(legsTargets[i], idealLegs[i], i * legOffset));
            }
        }
        if (!gameObject.GetPhotonView().IsMine)
            return;
        

        //Main Body Physics
        /*RaycastHit t_hit;

        if (Physics.Raycast(Main.position, Vector3.down, out t_hit, rangeBeforePropingUp, ground))
        {
            rb.AddForce(Vector3.up * propForce);
        }*/


        bool isJumpingInput = Input.GetKey(KeyCode.Space);
        bool isJumping = isJumpingInput && Physics.CheckSphere(groundCheck.position, checkRaduis, ground);

        float t_hMove = Input.GetAxisRaw("Horizontal");
        float t_vMove = Input.GetAxisRaw("Vertical");

        if (isJumping)
        {
            rb.AddRelativeForce(Vector3.up * jumpForce);
        }

        if (t_hMove != 0 || t_vMove != 0)
        {
            Vector3 t_Direction = new Vector3(t_hMove, 0, t_vMove).normalized;
            t_Direction = transform.TransformDirection(t_Direction);

            Vector3 t_targetVelocity = t_Direction * speed * Time.fixedDeltaTime;
            t_targetVelocity.y = rb.velocity.y;
            rb.velocity = t_targetVelocity;
        }
    }
    //Make it actually take damage instead of insta killing
    [PunRPC]
    public void TakeDamage(int p_damage, int playerPhotonID)
    {
        Player thePLAYER = weapon.player;

        thePLAYER.TakeDamage(p_damage, playerPhotonID, true);
    }

    IEnumerator Move(Transform leg, Transform idealLeg, float distanceBeforeLegMovesModifier)
    {
        //most of this code is from the gecko here https://www.weaverdev.io/blog/bonehead-procedural-animation

        

        Moving = true;

        Vector3 startPoint = leg.position;
        Quaternion startRot = leg.rotation;

        Quaternion endRot = idealLeg.rotation;

        Vector3 idealLegPosition = idealLeg.position;

        RaycastHit t_hit;

        if (Physics.Raycast(idealLegPosition, Vector3.down, out t_hit, Mathf.Infinity, ground))
        {
            idealLegPosition = t_hit.point;
        }

        

         // Directional vector from the foot to the home position
         Vector3 towardHome = (idealLegPosition - leg.position);
        // Total distnace to overshoot by   
        float overshootDistance = DistanceBeforeLegMoves - distanceBeforeLegMovesModifier * stepOvershootFraction;
        Vector3 overshootVector = towardHome * overshootDistance;
        // Since we don't ground the point in this simplified implementation,
        // we restrict the overshoot vector to be level with the ground
        // by projecting it on the world XZ plane.
        overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);

        // Apply the overshoot
        Vector3 endPoint = idealLegPosition + overshootVector;

        // We want to pass through the center point
        Vector3 centerPoint = (startPoint + endPoint) / 2;
        // But also lift off, so we move it up by half the step distance (arbitrarily)
        centerPoint += idealLeg.up * Vector3.Distance(startPoint, endPoint) / 2f;

        float timeElapsed = 0;
        do
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / LegMoveSpeed;

            // Quadratic bezier curve
            leg.position =
              Vector3.Lerp(
                Vector3.Lerp(startPoint, centerPoint, normalizedTime),
                Vector3.Lerp(centerPoint, endPoint, normalizedTime),
                normalizedTime
              );

            leg.rotation = Quaternion.Slerp(startRot, endRot, normalizedTime);

            yield return null;
        }
        while (timeElapsed < LegMoveSpeed);

        Moving = false;
    }
}
