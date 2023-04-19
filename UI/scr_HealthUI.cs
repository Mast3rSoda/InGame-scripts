using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scr_HealthUI : MonoBehaviour
{
    [SerializeField] private Image[] healthImages;

    private float damageTaken = 0f;
    private float healthBarsRemoved = 0f;
    private float currentScaleValue = 1f;

    private Image currentImage = null;

    private Action hitAction;
    private void Awake()
    {
        hitAction = () => TakeDamage();
        scr_EnemyAI.hitAction += hitAction;
    }

    private void OnDestroy()
    {
        scr_EnemyAI.hitAction -= hitAction;

    }

    private void Update()
    {
        if (damageTaken == healthBarsRemoved || currentImage == null) return;

        currentScaleValue -= (damageTaken - healthBarsRemoved) * currentScaleValue * Time.deltaTime * 8f;

        if (currentScaleValue < 0.03f)
            currentScaleValue = 0f;

        currentImage.transform.localScale = new(currentScaleValue, currentImage.transform.localScale.y, currentImage.transform.localScale.z);

        if (currentScaleValue == 0)
        {
            healthBarsRemoved++;
            currentScaleValue = 1f;

            if (healthBarsRemoved == healthImages.Length)
            {
                enabled = false;
                return;
            }

            currentImage = healthImages[^(int)(healthBarsRemoved + 1)];
        }
    }

    private void TakeDamage()
    {
        damageTaken++;

        if (!currentImage)
            currentImage = healthImages[^1];

    }
}
