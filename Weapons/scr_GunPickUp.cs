using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_GunPickUp : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    private Transform gunPosition;

    private void Awake()
    {
        gunPosition = GameObject.FindWithTag("GunPosition").transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((playerLayer.value | (1 << other.gameObject.layer)) != playerLayer.value) return;

        if (gunPosition.transform.childCount > 0) return;

        transform.parent.GetComponent<BoxCollider>().enabled = false;
        Destroy(transform.parent.GetComponent<Rigidbody>());

        transform.parent.parent = gunPosition;
        transform.parent.localPosition = Vector3.zero;
        transform.parent.localRotation = Quaternion.identity;

    }

    private void OnTriggerStay(Collider other)
    {
        if ((playerLayer.value | (1 << other.gameObject.layer)) != playerLayer.value) return;

        if (gunPosition.transform.childCount > 0) return;

        transform.parent.GetComponent<BoxCollider>().enabled = false;
        Destroy(transform.parent.GetComponent<Rigidbody>());

        transform.parent.parent = gunPosition;
        transform.parent.localPosition = Vector3.zero;
        transform.parent.localRotation = Quaternion.identity;
    }

}
