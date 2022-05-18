///This moves the character and is like the main thing
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//I like being a special snowflake and I used physics.gravity instead of rigid physics movement
[RequireComponent(typeof(Rigidbody))]
public class SFPSC_PlayerMovement : MonoBehaviour
{
    private static Vector3 vecZero = Vector3.zero;
    private Rigidbody rb;

    private bool enableMovement = true;

    [Header("Movement properties")]
    public float walkSpeed = 8.0f;
    public float runSpeed = 12.0f;
    public float changeInStageSpeed = 10.0f; //This chages speed using a lerp, suprisingly this took my multiple hours to figure out
    public float maximumPlayerSpeed = 150.0f;
    [HideInInspector] public float vInput, hInput;
    public Transform groundChecker;
    public float groundCheckerDist = 0.2f;

    [Header("Jump")]
    public float jumpForce = 500.0f;
    public float jumpCooldown = 1.0f;
    private bool jumpBlocked = false;

    private SFPSC_WallRun wallRun;
    private SFPSC_GrapplingHook grapplingHook;



    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();

        TryGetWallRun();
        TryGetGrapplingHook();
    }

    public void TryGetWallRun()
    {
        this.TryGetComponent<SFPSC_WallRun>(out wallRun);
    }

    public void TryGetGrapplingHook()
    {
        this.TryGetComponent<SFPSC_GrapplingHook>(out grapplingHook);
    }

    private bool isGrounded = false;
    public bool IsGrounded { get { return isGrounded; } }

    private Vector3 inputForce;
    private int i = 0;
    private float prevY;
    private void FixedUpdate()
    {
        if ((wallRun != null && wallRun.IsWallRunning) || (grapplingHook != null && grapplingHook.IsGrappling))
            isGrounded = false;
        else
        {
            //MAKES IT FASTER
            isGrounded = (Mathf.Abs(rb.velocity.y - prevY) < .1f) && (Physics.OverlapSphere(groundChecker.position, groundCheckerDist).Length > 1); 
            prevY = rb.velocity.y;
        }

       
        vInput = Input.GetAxisRaw("Vertical");
        hInput = Input.GetAxisRaw("Horizontal");

     
        rb.velocity = ClampMag(rb.velocity, maximumPlayerSpeed);

        if (!enableMovement)
            return;
        inputForce = (transform.forward * vInput + transform.right * hInput).normalized * (Input.GetKey(SFPSC_KeyManager.Run) ? runSpeed : walkSpeed);

        if (isGrounded)
        {
        
            if (Input.GetButton("Jump") && !jumpBlocked)
            {
                rb.AddForce(-jumpForce * rb.mass * Vector3.down);
                jumpBlocked = true;
                Invoke("UnblockJump", jumpCooldown);
            }
     

            //AHHHHHHHHH
            //I don't know what this does but it works
            rb.velocity = Vector3.Lerp(rb.velocity, inputForce, changeInStageSpeed * Time.fixedDeltaTime);
        }
        else

            rb.velocity = ClampSqrMag(rb.velocity + inputForce * Time.fixedDeltaTime, rb.velocity.sqrMagnitude);
    }

    private static Vector3 ClampSqrMag(Vector3 vec, float sqrMag)
    {
        if (vec.sqrMagnitude > sqrMag)
            vec = vec.normalized * Mathf.Sqrt(sqrMag);
        return vec;
    }

    private static Vector3 ClampMag(Vector3 vec, float maxMag)
    {
        if (vec.sqrMagnitude > maxMag * maxMag)
            vec = vec.normalized * maxMag;
        return vec;
    }

    #region Previous Ground Check
    /*private void OnCollisionStay(Collision collision)
    {
        isGrounded = false;
        Debug.Log(collision.contactCount);
        for(int i = 0; i < collision.contactCount; ++i)
        {
            if (Vector3.Dot(Vector3.up, collision.contacts[i].normal) > .2f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }*/
    #endregion

    private void UnblockJump()
    {
        jumpBlocked = false;
    }
    
    

    public void EnableMovement()
    {
        enableMovement = true;
    }

    //People who can't move exist, so sometimes you can't fucking move

    public void DisableMovement()
    {
        enableMovement = false;
    }
}
