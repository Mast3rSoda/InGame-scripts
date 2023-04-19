using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_PistolAni : MonoBehaviour
{
    [Header("Prefab Refrences")]
    public GameObject bulletPrefab;
    public GameObject casingPrefab;
    public GameObject muzzleFlashPrefab;

    [Header("Location Refrences")]
    [SerializeField] private Transform barrelLocation;
    [SerializeField] private Transform casingExitLocation;

    [Header("Settings")]
    [SerializeField] private float destroyTimer = 1f;
    [SerializeField] private float ejectPower = 500f;

    private CinemachineImpulseSource impulseSource;
    private scr_GunRecoil gunRecoil;
    
    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
        gunRecoil = GetComponent<scr_GunRecoil>();
    }

    void Start()
    {
        if (barrelLocation == null)
            barrelLocation = transform;

    }

    public void Shoot()
    {
        gunRecoil.Fire();

        if (transform.parent.parent.CompareTag("GunPosition")) 
            impulseSource.GenerateImpulse(1.7f);
        if (!muzzleFlashPrefab) return;
        
        GameObject tempFlash;

        tempFlash = Instantiate(muzzleFlashPrefab, barrelLocation.position, barrelLocation.rotation);

        tempFlash.transform.SetParent(barrelLocation.transform);

        Destroy(tempFlash, 0.2f);

    }

    public void CasingRelease()
    {
        if (!casingExitLocation || !casingPrefab) return;

        GameObject tempCasing;
        tempCasing = Instantiate(casingPrefab, casingExitLocation.position, casingExitLocation.rotation) as GameObject;

        tempCasing.GetComponent<Rigidbody>().AddExplosionForce(Random.Range(ejectPower * 0.7f, ejectPower), (casingExitLocation.position - casingExitLocation.right * 0.3f - casingExitLocation.up * 0.6f), 1f);

        tempCasing.GetComponent<Rigidbody>().AddTorque(new Vector3(0, Random.Range(100f, 500f), Random.Range(100f, 1000f)), ForceMode.Impulse);

        Destroy(tempCasing, destroyTimer);
    }
}
