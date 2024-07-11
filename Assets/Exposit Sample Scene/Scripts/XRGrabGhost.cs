using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class XRGrabGhost : MonoBehaviour
{
    public GameObject DropPoint;
    public bool ResetPositionOnDrop = false;
    private float lerpDuration = 1f;
    Collider grabObjectcolider;
    public UnityEvent OnGrab;
    public UnityEvent OnRelese;
   
    public void ActiveGhost()
    {
        StartCoroutine(OnGhostOnDelay());
        OnGrab.Invoke();
    }
    public void DeactiveGhost()
    {
        StartCoroutine(OffGhostOnDelay());
        DropPoint.SetActive(false);
        if (ResetPositionOnDrop)
        {
            StartCoroutine(Release());
        }

    }
    public void Start()
    {
        grabObjectcolider = DropPoint.GetComponent<Collider>();
    }
    public void OnTriggerEnter(Collider other)
    {
        if(other == grabObjectcolider)
        {
            StartCoroutine(Release());
            Debug.Log("Droppped");
        }
    }
    IEnumerator OnGhostOnDelay()
    {
        yield return new WaitForSeconds(2);
        DropPoint.SetActive(true);
    }
    IEnumerator OffGhostOnDelay()
    {
        yield return new WaitForSeconds(1.8f);
        DropPoint.SetActive(false);
    }
    IEnumerator Release()
    {
        float elapsedTime = 0f;
        GetComponent<Rigidbody>().isKinematic = true; // Make it non-kinematic again
        OnRelese.Invoke();

        while (elapsedTime < lerpDuration)
        {
            transform.position = Vector3.Lerp(gameObject.transform.position, DropPoint.transform.position, elapsedTime / lerpDuration);
            transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, DropPoint.transform.rotation, elapsedTime / lerpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}

