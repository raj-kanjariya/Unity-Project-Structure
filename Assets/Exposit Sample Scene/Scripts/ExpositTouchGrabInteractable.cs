using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
[HelpURL("https://docs.google.com/document/d/1rlWtFSpSTjG-AZoaL1kzDTmgNX4Bxgf-glSFpHtpcqg/edit?usp=sharing")]
public class ExpositTouchGrabInteractable : MonoBehaviour
{
    private Transform originalParent;
    [Header("Insert XR Colider")]
    public Collider RighthandColider;
    public Collider LefthandColider;
    public Transform RightAttachTransform;
    public Transform LeftAttachTransform;
    public bool UseDefaultGrab = false;
    [Header("Insert Object HandPose Here")]
    public GameObject RightHandPose;
    public GameObject LeftHandPose;
    public GameObject DropPoint;
    private bool isGrabbed = false;
    public bool DropWithSnap = false;
    private Collider DropColider;
    private float TimeDuration = 1f;
    public UnityEvent OnGrab;
    public UnityEvent OnRelese;
    bool isRightHandGrab = false;
    bool isLeftHandGrab = false;
    [SerializeField] ExpositPlayerChanger playerHandChanger;
   
    void Start()
    {
       
        RightHandPose.SetActive(false);
        LeftHandPose.SetActive(false);
        originalParent = DropPoint.transform.parent; // Store the original parent of the object
        DropPoint.SetActive(false);
        DropColider = DropPoint.GetComponent<Collider>();
    }
    void OnTriggerEnter(Collider other)
    {
        // Check if the other object is tagged as "Hand"
        if (other == RighthandColider)
        {
            if (!isGrabbed)
            {
                StartCoroutine(Grab(other.transform));
                StartCoroutine(OnDropObjectLate());
                isRightHandGrab = true;
            }
        }
        if (other == LefthandColider)
        {
            if (!isGrabbed)
            {
                StartCoroutine(Grab(other.transform));
                StartCoroutine(OnDropObjectLate());
                isLeftHandGrab = true;
            }
        }
        if (other == DropColider)
        {
            if (isGrabbed)
            {
                StartCoroutine(Release());
                DropPoint.SetActive(false);
                Debug.Log("function called relese");
            }
        }
    }
    IEnumerator Grab(Transform hand)
    {
        isGrabbed = true;
        transform.SetParent(hand); // Make the object a child of the hand
        GetComponent<Rigidbody>().isKinematic = true; // Make it kinematic so it moves with the hand
        OnGrab.Invoke();
        float elapsedTime = 0f;
       
        if (UseDefaultGrab)
        {
            transform.localPosition = Vector3.zero; // Optionally, reset the position relative to the hand
            transform.localRotation = Quaternion.identity; // Optionally, reset the rotation relative to the hand
          
        }
        else
        {
            while (elapsedTime < TimeDuration)
            {
                if (isRightHandGrab)
                {
                    transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, RightAttachTransform.transform.localPosition, elapsedTime / TimeDuration);
                    transform.localRotation = Quaternion.Lerp(gameObject.transform.localRotation, RightAttachTransform.transform.localRotation, elapsedTime / TimeDuration);
                    RightHandPose.SetActive(true);
                    playerHandChanger.VRRightHandsOff();
                }
                if (isLeftHandGrab)
                {
                   
                    transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, LeftAttachTransform.transform.localPosition, elapsedTime / TimeDuration);
                    transform.localRotation = Quaternion.Lerp(gameObject.transform.localRotation, LeftAttachTransform.transform.localRotation, elapsedTime / TimeDuration);
                    playerHandChanger.VRLeftHandsOff();
                    LeftHandPose.SetActive(true);
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
    IEnumerator Release()
    {
        float elapsedTime = 0f;
        StartCoroutine(OnGrabBoolAgine());
        transform.SetParent(originalParent); // Restore the original parent
        GetComponent<Rigidbody>().isKinematic = true; // Make it non-kinematic again
        RightHandPose.SetActive(false);
        LeftHandPose.SetActive(false);
        playerHandChanger.VRRightHandsOn();
        playerHandChanger.VRLeftHandsOn();

        if (DropWithSnap)
        {
            gameObject.transform.localRotation = DropPoint.transform.localRotation;
            gameObject.transform.localPosition = DropPoint.transform.localPosition;
           
        }
        else
        {
            while (elapsedTime < TimeDuration)
            {
                transform.position = Vector3.Lerp(gameObject.transform.position, DropPoint.transform.position, elapsedTime / TimeDuration);
                transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, DropPoint.transform.rotation, elapsedTime / TimeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
                isRightHandGrab = false;
                isLeftHandGrab = false;
            }
        }
        OnRelese.Invoke();
    }
    IEnumerator OnGrabBoolAgine()
    {
        yield return new WaitForSeconds(3);
        isGrabbed = false;
    }
    IEnumerator OnDropObjectLate()
    {
        yield return new WaitForSeconds(3);
        DropPoint.SetActive(true);
    }
}