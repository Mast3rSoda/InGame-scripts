using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_BallThingyFloat : MonoBehaviour
{
    private float startPos, endPos;

    private bool goUp = true;

    private void Awake()
    {
        startPos = transform.position.y;
        endPos = transform.position.y + 2;
        transform.position += new Vector3(0f, 0.01f, 0f);
    }

    private void Update()
    {
        if (goUp)
            transform.position = new(transform.position.x, Mathf.Lerp(transform.position.y, endPos, (transform.position.y - startPos) * 0.005f), transform.position.z);

        if (!goUp)
            transform.position = new(transform.position.x, Mathf.Lerp(transform.position.y, startPos, (endPos - transform.position.y) * 0.005f), transform.position.z);

        if ((transform.position.y - endPos > -0.01f && goUp) || (transform.position.y - startPos < 0.01f && !goUp))
            goUp = !goUp;
    }
}
