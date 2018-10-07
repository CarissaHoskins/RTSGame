using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverBehavior : MonoBehaviour
{

    public float moveSpeed = 3f;
    public float rotatingSpeed = 100f;

    private bool isWandering = false;
    private bool isRotatingLeft = false;
    private bool isRotatingRight = false;
    private bool isWalking = false;

	void Start ()
    {
		
	}
	
	void Update ()
    {
        if (!isWandering)
        {
            StartCoroutine("Wander");
        }
        if (isRotatingRight)
        {
            transform.Rotate(transform.up * Time.deltaTime * rotatingSpeed); //transform.right
        }
        if (isRotatingLeft)
        {
            transform.Rotate(transform.up * Time.deltaTime * -rotatingSpeed); //transform.right
        }
        if (isWalking)
        {
            transform.position += (transform.forward * moveSpeed * Time.deltaTime);
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Block")
        {
            isWalking = false;
        }
    }

    IEnumerator Wander()
    {
        int RotateTime = Random.Range(1, 3);
        int RotateWait = Random.Range(1, 3);
        int RotateLorR = Random.Range(1, 3);
        int WalkWait = Random.Range(1, 3);
        int WalkTime = Random.Range(1, 3);

        isWandering = true;

        yield return new WaitForSecondsRealtime(WalkWait);
        isWalking = true;
        yield return new WaitForSecondsRealtime(WalkTime);
        isWalking = false;
        yield return new WaitForSecondsRealtime(RotateWait);
        if (RotateLorR == 1)
        {
            isRotatingRight = true;
            yield return new WaitForSecondsRealtime(RotateTime);
            isRotatingRight = false;
        }
        if (RotateLorR == 2)
        {
            isRotatingLeft = true;
            yield return new WaitForSecondsRealtime(RotateTime);
            isRotatingLeft = false;
        }
        isWandering = false;
    }
}
