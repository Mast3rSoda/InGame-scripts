using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class scr_WinCondition : MonoBehaviour
{
    [SerializeField] private GameObject winMenu;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GameObject playerCam;

    private void OnTriggerEnter(Collider other)
    {
        if ((playerLayer.value | (1 << other.gameObject.layer)) != playerLayer.value) return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        winMenu.SetActive(true);
        playerCam.SetActive(false);
    }

}
