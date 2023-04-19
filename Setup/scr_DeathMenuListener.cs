using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_DeathMenuListener : MonoBehaviour
{
    private bool isPlayerDead = false;

    private Action playerDeath;

    private void Awake()
    {
        playerDeath = () => PlayerDeath();
        scr_PlayerHealth.playerDeath += playerDeath;
    }

    private void Update()
    {
        if (isPlayerDead)
        {
            if (Input.anyKeyDown)
            {
                transform.GetChild(0).gameObject.SetActive(true);
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    private void OnDestroy()
    {
        scr_PlayerHealth.playerDeath -= playerDeath;
    }

    private void PlayerDeath()
    {
        isPlayerDead = true;
    }
}
