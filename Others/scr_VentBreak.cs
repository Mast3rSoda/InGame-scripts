using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_VentBreak : MonoBehaviour, IDamage
{
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask gunLayer;
    private CapsuleCollider playerCollider;

    private Vector3 hitPoint;

    private void Awake()
    {
        playerCollider = GameObject.FindWithTag("Player").GetComponent<CapsuleCollider>();
    }

    public void Damage(Vector3 hitPosition)
    {
        this.hitPoint = hitPosition;
        Invoke(nameof(DestroyVent), 0.1f);
    }


    private void DestroyVent()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject childGO = transform.GetChild(i).gameObject;

            Rigidbody rb = childGO.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
            rb.angularDrag = 0f;
            rb.AddExplosionForce(10f, new(hitPoint.x, hitPoint.y, hitPoint.z - 0.3f), 1.5f, 0f, ForceMode.Impulse);
            rb.AddTorque(new Vector3(Random.Range(1f, 5f), Random.Range(1f, 5f), Random.Range(1f, 5f)), ForceMode.Impulse);
            Physics.IgnoreCollision(childGO.GetComponent<BoxCollider>(), playerCollider, true);
        }
            transform.GetComponent<BoxCollider>().enabled = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!(((playerLayer.value | (1 << collision.gameObject.layer)) == playerLayer.value) || ((gunLayer.value | (1 << collision.gameObject.layer)) == gunLayer.value))) return;

        if (collision.attachedRigidbody.velocity.magnitude < 10f) return;

        hitPoint = collision.transform.position;

        DestroyVent();

    }

}
