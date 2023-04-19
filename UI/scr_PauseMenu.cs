using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class scr_PauseMenu : MonoBehaviour
{

    public static Action canShootAction;

    [SerializeField] private GameObject playerCam;    
    public void ResumeGame()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1.0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerCam.SetActive(true);
        canShootAction?.Invoke();
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
