using NaughtyAttributes;
using UnityEngine;

public class EntityAiming : MonoBehaviour
{
    #region Properties
    protected Entity.EntityReferenceContainer container;

    [Tooltip("Transform to rotate")]
    public Transform rotator; // Temp, should probably be reworked later
    [Tooltip("Transform that is used to instantiate bullets")]
    public Transform bulletPointer;
    [Tooltip("Direction object is initially looking")]
    public Direction direction;
    public bool flipSprite;

    [HorizontalLine]
    [SerializeField]
    protected bool useLookTarget;
    [SerializeField]
    protected bool useLookLocation;
    [SerializeField]
    protected Transform lookTarget;
    [SerializeField]
    protected Vector2 lookLocation;
    [SerializeField]
    protected bool spriteFlipped = false;
    #endregion

    #region Initialization
    protected virtual void Awake ()
    {
        if (rotator == null)
        {
            Debug.LogWarning("No rotator set for: " + gameObject.name + ", using attached GameObject instead");
            rotator = gameObject.transform;
        }
    }

    /// <summary>
    /// Sets the Entity references to other Entity componenets
    /// </summary>
    /// <param name="_container">Reference Container Class</param>
    public void SetEntityReference(Entity.EntityReferenceContainer _container)
    {
        container = _container;
    }
    #endregion

    #region UpdateMethods
    protected virtual void Update ()
    {
        if (useLookTarget && lookTarget)
        {
            LookAt(lookTarget.position);
        }
        else if (useLookLocation)
        {
            LookAt(lookLocation);
        }
    }

    /// <summary>
    /// Rotates towards the supplied Vector2
    /// </summary>
    /// <param name="_target">Target to look at</param>
    public virtual void LookAt(Vector2 _target)
    {
        LookInDirection(_target - (Vector2)rotator.position);
    }
    #endregion

    #region Input
    /// <summary>
    /// Rotates towards the supplied direction
    /// <para>positive Y represents 0, and increments clockwise</para>
    /// </summary>
    /// <param name="_direction">Durrection to look in</param>
    public virtual void LookInDirection(Vector2 _direction)
    {
        DisableAutolook();

        float angle = Vector2.SignedAngle(Vector2.up, _direction);
        if (flipSprite)
        {
            if (angle<=0 && spriteFlipped)
                FlipSprite(false);
            else if (angle>0 && !spriteFlipped)
                FlipSprite(true);
        }

        Quaternion rot = Quaternion.Euler(new Vector3(0, 0, angle));
        rotator.rotation = OffsetDirection(rot);
    }

    /// <summary>
    /// Sets a Transform for the script to rotate torwards
    /// </summary>
    public virtual void SetLookTarget(Transform _target)
    {
        lookTarget = _target;
        useLookTarget = true;

        useLookLocation = false;
    }

    /// <summary>
    /// Sets a Vector3 location for the script to rotate torwards
    /// </summary>
    public virtual void SetLookLocation(Vector3 _location)
    {
        lookLocation = _location;
        useLookLocation = true;

        useLookTarget = false;
    }

    /// <summary>
    /// Prevents the script from automatically looking at a target
    /// </summary>
    public virtual void DisableAutolook()
    {
        useLookTarget = false;
        useLookLocation = false;
    }
    #endregion

    #region PrivateMethods
    /// <summary>
    /// Inverts the scale along the y axis
    /// </summary>
    /// <param name="flip">True to flip, false for default orientation</param>
    protected virtual void FlipSprite(bool flip)
    {
        rotator.transform.localScale = new Vector3(rotator.transform.localScale.x, (flip ? -1 : +1) * Mathf.Abs(rotator.transform.localScale.y), rotator.transform.localScale.z);
        spriteFlipped = flip;

        //Debug.Log("Sprite Flipped!\n" + gameObject.name);
        //Call other events here if needed
    }

    /// <summary>
    /// Rotates the provided Quaternion based on the sprites default orientation
    /// </summary>
    /// <param name="_rotation">Supplied rotation to be offset</param>
    protected virtual Quaternion OffsetDirection(Quaternion _rotation)
    {
        Quaternion offset;

        switch (direction)
        {
            case Direction.Left:
                offset = Quaternion.Euler(0, 0, -90);
                break;
            case Direction.Right:
                offset = Quaternion.Euler(0, 0, 90);
                break;
            case Direction.Down:
                offset = Quaternion.Euler(0, 0, 180);
                break;
            default:
                offset = Quaternion.identity;
                break;
        }

        return _rotation * offset;
    }
    #endregion
}

#region Enums
public enum Direction
{
    Right,
    Left,
    Up,
    Down
}
#endregion