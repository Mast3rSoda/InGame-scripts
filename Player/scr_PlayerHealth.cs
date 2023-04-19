using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Cinemachine.CinemachineCore;

public class scr_PlayerHealth : MonoBehaviour, IDamage
{
    private float health = 3f;
    private CinemachineImpulseSource impulseSource;

    [SerializeField] private LayerMask killSwitchLayer;

    private Vector3 hitPosition;

    public static System.Action playerDeath;

    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Damage(Vector3 hitPosition)
    {
        this.hitPosition = hitPosition;
        Invoke(nameof(TakeDamage), 0.1f);
    }

    private void TakeDamage()
    {
        health--;
        impulseSource.GenerateImpulse(1.5f);
        if (health > 0) return;

        playerDeath?.Invoke();

        Transform headTransform = transform.GetChild(0);
        headTransform.SetParent(null);
        headTransform.GetComponent<BoxCollider>().enabled = true;
        Rigidbody rb = headTransform.AddComponent<Rigidbody>();
        rb.mass = 3f;
        rb.drag = 1f;
        rb.angularDrag = 1f;

        GameObject.Find("PlayerCam").GetComponent<CinemachineInputProvider>().XYAxis = null;

        Transform gunPosition = GameObject.Find("Main Camera").transform.Find("GunPosition");

        if (gunPosition.childCount == 0)
        {
            Destroy(gameObject);
            return;
        }

        Transform gunTransform = gunPosition.GetChild(0);

        Rigidbody gunRB = gunTransform.AddComponent<Rigidbody>();
        gunRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
        gunRB.angularDrag = 0f;
        gunRB.mass = 2f;
        gunTransform.GetComponent<BoxCollider>().enabled = true;
        float gunX = gunTransform.position.x,
              gunY = gunTransform.position.y,
              gunZ = gunTransform.position.z;
        gunRB.AddExplosionForce(15f, new(Random.Range(gunX - 0.4f, gunX + 0.4f),
                                        Random.Range(gunY - 0.1f, gunY - 0.4f),
                                        Random.Range(gunZ - 0.4f, gunZ - 0.4f)), 3f, 0f, ForceMode.Impulse);

        gunRB.AddTorque(new Vector3(Random.Range(1f, 5f), Random.Range(1f, 5f), Random.Range(1f, 5f)), ForceMode.Impulse);

        rb.AddExplosionForce(40f, hitPosition, 3f, 0f, ForceMode.Impulse);


        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((killSwitchLayer.value | (1 << collision.gameObject.layer)) != killSwitchLayer.value) return;

        health = 1;
        TakeDamage();
    }



}
