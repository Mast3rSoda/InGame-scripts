using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class scr_GunShoot : MonoBehaviour
{
    [SerializeField] private GunData gunData;

    private Animator gunAnimator;

    private Transform playerCam;

    private bool canShoot = true;

    private Action shootAction;
    private Action stopShootAction;
    private Action canShootAction;

    private void Awake()
    {
        gunData = Instantiate(gunData);
        gunAnimator = GetComponentInChildren<Animator>();
        playerCam = GameObject.FindWithTag("MainCamera").transform;
        shootAction = () => Shoot();
        scr_PlayerInput.shootAction += shootAction;
        stopShootAction = () => StopShootAfterPause();
        scr_PlayerInput.stopShootAction += stopShootAction;
        canShootAction = () => Invoke(nameof(ResetShot), 0.1f);
        scr_PauseMenu.canShootAction += canShootAction;
    }

    private void Shoot()
    {
        if (!transform.parent) return;

        if (!transform.parent.CompareTag("GunPosition")) return;

        if (canShoot && gunData.currentAmmo > 0)
        {
            gunData.currentAmmo--;
            canShoot = false;
            Invoke(nameof(ResetShot), 1 / gunData.fireRate / 60f);

            if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit raycastHit, gunData.maxDistance))
            {
                if (raycastHit.transform.TryGetComponent(out IDamage damage))
                    damage.Damage(raycastHit.point);
            }


            if (gunData.currentAmmo == 0)
            {
                gunAnimator.SetTrigger("LastShot");
                return;
            }
            gunAnimator.SetTrigger("Fire");
        }

    }

    private void OnDestroy()
    {
        scr_PlayerInput.shootAction -= shootAction;
        scr_PlayerInput.stopShootAction -= stopShootAction;
        scr_PauseMenu.canShootAction -= canShootAction;
    }

    private void StopShootAfterPause()
    {
        canShoot = false;
    }

    private void ResetShot()
    {
        canShoot = true;
    }

}
