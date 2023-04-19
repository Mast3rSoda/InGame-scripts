using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;

// gra powstala na Wydziale Informatyki Politechniki Bialostockiej

public class scr_PlayerInput : MonoBehaviour
{
    //References
    private InputManager inputManager;
    private InputManager.PlayerActions player;
    private scr_PlayerMovement playerMovement;
    private scr_PlayerGrapple playerGrapple;

    //Actions
    public static Action shootAction;
    public static Action throwAction;
    public static Action stopShootAction;

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject playerCam;

    private void Awake()
    {
        inputManager = new InputManager();
        player = inputManager.Player;
        playerMovement = GetComponent<scr_PlayerMovement>();
        playerGrapple = GetComponent<scr_PlayerGrapple>();
        player.Jump.performed += ctx => playerMovement.ProcessJump();
        player.Crouch.started += ctx => playerMovement.StartCrouch();
        player.Crouch.canceled += ctx => playerMovement.StopCrouch();
        player.Grapple.performed += ctx => playerGrapple.ProcessGrapple();
        player.Grapple.canceled += ctx => playerGrapple.StopGrapple();
        player.Fire.started += ctx => shootAction?.Invoke();
        player.Throw.started += ctx => throwAction?.Invoke();

        player.Pause.performed += ctx =>
        {
            if (pauseMenu.activeInHierarchy)
            {
                pauseMenu.SetActive(false);
                Time.timeScale = 1.0f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                playerCam.SetActive(true);
                return;
            }

            pauseMenu.SetActive(true);
            playerCam.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            stopShootAction?.Invoke();

        };
    }

    private void OnEnable()
    {
        player.Enable();
    }
    private void OnDisable()
    {
        player.Disable();
    }

    private void FixedUpdate()
    {
        playerMovement.ProcessMove(player.Move.ReadValue<Vector2>());

    }

}
