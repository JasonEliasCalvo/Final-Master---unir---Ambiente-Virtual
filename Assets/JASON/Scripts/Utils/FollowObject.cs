using System;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    [SerializeField] private GameObject followObject;
    [SerializeField] private FollowType followType;

    [SerializeField] private float smoothSpeed = 5f;

    void Update()
    {
        if (followObject == null) return;

        if ((followType & FollowType.Position) == FollowType.Position)
            FollowPosition();

        if ((followType & FollowType.Rotation) == FollowType.Rotation)
            FollowRotation();

        if ((followType & FollowType.Scale) == FollowType.Scale)
            FollowScale();
    }

    private void FollowPosition()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            followObject.transform.position,
            Time.deltaTime * smoothSpeed
        );
    }

    private void FollowRotation()
    {
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            followObject.transform.rotation,
            Time.deltaTime * smoothSpeed
        );
    }

    private void FollowScale()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            followObject.transform.localScale,
            Time.deltaTime * smoothSpeed
        );
    }
}

