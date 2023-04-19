using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_GunRecoil : MonoBehaviour
{

    [SerializeField] private Transform magLocation;

    [SerializeField] private float rotRecoilSpeed = 16f;
    [SerializeField] private float rotReturnSpeed = 26f;

    [SerializeField] private Vector3 RecoilRotation = new(100f, 40f, 70f);
    [SerializeField] private Vector3 RecoilKickBack = new(20f, -20f, -10f);

    private Vector3 rotRecoil;
    private Vector3 finalRotation;

    private void FixedUpdate()
    {
        rotRecoil = Vector3.Lerp(rotRecoil, Vector3.zero, rotReturnSpeed * Time.deltaTime);

        finalRotation = Vector3.Slerp(finalRotation, rotRecoil, rotRecoilSpeed * Time.deltaTime);
       
        magLocation.localRotation = transform.localRotation = Quaternion.Euler(finalRotation);
    }

    public void Fire()
    {
            rotRecoil += new Vector3(-RecoilRotation.x, Random.Range(-RecoilRotation.y, RecoilRotation.y), Random.Range(-RecoilRotation.z, RecoilRotation.z));
            rotRecoil += new Vector3(Random.Range(-RecoilKickBack.x, RecoilKickBack.x), Random.Range(-RecoilKickBack.y, RecoilKickBack.y), RecoilKickBack.z);
    }
}
