using UnityEngine;
using UnityEngine.UI;

public class scr_GrappleUILoad : MonoBehaviour
{

    [SerializeField]
    private Image leftWing, rightWing;

    private Image currentImage = null;

    private float currentScaleValue = 1f, grappleRechargeTime = 6f;

    private void Update()
    {
        RechargeGrapple();
    }

    public void manageGrappleUICharging(float grappleCharges)
    {
        if (grappleCharges == 2) return;
        if (grappleCharges == 1)
        {
            if (currentImage == leftWing)
            {
                currentImage.transform.localScale = new Vector3(1.0f, currentImage.transform.localScale.y, currentImage.transform.localScale.z);
                currentImage.color = Color.white;
            }

            currentScaleValue = 0f;
            currentImage = rightWing;
        }
        if (grappleCharges == 0)
        {
            currentImage.transform.localScale = new Vector3(0.0f, currentImage.transform.localScale.y, currentImage.transform.localScale.z);
            currentImage = leftWing;
        }
        currentImage.color = new Color32(170, 170, 170, 255);
        currentImage.transform.localScale = new Vector3(currentScaleValue, currentImage.transform.localScale.y, currentImage.transform.localScale.z);

    }

    private void RechargeGrapple()
    {
        if (currentScaleValue == 1) return;

        currentScaleValue += 1f / grappleRechargeTime * Time.deltaTime;
        if (currentScaleValue >= 1f)
        {
            currentImage.color = Color.white;
            currentScaleValue = 1f;
        }
        currentImage.transform.localScale = new Vector3(currentScaleValue, currentImage.transform.localScale.y, currentImage.transform.localScale.z);

    }

}
