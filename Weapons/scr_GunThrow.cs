using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_GunThrow : MonoBehaviour
{
    [SerializeField] private float throwPower = 60f;

    private Transform throwPosition;
    private CapsuleCollider playerCollider;

    private CinemachineImpulseSource impulseSource;

    private System.Action throwAction;

    private void Awake()
    {
        throwAction = () => Throw();
        scr_PlayerInput.throwAction += throwAction;
        impulseSource = GetComponent<CinemachineImpulseSource>();
        throwPosition = GameObject.FindWithTag("ThrowPosition").transform;
        playerCollider = GameObject.FindWithTag("Player").GetComponent<CapsuleCollider>();
        Physics.IgnoreCollision(gameObject.GetComponent<BoxCollider>(), playerCollider, true);
    }

    private void Throw()
    {
        if (!transform.parent) return;

        if (!transform.parent.CompareTag("GunPosition")) return;

        impulseSource.GenerateImpulse(1.2f);
        
        transform.SetParent(null);
        transform.position = throwPosition.position;

        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.angularDrag = 0f;
        rb.mass= 2f;
        
        rb.AddForce(throwPower * 1.2f * transform.forward, ForceMode.Impulse);
        rb.AddTorque(new Vector3(Random.Range(10f, 20f), Random.Range(4f, 10f), Random.Range(5f, 15f)), ForceMode.Impulse);

        Physics.IgnoreCollision(transform.Find("GunTrigger").GetComponent<BoxCollider>(), playerCollider, true);
        
        gameObject.GetComponent<BoxCollider>().enabled = true;
        Invoke(nameof(EnablePlayerCollision), 1f);
    }

    private void OnDestroy()
    {
        scr_PlayerInput.throwAction -= throwAction;

    }

    private void EnablePlayerCollision()
    {
        if (gameObject.GetComponent<BoxCollider>().enabled && playerCollider != null)
        {
            Physics.IgnoreCollision(transform.Find("GunTrigger").GetComponent<BoxCollider>(), playerCollider, false);

        }

    }

}
