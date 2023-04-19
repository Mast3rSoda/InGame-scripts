using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_PlayerGrapple : MonoBehaviour
{

    //Components
    [Header("Components")]
    private LineRenderer lineRenderer;

    //Assignable
    [Header("Assignable")]
    public Transform playerCam;
    public LayerMask ground;
    public scr_GrappleUILoad grappleUI;

    //Grapple Variables
    [Header("Grapple variables")]
    private float maxGrappleDistance = 60f;
    private int grappleCharges = 2;
    private float grappleRechargeTime = 6f;
    private Vector3 grapplePoint;
    private Vector3 currentGrapplePosition;
    private SpringJoint joint;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    void LateUpdate()
    {
        DrawRope();
    }

    public void ProcessGrapple()
    {
        if (grappleCharges == 0) return;

        if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit raycastHit, maxGrappleDistance, ground))
        {
            grappleCharges--;
            grapplePoint = raycastHit.point;
            joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

            //good values: spr - 30 - d / 4.5
            //good values: dam - 5 + d / 15
            //good values: mass - 2 - d / 60

            joint.spring = 26f - distanceFromPoint / 10f;
            joint.damper = 4f + distanceFromPoint / 8f;
            joint.massScale = 1.4f;
            joint.enableCollision = true;

            currentGrapplePosition = transform.position;
            lineRenderer.positionCount = 2;
        }
    }

    private void RechargeGrapple()
    {
        if (grappleCharges == 0)
        {
            Invoke(nameof(RechargeGrapple), grappleRechargeTime);
        }
        grappleCharges += 1;
        if(!isGrapling())
            grappleUI.manageGrappleUICharging(grappleCharges);
    }

    //on grapple -> send info that we want to change the ui ->
    //if grapple charges are 1 (we can send that as parameter) ->
    //start charging the right wing
    //if grapple charges are 0 deload (or just set the width to 0?) the right wing
    //remember the width
    //start lerping from remembered width on left wing to recharge time
    //should work - didn't work, but it's working now!



    public void StopGrapple()
    {
        if (!joint) return;

        if (grappleCharges == 1 && !IsInvoking(nameof(RechargeGrapple)))
            Invoke(nameof(RechargeGrapple), grappleRechargeTime);

        grappleUI.manageGrappleUICharging(grappleCharges);
        Destroy(joint);
        lineRenderer.positionCount = 0;
    }

    private bool isGrapling()
    {
        return joint;
    }

    private void DrawRope()
    {
        //If not grappling, don't draw rope
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, currentGrapplePosition);
    }
}
