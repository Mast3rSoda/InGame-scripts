using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public Transform upperObject;



    private void Update()
    {
        Quaternion targetAngle = Quaternion.LookRotation(upperObject.position - transform.position);
        Debug.Log(targetAngle.eulerAngles);
    }

}
