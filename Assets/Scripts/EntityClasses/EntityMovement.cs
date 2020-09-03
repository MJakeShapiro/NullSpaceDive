using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityMovement : MonoBehaviour
{
    [Tooltip("Movement speed in m/s")]
    public float moveSpeed;
    [Tooltip("Movement using Velocity or MovePosition")]
    public bool useVelocity;

    [SerializeField][ReadOnly]
    private Vector2 moveDirection;

    private Rigidbody2D rb;


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate()
    {
        HandleMovement();
    }

    protected virtual void HandleMovement()
    {
        if (useVelocity)
            rb.velocity = moveDirection * moveSpeed;
        else
            rb.MovePosition(rb.position + (moveDirection * moveSpeed * Time.fixedDeltaTime));
    }

    public virtual void SetMoveDirection(Vector2 newDirection)
    {
        moveDirection = newDirection.normalized;
    }
}