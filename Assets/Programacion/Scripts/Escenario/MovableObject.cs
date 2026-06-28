using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Flags enums
[Flags]
public enum FollowType
{
    None = 0,
    Position = 1 << 0,
    Rotation = 1 << 1,
    Scale = 1 << 2
}

[Flags]
public enum PositionType
{
    None = 0,
    X = 1 << 0,
    Y = 1 << 1,
    Z = 1 << 2
}

[Flags]
public enum RotationType
{
    None = 0,
    X = 1 << 0,
    Y = 1 << 1,
    Z = 1 << 2
}
#endregion; 
public class MovableObject : MonoBehaviour
{
    [SerializeField] protected Transform[] movePoints;
    [SerializeField] protected float moveSpeed;

    protected Transform currentTarget;
    protected int nextPlatform = 1;
    protected bool platformOrder = true;
    protected bool canMove = false;

    private void Update()
    {
        if (canMove)
        {
            Move();
        }
    }

    protected virtual void Move()
    {

    }

    public void StartMoving() => canMove = true;
    public void StopMoving() => canMove = false;
}
