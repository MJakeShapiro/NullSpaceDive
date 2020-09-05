using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[System.Serializable]
public class EntityMovement : MonoBehaviour
{
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

    protected virtual void HandleMovement()
    {
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

    public enum MovementType
    {
        MovePosition,
        Velocity,
        AddForce
    }
}