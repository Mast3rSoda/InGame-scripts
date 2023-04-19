using UnityEngine;

public class scr_PlayerMovement : MonoBehaviour
{

    //Components
    [Header("Components")]
    private Rigidbody rb;

    //Assignable
    [Header("Assignable")]
    [SerializeField] private Transform playerCam;
    [SerializeField] private LayerMask ground;
    [SerializeField] private LayerMask gun;
    [SerializeField] private GameObject gunHolder;

    //Move Variables
    [Header("Move variables")]
    private float moveSpeed = 25f;
    private float maxSpeed = 14f;
    private float defaultMaxSpeed = 14f;
    private float stopThreshold = 0.2f;
    private float slopeStopThreshold = 1.4f;

    //CounterMove Variables
    [Header("CounterMove variables")]
    private float counterMovementMult = 1.5f;
    private float counterThreshold = 0.05f;


    //Jump Variables
    [Header("Jump variables")]
    private float jumpSpeed = 30f;
    private float jumpCooldown = 0.4f;
    private bool isGrounded = false;
    private bool readyToJump = true;

    //Walljump Variables
    [Header("Walljump variables")]
    private Vector3 walljumpDirection = Vector3.zero;
    private Vector3 lastWallJumpDir = Vector3.zero;
    private float walljumpDegThreshold = 5f;
    private float walljumpForce = 35f;

    //Crouch Variables
    [Header("Crouch variables")]
    private bool isCrouching = false;
    private float crouchVelMult = 1f;
    private float defaultCrouchVelMult = 0.7f;
    private float maxCrouchSpeed = 5f;

    //SlopeCheck Variables
    [Header("SlopeCheck variables")]
    private float minSlopeAngle = 0.7f;
    private bool isOnGroundSlope = false;

    //Slide Variables
    [Header("Slide variables")]
    private bool isSliding = false;
    private float slideThreshold = 8f;
    private float slideVelMult = 0.3f;

    //Scales
    [Header("Scales")]
    private Vector3 playerScale;
    private Vector3 crouchScale = new(1f, 0.5f, 1f);

    //Collision Variables
    [Header("Collisions")]
    private Vector3 collisionNormal;
    private float collisionDegree = -1f;

    //Other
    [Header("Other")]
    private float gravityForce = 12f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;

    }

    // Update is called once per frame
    void Update()
    {
        RotatePlayer();
    }

    private void FixedUpdate()
    {
        if (collisionDegree == -1)
            isGrounded = false;

        if (collisionDegree != -1)
        {
            //while not touching ground get the walljump direction and slopecheck
            if (collisionDegree > 45f && collisionDegree <= 90f)
            {
                isGrounded = isOnGroundSlope = false;

                if (isCrouching)
                    isSliding = true;

                walljumpDirection = collisionNormal;
                //Debug.Log("walljumpDirection: " + walljumpDirection);

                float yDir = walljumpDirection.y;
                walljumpDirection.y = 0.0f;


                //Slope check!
                SlopeSlide(walljumpDirection, yDir);
            }

            //if touching ground
            if (collisionDegree <= 45f)
            {
                walljumpDirection = lastWallJumpDir = Vector3.zero;

                isOnGroundSlope = collisionDegree > 0;

                //if (isCrouching && !isGrounded && collisionDegree >= slideDegThreshold)
                //    isSliding = true;

                isGrounded = true;

                if (isCrouching)
                    maxSpeed = maxCrouchSpeed;

                if (isSliding && rb.velocity.y >= 0 && rb.velocity.magnitude < slideThreshold)
                    isSliding = false;

            }
        }



        //Debug.Log("CollisionDegree: " + collisionDegree);
        //Debug.Log("CollisionNormal: " + collisionNormal);
        collisionDegree = -1;

    }


    public void ProcessMove(Vector2 input)
    {
        //Debug.Log(isGrounded);
        //Debug.Log(rb.velocity);
        //Debug.Log(readyToJump);
        //Debug.Log("collisionNormal: " + collisionNormal);
        //Debug.Log("collisionDegree: " + collisionDegree);

        //if (isOnGroundSlope)
        //    collisionDegree = Vector3.Angle(collisionNormal, Vector3.up);

        if (isSliding)
        {
            Vector3 moveDir = new(rb.velocity.x, 0.0f, rb.velocity.z);
            rb.AddForce(moveDir.magnitude * 0.22f * moveSpeed * slideVelMult * -moveDir.normalized, ForceMode.Force);
            return;
        }

        //get the input
        Vector3 moveVector = new(input.x, 0.0f, input.y);

        //get the force values based on camera rotation
        moveVector = moveVector.x * playerCam.right + moveVector.z * playerCam.forward;
        moveVector.y = 0.0f;
        moveVector.Normalize();

        // uhhhh... y = moveScalar * -moveVector.x * collisionNormal.x <- we need to do something with collisionNormal, cause the sign is mucho importante
        // add the z axis to y as well cause one of these will be 0 checked | should work the way it is written
        // I'm smart..

        float moveScalar = isGrounded ? collisionDegree / 90f : 0.0f;
        float mVx = moveVector.x, mVz = moveVector.z;

        moveVector *= 1 - moveScalar;
        moveVector.y = moveScalar * -mVx * GetSign(collisionNormal.x) +
                       moveScalar * -mVz * GetSign(collisionNormal.z);

        //Debug.Log(moveVector);

        if (moveVector.magnitude > 0)
            rb.useGravity = true;

        //we add the force
        rb.AddForce(crouchVelMult * moveSpeed * moveVector, ForceMode.Acceleration);

        CounterMovement(moveVector);

        //max speed check
        SpeedCheck();

    }

    private void RotatePlayer()
    {
        transform.localEulerAngles = new Vector3(0f, playerCam.eulerAngles.y, 0f);
    }

    private int GetSign(float x)
    {
        if (x < -0.01f)
            return -1;
        if (x > 0.01)
            return 1;
        return 0;
    }

    //Uhhhh.. this is hard
    //AHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH
    //i think it's done? - not really, no
    //now it seems like it's done
    private void CounterMovement(Vector3 moveVector)
    {
        if (!isGrounded) return;

        Vector3 flatVel = new(rb.velocity.x, 0f, rb.velocity.z);

        //Debug.Log("flatVel: " + flatVel.normalized);
        //Debug.Log("moveVector: " + moveVector);

        //on ground counter movement
        if (collisionDegree == 0)
        {

            //when no keypress and low velocity -> stop player
            if (moveVector.magnitude <= counterThreshold && flatVel.magnitude <= stopThreshold && flatVel.magnitude > 0.0001f)
            {
                rb.velocity = Vector3.zero;
                return;
            }

            //when no keypress and high velocity -> decrease velocity
            if (moveVector.magnitude <= counterThreshold && flatVel.magnitude > stopThreshold)
            {
                rb.AddForce(moveSpeed * counterMovementMult * -flatVel);
                return;
            }

            //when different keypress than move direction and high velocity -> add force to change direction
            if ((moveVector - flatVel.normalized).magnitude > counterThreshold && flatVel.magnitude > stopThreshold)
            {
                rb.AddForce(counterMovementMult * flatVel.magnitude * moveSpeed * (moveVector - flatVel.normalized));
                return;
            }
            return;
        }

        //on slope counter movement

        //when no keypress and low velocity -> stop player
        if (moveVector.magnitude <= counterThreshold && rb.velocity.magnitude <= slopeStopThreshold)
        {
            rb.velocity = Vector3.zero;
            rb.useGravity = false;
            return;
        }

        //when no keypress and high velocity -> decrease velocity
        if (moveVector.magnitude <= counterThreshold && rb.velocity.magnitude > slopeStopThreshold)
        {
            rb.AddForce(moveSpeed * counterMovementMult * -rb.velocity);
            return;
        }

        flatVel.Normalize();

        float moveScalar = isOnGroundSlope ? collisionDegree / 90f : 0.0f;
        float fVx = flatVel.x, fVz = flatVel.z;

        flatVel *= 1 - moveScalar;
        flatVel.y = moveScalar * -fVx * GetSign(collisionNormal.x) +
                    moveScalar * -fVz * GetSign(collisionNormal.z);


        //we can't subtract the flatvel cause of the y value
        //now we can cause I've rewritten like 80% of the code again
        //but I'm finally happy with how it's working

        //Debug.Log(moveVector - flatVel);
        //when different keypress than move direction and high velocity->add force to change direction
        if ((moveVector - flatVel).magnitude > counterThreshold && rb.velocity.magnitude > slopeStopThreshold)
        {
            rb.AddForce(counterMovementMult * rb.velocity.magnitude * moveSpeed * (moveVector - flatVel));
            return;
        }

    }

    // need to work on the speedcheck
    private void SpeedCheck()
    {
        Vector3 velocityVector = isOnGroundSlope ? rb.velocity : new(rb.velocity.x, 0f, rb.velocity.z);
        if (velocityVector.magnitude <= maxSpeed) return;

        rb.AddForce(moveSpeed * -velocityVector.normalized, ForceMode.Acceleration);
    }

    public void StartCrouch()
    {
        isCrouching = true;
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        crouchVelMult = defaultCrouchVelMult;
        if (rb.velocity.magnitude >= slideThreshold)
            isSliding = true;
    }
    public void StopCrouch()
    {
        isCrouching = isSliding = false;
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        crouchVelMult = 1f;
        maxSpeed = defaultMaxSpeed;
    }

    public void ProcessJump()
    {
        if (readyToJump && (isGrounded || walljumpDirection != Vector3.zero))
        {
            if (lastWallJumpDir != Vector3.zero && Vector3.Angle(walljumpDirection, lastWallJumpDir) < walljumpDegThreshold)
                return;

            readyToJump = false;
            lastWallJumpDir = walljumpDirection;
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y < 0.0f ? 0.0f : rb.velocity.y / 5, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpSpeed + walljumpDirection * walljumpForce, ForceMode.Impulse);

            Invoke(nameof(ResetJump), jumpCooldown);
        }

    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if ((ground.value | (1 << collision.gameObject.layer)) != ground.value) return;

        //Debug.Log(collision.gameObject.name);


        Vector3 localCollisionNormal;
        float localCollisionDegree;


        for (int i = 0; i < collision.contactCount; i++)
        {
            //get the collision normal and degree
            localCollisionNormal = collision.GetContact(i).normal;
            localCollisionDegree = Vector3.Angle(localCollisionNormal, Vector3.up);
            //Debug.Log("LCN: " + localCollisionNormal + " LCD: " + localCollisionDegree);

            if (localCollisionDegree < collisionDegree || collisionDegree == -1)
            {
                collisionDegree = localCollisionDegree;
                collisionNormal = localCollisionNormal;
            }
        }
        //Debug.Log("CN: " + collisionNormal + " CD: " + collisionDegree);
    }

    private void SlopeSlide(Vector3 walljumpDirection, float yDir)
    {

        //if degree is too little don't add the down force - due to wall jumping - 0.18 equals to 80 deg
        if (yDir < 0.18f) return;

        float xAbsValue = Mathf.Abs(walljumpDirection.x);
        float zAbsValue = Mathf.Abs(walljumpDirection.z);

        //Debug.Log("x: " + xAbsValue + "z: " + zAbsValue);

        //get the values if we are on a slope, otherwise set the value to 0 - y value limited so that we don't fall too quickly on big slopes
        Vector3 forceDownDir = new(xAbsValue > minSlopeAngle ? xAbsValue - 1 : 0,
                                   yDir > 0.55f ? yDir - 1 : -0.45f,
                                   zAbsValue > minSlopeAngle ? zAbsValue - 1 : 0);

        if (walljumpDirection.x > 0) forceDownDir.x *= -1;
        if (walljumpDirection.z > 0) forceDownDir.z *= -1;

        //Debug.Log("ForceDownDir: " + forceDownDir);

        if (rb.velocity.y > 0)
        {
            rb.AddForce(gravityForce * moveSpeed * forceDownDir, ForceMode.Force);
            return;
        }
        rb.AddForce(moveSpeed / 2f * gravityForce * forceDownDir, ForceMode.Force);
    }

    //OOF wanky groundCheck - will fix later maybe?
    //will need to look into it
    private void OnCollisionExit(Collision collision)
    {
        if ((ground.value | (1 << collision.gameObject.layer)) != ground.value) return;

        //we can use readyToJump to force the player down after running up a slope | dont forget to use the pos y value
        //if (readyToJump && rb.velocity.y > 0)
        //    rb.AddForce(rb.velocity.y * 0.75f * Vector3.down, ForceMode.VelocityChange);
        rb.useGravity = true;
        isGrounded = isOnGroundSlope = false;
        walljumpDirection = collisionNormal = Vector3.zero;
    }

}
