﻿using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityMovement : MonoBehaviour
{
    protected Entity.EntityReferenceContainer container;

    [Tooltip("Movement speed in m/s")]
    public float moveSpeed;
    [Tooltip("Back-end handling method for movement")]
    public MovementType type;

    [SerializeField][ReadOnly]
    private Vector2 moveDirection;
    private float speedFac;

    private Rigidbody2D rb;


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate()
    {
        HandleMovement();
    }

    /// <summary>
    /// Sets the Entity references to other Entity componenets
    /// </summary>
    /// <param name="_container">Reference Container Class</param>
    public void SetEntityReference(Entity.EntityReferenceContainer _container)
    {
        container = _container;
    }

    protected virtual void HandleMovement()
    {
        if (moveDirection == Vector2.zero)
            return;

        switch(type)
        {
            case MovementType.MovePosition:
                rb.MovePosition(rb.position + (moveDirection * moveSpeed * speedFac * Time.fixedDeltaTime));
                break;
            case MovementType.Velocity:
                rb.velocity = moveDirection * moveSpeed * speedFac;
                break;
            //case MovementType.AddForce:
            default:
                Debug.LogWarning("Uh oh! Movement type "+type.ToString()+" not supported for: " + name);
                break;
        }
    }

    public virtual void SetMoveDirection(Vector2 newDirection)
    {
        moveDirection = newDirection.normalized;
        speedFac = 1;
    }

    public virtual void SetMoveDirection(Vector2 newDirection, float _speedFac)
    {
        moveDirection = newDirection.normalized;
        speedFac = _speedFac;
    }

    public virtual void StopMoving ()
    {
        moveDirection = Vector2.zero;
    }

    public enum MovementType
    {
        MovePosition,
        Velocity,
        AddForce
    }
}