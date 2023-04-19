using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class scr_EnemyAI : MonoBehaviour, IDamage
{
    private NavMeshAgent navMeshAgent;

    private Transform playerTransform;
    private CapsuleCollider playerCollider;
    private Rigidbody playerRB;

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstructionLayer;
    [SerializeField] private LayerMask gunLayer;
    [SerializeField] private Transform headLocation;
    [SerializeField] private GameObject[] bones;
    [SerializeField] private GameObject leftArm;
    [SerializeField] private GunData gunData;

    private Vector3 centerLocation;
    private Vector3 playerFloorLocation;
    private Quaternion previousRotation = Quaternion.identity;

    private Animator animator;
    private Animator gunAnimator;

    private float FOVradius = 25f;
    private float FOVangle = 120f;

    private float walkDistance = 25f;
    private float defaultMoveSpeed = 10f;

    private float ribRotation = 0f;

    private Vector3 playerVel = Vector3.zero;

    private bool canSeePlayer = false;
    private bool isAwareOfPlayer = false;
    private bool canPerformAction = true;
    private bool canRoll = true;
    private bool canTurnUp = false;

    private Vector3 hitPosition;

    public static System.Action hitAction;

    private System.Action playerDeath;

    /// <summary>
    /// State value to control the FSM.
    /// 0 - Rotate
    /// 1 - Patroling
    /// 2 - Aware
    /// 3 - Chasing
    /// 4 - Shooting
    /// 5 - Repositioning
    /// 6 - Searching
    /// 7 - Death?
    /// 
    /// </summary>
    private int state = 0;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindWithTag("Player").transform;
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();
        playerCollider = playerTransform.GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        gunAnimator = GetComponentsInChildren<Animator>()[1];
        playerDeath = () => DisableAI();
        scr_PlayerHealth.playerDeath += playerDeath;
    }

    private void OnDestroy()
    {
        scr_PlayerHealth.playerDeath -= playerDeath;

    }

    private void FixedUpdate()
    {
        centerLocation = transform.position;
        centerLocation.y = transform.position.y + 1;
        playerFloorLocation = playerTransform.position;
        playerFloorLocation.y = playerTransform.position.y - 1;
        FieldOfViewCheck();
        AwarenessCheck();

        //Debug.Log(state);

        switch (state)
        {
            case 0:
                RotateState();
                break;
            case 1:
                PatrolingState();
                break;
            case 2:
                AwareState();
                break;
            case 3:
                ChasingState();
                break;
            case 4:
                ShootingState();
                break;
            case 5:
                RepositioningState();
                break;
            case 6:
                SearchingState();
                break;
            case 7:
                DeathState();
                return;

        }

        ExecuteAnimations();

    }

    private void ExecuteAnimations()
    {

        Vector3 velVector = transform.InverseTransformDirection(navMeshAgent.velocity);
        velVector.y = 0.0f;

        Vector3 temp = velVector;

        velVector.Normalize();

        velVector *= temp.magnitude / navMeshAgent.speed * 1.5f;

        animator.SetFloat("xAxis", velVector.x);
        animator.SetFloat("zAxis", velVector.z);

        if (canTurnUp && targetAngle != Quaternion.identity)
        {
            if (targetAngle.eulerAngles.x > 180)
                ribRotation = Mathf.Lerp(ribRotation, targetAngle.eulerAngles.x - 360, (navMeshAgent.angularSpeed / 30) * Time.fixedDeltaTime);
            else
                ribRotation = Mathf.Lerp(ribRotation, targetAngle.eulerAngles.x, (navMeshAgent.angularSpeed / 30) * Time.fixedDeltaTime);
        }
        else if (!canTurnUp)
            ribRotation = Mathf.Lerp(ribRotation, 0f, (navMeshAgent.angularSpeed / 40) * Time.fixedDeltaTime);

        animator.SetFloat("yAxis", ribRotation);

        if (velVector.magnitude > 0f)
        {
            previousRotation = Quaternion.identity;
            animator.SetFloat("Turn", 0f);
            return;
        }

        if (previousRotation == Quaternion.identity)
        {
            previousRotation = transform.rotation;
            return;
        }

        animator.SetFloat("Turn", (transform.rotation.eulerAngles.y - previousRotation.eulerAngles.y) / (navMeshAgent.angularSpeed * Time.fixedDeltaTime));
        previousRotation = transform.rotation;

    }

    private void FieldOfViewCheck()
    {
        Collider[] foundColliders = Physics.OverlapSphere(headLocation.position, FOVradius, playerLayer);

        if (foundColliders.Length != 0)
        {
            Vector3 playerPosition = foundColliders[0].transform.position;
            Vector3 playerDirection = (playerPosition - centerLocation).normalized;

            if (Vector3.Angle(headLocation.forward, playerDirection) < FOVangle / 2)
            {
                float distanceToPlayer = Vector3.Distance(centerLocation, playerPosition);

                if (!Physics.Raycast(centerLocation, playerDirection, distanceToPlayer, obstructionLayer, QueryTriggerInteraction.Ignore))
                {
                    canSeePlayer = true;
                    isAwareOfPlayer = false;
                    return;
                }
            }
        }
        canSeePlayer = false;
    }

    private void AwarenessCheck()
    {
        if (isAwareOfPlayer || !canRoll || canSeePlayer) return;

        Collider[] foundColliders = Physics.OverlapSphere(headLocation.position, FOVradius * 1.5f, playerLayer);

        if (foundColliders.Length == 0) return;

        if (Random.value < 0.3f)
            isAwareOfPlayer = true;
        else
            Invoke(nameof(RollReset), 1f);

        canRoll = false;
    }

    Quaternion targetAngle = Quaternion.identity;

    private void RotateState()
    {
        if (canSeePlayer)
        {
            targetAngle = previousRotation = Quaternion.identity;
            animator.SetFloat("Turn", 0);
            state = 3;
            canPerformAction = false;
            navMeshAgent.speed = defaultMoveSpeed;
            Invoke(nameof(CanPerformActionReset), 0.3f);
            return;
        }

        if (!canPerformAction) return;

        if (targetAngle == Quaternion.identity)
        {
            targetAngle = Quaternion.Euler(0f, Random.Range(0f, 359f), 0f);
            return;
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetAngle, navMeshAgent.angularSpeed / 2 * Time.fixedDeltaTime);

        if (transform.rotation == targetAngle)
        {
            canPerformAction = false;
            targetAngle = previousRotation = Quaternion.identity;
            if (Random.value < 0.5)
                state = 1;

        }

        if (isAwareOfPlayer)
        {
            canPerformAction = false;
            targetAngle = previousRotation = Quaternion.identity;
            animator.SetFloat("Turn", 0);
            state = 2;
        }

        Invoke(nameof(CanPerformActionReset), Random.Range(1f, 2f));

    }

    private void CanPerformActionReset()
    {
        canPerformAction = true;
    }

    Vector3 targetLocation = Vector3.zero;

    private void PatrolingState()
    {

        if (canSeePlayer)
        {
            targetLocation = Vector3.zero;
            state = 3;
            canPerformAction = false;
            navMeshAgent.speed = defaultMoveSpeed;
            Invoke(nameof(CanPerformActionReset), 0.3f);
            return;
        }

        if (!canPerformAction || navMeshAgent.pathPending)
            return;

        if (targetLocation != Vector3.zero)
        {
            if (isAwareOfPlayer)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
                navMeshAgent.isStopped = false;
                canPerformAction = false;
                targetLocation = Vector3.zero;
                state = 2;
                Invoke(nameof(CanPerformActionReset), Random.Range(1f, 2f));
            }


            if (Vector3.Distance(transform.position, targetLocation) <= 2)
            {
                canPerformAction = false;
                targetLocation = Vector3.zero;
                navMeshAgent.speed = defaultMoveSpeed;

                if (Random.value < 0.5)
                    state = 0;

                Invoke(nameof(CanPerformActionReset), Random.Range(1f, 2f));
            }
            return;
        }
        navMeshAgent.speed = (defaultMoveSpeed / 2f);
        Vector3 randomPosition = (Random.insideUnitSphere * walkDistance) + transform.position;
        NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, walkDistance, navMeshAgent.areaMask);
        targetLocation = hit.position;
        navMeshAgent.SetDestination(targetLocation);



    }

    private bool canGo = false;

    private void AwareState()
    {

        if (canSeePlayer)
        {
            targetLocation = Vector3.zero;
            targetAngle = Quaternion.identity;
            state = 3;
            canPerformAction = false;
            navMeshAgent.speed = defaultMoveSpeed;
            Invoke(nameof(CanPerformActionReset), 0.3f);
            return;
        }

        if (!canPerformAction || !isAwareOfPlayer)
            return;

        if (targetAngle == Quaternion.identity && targetLocation == Vector3.zero)
        {
            targetAngle = Quaternion.LookRotation(playerTransform.position - centerLocation);
            Vector3 randomPosition = (Random.insideUnitSphere * (Vector3.Distance(centerLocation, playerTransform.position) / 2)) + ((transform.position + playerFloorLocation) / 2);
            NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, (Vector3.Distance(centerLocation, playerTransform.position) / 2), navMeshAgent.areaMask);
            targetLocation = hit.position;
            Invoke(nameof(CanGoReset), Random.Range(2f, 3f));
        }
        if (Quaternion.Angle(headLocation.rotation, targetAngle) > 30f && targetAngle != Quaternion.identity)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetAngle, navMeshAgent.angularSpeed * Time.fixedDeltaTime);
            canTurnUp = true;
        }
        else
            targetAngle = Quaternion.identity;

        if (canGo)
        {
            navMeshAgent.speed = defaultMoveSpeed / 2;
            navMeshAgent.SetDestination(targetLocation);
            canGo = false;
        }

        if (Vector3.Distance(transform.position, targetLocation) <= 2f)
        {
            canPerformAction = isAwareOfPlayer = false;
            navMeshAgent.speed = defaultMoveSpeed;
            targetLocation = Vector3.zero;
            canTurnUp = false;
            Invoke(nameof(RollReset), 3f);

            if (Random.value < 0.5)
                state = 0;
            else
                state = 1;

            Invoke(nameof(CanPerformActionReset), Random.Range(1f, 2f));
        }


    }

    private void RollReset()
    {
        canRoll = true;
    }

    private void CanGoReset()
    {
        canGo = true;
    }


    private void ChasingState()
    {

        if (!canSeePlayer)
        {
            navMeshAgent.updateRotation = true;
            targetAngle = Quaternion.identity;
            targetLocation = playerFloorLocation;
            playerVel = playerRB.velocity;
            state = 6;
            return;
        }

        navMeshAgent.updateRotation = false;
        targetAngle = Quaternion.LookRotation(playerTransform.position - centerLocation);
        canTurnUp = true;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetAngle, navMeshAgent.angularSpeed * 1.6f * Time.fixedDeltaTime);

        if (!canPerformAction || navMeshAgent.pathPending) return;

        if (Vector3.Distance(centerLocation, playerTransform.position) <= (FOVradius / 2) && targetLocation == Vector3.zero)
        {
            state = 4;
            canPerformAction = false;
            Invoke(nameof(CanPerformActionReset), 0.2f);
            return;
        }

        else if (Vector3.Distance(centerLocation, playerTransform.position) > (FOVradius / 2) && targetLocation == Vector3.zero)
        {
            Vector3 randomPosition = (Random.insideUnitSphere * (FOVradius / 5)) + ((transform.position + playerFloorLocation) / 2f);
            NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, FOVradius / 3, navMeshAgent.areaMask);
            targetLocation = hit.position;
            navMeshAgent.SetDestination(targetLocation);
            return;
        }

        if (Vector3.Distance(transform.position, targetLocation) <= 2)
        {
            targetLocation = Vector3.zero;
            state = 4;
            canPerformAction = false;
            Invoke(nameof(CanPerformActionReset), 0.2f);
        }

        if (Vector3.Distance(transform.position, targetLocation) > 2 && navMeshAgent.velocity.magnitude == 0)
            targetLocation = Vector3.zero;

    }

    private bool canShoot = true;

    private void ShootingState()
    {

        navMeshAgent.updateRotation = false;
        canTurnUp = true;
        targetAngle = Quaternion.LookRotation(playerTransform.position - centerLocation);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetAngle, navMeshAgent.angularSpeed * 2f * Time.fixedDeltaTime);

        if (!canPerformAction) return;

        if (!canShoot)
        {
            if (Vector3.Distance(centerLocation, playerTransform.position) <= FOVradius / 2)
            {
                state = 5;
                return;
            }

            state = 3;
        }

        if (Quaternion.Angle(headLocation.rotation, targetAngle) > 5f) return;

        float playerVelMag = playerRB.velocity.magnitude;

        if (Physics.Raycast(headLocation.position, new(
                            headLocation.forward.x + Random.Range(-playerVelMag / 115, playerVelMag / 115),
                            headLocation.forward.y + Random.Range(-playerVelMag / 115, playerVelMag / 115),
                            headLocation.forward.z + Random.Range(-playerVelMag / 115, playerVelMag / 115)),
                            out RaycastHit raycastHit, gunData.maxDistance))
        {
            if (raycastHit.transform.TryGetComponent(out IDamage damage))
            {
                damage.Damage(raycastHit.point);
                hitAction?.Invoke();
            }
        }

        targetAngle = Quaternion.identity;
        gunAnimator.SetTrigger("Fire");
        animator.SetTrigger("Shooting");
        canShoot = false;
        Invoke(nameof(ShotReset), 1.5f);

        if (Vector3.Distance(centerLocation, playerTransform.position) <= FOVradius / 2)
        {
            state = 5;
            canPerformAction = false;
            Invoke(nameof(CanPerformActionReset), 0.2f);
            return;
        }

        state = 3;
        canPerformAction = false;
        Invoke(nameof(CanPerformActionReset), 0.2f);


    }

    private void ShotReset()
    {
        canShoot = true;
    }

    private void RepositioningState()
    {
        if (!canSeePlayer)
        {
            navMeshAgent.updateRotation = true;
            targetAngle = Quaternion.identity;
            targetLocation = playerFloorLocation;
            playerVel = playerRB.velocity;
            state = 6;
            return;
        }

        targetAngle = Quaternion.LookRotation(playerTransform.position - centerLocation);
        canTurnUp = true;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetAngle, navMeshAgent.angularSpeed * 1.6f * Time.fixedDeltaTime);

        if (!canPerformAction || navMeshAgent.pathPending) return;

        if (targetLocation != Vector3.zero)
        {

            if (Vector3.Distance(transform.position, targetLocation) <= 2)
            {
                targetLocation = Vector3.zero;
                state = 4;
                canPerformAction = false;
                Invoke(nameof(CanPerformActionReset), 0.2f);
                return;
            }
            if (Vector3.Distance(transform.position, targetLocation) > 2 && navMeshAgent.velocity.magnitude == 0)
                targetLocation = Vector3.zero;
            return;
        }

        Vector3 randomPosition = (Random.insideUnitSphere * (FOVradius / 5)) + ((transform.position + playerFloorLocation) / 2);
        NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, FOVradius / 3, navMeshAgent.areaMask);
        targetLocation = hit.position;
        navMeshAgent.SetDestination(targetLocation);
    }


    private void SearchingState()
    {

        if (canSeePlayer)
        {
            targetLocation = Vector3.zero;
            targetAngle = Quaternion.identity;
            state = 3;
            canPerformAction = false;
            navMeshAgent.speed = defaultMoveSpeed;
            Invoke(nameof(CanPerformActionReset), 0.2f);
            return;
        }

        if (!canPerformAction || navMeshAgent.pathPending) return;

        if (navMeshAgent.destination != targetLocation && targetLocation != Vector3.zero)
            navMeshAgent.SetDestination(targetLocation);

        if (Vector3.Distance(transform.position, targetLocation) <= 2 && targetLocation != Vector3.zero)
        {
            navMeshAgent.updateRotation = false;

            if (playerVel != Vector3.zero)
                targetAngle = Quaternion.LookRotation(playerVel, Vector3.up);
            else
                targetAngle = Quaternion.LookRotation(targetLocation, Vector3.up);

            canTurnUp = true;
            targetLocation = Vector3.zero;
            playerVel = Vector3.zero;
            return;
        }
        else if (Vector3.Distance(transform.position, targetLocation) > 2 && targetLocation != Vector3.zero)
            return;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetAngle, navMeshAgent.angularSpeed * 1.5f * Time.fixedDeltaTime);

        if (Quaternion.Angle(headLocation.rotation, targetAngle) < 5f || targetAngle == Quaternion.identity)
        {
            targetAngle = Quaternion.identity;

            if (Random.value < 0.5)
                state = 0;
            else
                state = 1;

            canTurnUp = false;
            navMeshAgent.updateRotation = true;
            canPerformAction = false;
            Invoke(nameof(RollReset), Random.Range(2f, 3f));
            Invoke(nameof(CanPerformActionReset), Random.Range(1f, 2f));
        }


    }

    private void DeathState()
    {


        animator.enabled = false;

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
        navMeshAgent.enabled = false;

        transform.GetComponent<Rigidbody>().isKinematic = false;

        GameObject gun = gunAnimator.gameObject.transform.parent.gameObject;

        gun.transform.SetParent(null);
        Rigidbody gunRB = gun.AddComponent<Rigidbody>();
        gunRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
        gunRB.angularDrag = 0f;
        gunRB.mass = 2f;
        gun.GetComponent<BoxCollider>().enabled = true;
        gun.transform.Find("GunTrigger").GetComponent<BoxCollider>().enabled = true;
        float gunX = gun.transform.position.x,
              gunY = gun.transform.position.y,
              gunZ = gun.transform.position.z;
        gunRB.AddExplosionForce(15f, new(Random.Range(gunX - 0.4f, gunX + 0.4f),
                                        Random.Range(gunY - 0.1f, gunY - 0.4f),
                                        Random.Range(gunZ - 0.4f, gunZ - 0.4f)), 3f, 0f, ForceMode.Impulse);

        gunRB.AddTorque(new Vector3(Random.Range(1f, 5f), Random.Range(1f, 5f), Random.Range(1f, 5f)), ForceMode.Impulse);


        gameObject.GetComponent<CapsuleCollider>().enabled = false;

        foreach (GameObject bone in bones)
        {
            CapsuleCollider cC = bone.GetComponent<CapsuleCollider>();
            cC.enabled = true;
            Physics.IgnoreCollision(cC, playerCollider, true);
            Rigidbody rb = bone.AddComponent<Rigidbody>();
            rbs.Add(rb);
            rb.mass = 1f;
            rb.drag = 1f;
            rb.angularDrag = 1f;
            rb.AddExplosionForce(15f, hitPosition, 1.5f, 0f, ForceMode.Impulse);
            characterJoints.Add(bone.AddComponent<CharacterJoint>());

        }

        ConfigureCharacterJoints();

        leftArm.transform.SetParent(bones[10].transform);


        Destroy(this);
    }

    List<CharacterJoint> characterJoints = new List<CharacterJoint>();
    List<Rigidbody> rbs = new List<Rigidbody>();

    private void ConfigureCharacterJoints()
    {
        characterJoints[0].connectedBody = rbs[7];
        characterJoints[1].connectedBody = rbs[0];
        characterJoints[2].connectedBody = rbs[1];
        characterJoints[3].connectedBody = rbs[2];
        characterJoints[4].connectedBody = rbs[0];
        characterJoints[5].connectedBody = rbs[4];
        characterJoints[6].connectedBody = rbs[5];
        characterJoints[7].connectedBody = rbs[0];
        characterJoints[8].connectedBody = rbs[7];
        characterJoints[9].connectedBody = rbs[10];
        characterJoints[10].connectedBody = rbs[8];
        characterJoints[11].connectedBody = rbs[7];
        characterJoints[12].connectedBody = rbs[7];
        characterJoints[13].connectedBody = rbs[12];
        characterJoints[14].connectedBody = rbs[13];
    }

    public void Damage(Vector3 hitPosition)
    {
        this.hitPosition = hitPosition;
        Invoke(nameof(ChangeToDeathState), 0.1f);
    }

    private void ChangeToDeathState()
    {
        state = 7;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((gunLayer.value | (1 << collision.gameObject.layer)) != gunLayer.value) return;

        if (collision.rigidbody.velocity.magnitude < 10f) return;

        state = 7;
        hitPosition = collision.GetContact(0).point;

    }

    private void DisableAI()
    {
        animator.enabled = false;

        if (navMeshAgent.hasPath)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }

        navMeshAgent.enabled = false;
        Destroy(this);
    }
}
