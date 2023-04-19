using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_OpenDoor : MonoBehaviour
{
    [SerializeField] private Transform doorTransform;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask enemyLayer;

    private bool triggered = false;
    private float startPos, endPos;

    private int collisionCounter = 0;


    private void Awake()
    {
        startPos = doorTransform.position.y;
        endPos = startPos + 3.3f;
    }

    // Update is called once per frame
    void Update()
    {
        if (triggered && doorTransform.position.y < endPos)
            doorTransform.position = new(doorTransform.position.x, Mathf.Lerp(doorTransform.position.y, endPos, 3 * Time.deltaTime), doorTransform.position.z);

        if (!triggered && doorTransform.position.y > startPos)
            doorTransform.position = new(doorTransform.position.x, Mathf.Lerp(doorTransform.position.y, startPos, 3 * Time.deltaTime), doorTransform.position.z);


    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!(((playerLayer.value | (1 << collision.gameObject.layer)) == playerLayer.value) || ((enemyLayer.value | (1 << collision.gameObject.layer)) == enemyLayer.value))) return;

        triggered = true;
        collisionCounter++;
    }


    private void OnTriggerExit(Collider collision)
    {
        if (!(((playerLayer.value | (1 << collision.gameObject.layer)) == playerLayer.value) || ((enemyLayer.value | (1 << collision.gameObject.layer)) == enemyLayer.value))) return;

        collisionCounter--;
        if (collisionCounter == 0)
            triggered = false;
    }

}
