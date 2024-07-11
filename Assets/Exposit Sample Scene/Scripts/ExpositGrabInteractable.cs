using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class ExpositGrabInteractable : MonoBehaviour
{
    private Transform originalParent;
    [Header("Insert Object HandPose Here")]
    public GameObject RightHandPose;
    public GameObject LeftHandPose;
    public GameObject DropPoint;
    public Transform RightAttachTransform;
    public Transform LeftAttachTransform;
    public bool UseDefaultGrab = false;
    private bool isGrabbed = false;
    public bool DropWithSnap = false;
    private Collider DropColider;
    private float TimeDuration = 1f;
    public UnityEvent OnGrab;
    public UnityEvent OnRelease;
    bool isRightHandGrab = false;
    bool isLeftHandGrab = false;
    [SerializeField] ExpositPlayerChanger playerHandChanger;

    private XRGrabInteractable grabInteractable;

    void Start()
    {
        RightHandPose.SetActive(false);
        LeftHandPose.SetActive(false);
        originalParent = DropPoint.transform.parent; // Store the original parent of the object
        DropPoint.SetActive(false);
        DropColider = DropPoint.GetComponent<Collider>();

        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.onSelectEntered.AddListener(OnGrabObject);
        grabInteractable.onSelectExited.AddListener(OnReleaseObject);
    }

    private void OnDestroy()
    {
        grabInteractable.onSelectEntered.RemoveListener(OnGrabObject);
        grabInteractable.onSelectExited.RemoveListener(OnReleaseObject);
    }

    private void OnGrabObject(XRBaseInteractor interactor)
    {
        if (isGrabbed) return;

        isGrabbed = true;
        transform.SetParent(interactor.transform); // Make the object a child of the hand
        GetComponent<Rigidbody>().isKinematic = true; // Make it kinematic so it moves with the hand
        OnGrab.Invoke();
        StartCoroutine(Grab(interactor.transform));
        StartCoroutine(OnDropObjectLate());

        if (interactor is XRDirectInteractor directInteractor)
        {
            if (directInteractor.name.Contains("Right"))
            {
                isRightHandGrab = true;
            }
            else if (directInteractor.name.Contains("Left"))
            {
                isLeftHandGrab = true;
            }
        }
    }

    private void OnReleaseObject(XRBaseInteractor interactor)
    {
        if (!isGrabbed) return;

        StartCoroutine(Release());
        DropPoint.SetActive(false);
        isRightHandGrab = false;
        isLeftHandGrab = false;
        OnRelease.Invoke();
    }

    IEnumerator Grab(Transform hand)
    {
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
                    transform.localPosition = Vector3.Lerp(transform.localPosition, RightAttachTransform.transform.localPosition, elapsedTime / TimeDuration);
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, RightAttachTransform.transform.localRotation, elapsedTime / TimeDuration);
                    RightHandPose.SetActive(true);
                    playerHandChanger.VRRightHandsOff();
                }
                if (isLeftHandGrab)
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, LeftAttachTransform.transform.localPosition, elapsedTime / TimeDuration);
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, LeftAttachTransform.transform.localRotation, elapsedTime / TimeDuration);
                    LeftHandPose.SetActive(true);
                    playerHandChanger.VRLeftHandsOff();
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }

    IEnumerator Release()
    {
        float elapsedTime = 0f;
        transform.SetParent(originalParent); // Restore the original parent
        GetComponent<Rigidbody>().isKinematic = false; // Make it non-kinematic again
        RightHandPose.SetActive(false);
        LeftHandPose.SetActive(false);
        playerHandChanger.VRRightHandsOn();
        playerHandChanger.VRLeftHandsOn();

        if (DropWithSnap)
        {
            transform.localRotation = DropPoint.transform.localRotation;
            transform.localPosition = DropPoint.transform.localPosition;
        }
        else
        {
            while (elapsedTime < TimeDuration)
            {
                transform.position = Vector3.Lerp(transform.position, DropPoint.transform.position, elapsedTime / TimeDuration);
                transform.rotation = Quaternion.Lerp(transform.rotation, DropPoint.transform.rotation, elapsedTime / TimeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        StartCoroutine(OnGrabBoolAgain());
    }

    IEnumerator OnGrabBoolAgain()
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
