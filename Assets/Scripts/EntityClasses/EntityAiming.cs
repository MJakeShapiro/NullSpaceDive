using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class EntityAiming : MonoBehaviour
{
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

    protected virtual void Awake ()
    {
        if (rotator == null)
        {
            Debug.LogWarning("No rotator set for: " + gameObject.name + ", using attached GameObject instead");
            rotator = gameObject.transform;
        }
    }

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
    /// Sets the Entity references to other Entity componenets
    /// </summary>
    /// <param name="_container">Reference Container Class</param>
    public void SetEntityReference(Entity.EntityReferenceContainer _container)
    {
        container = _container;
    }

    /// <summary>
    /// Rotates towards the supplied Vector2
    /// </summary>
    /// <param name="_target">Target to look at</param>
    public virtual void LookAt(Vector2 _target)
    {
        LookInDirection(_target - (Vector2)rotator.position);
    }

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
    /// Inverts the scale along the y axis
    /// </summary>
    /// <param name="flip">True to flip, false for default orientation</param>
    public virtual void FlipSprite(bool flip)
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

    /// <summary>
    /// Sets a Transform for the script to rotate torwards
    /// </summary>
    public virtual void SetLookTarget (Transform _target)
    {
        lookTarget = _target;
        useLookTarget = true;

        useLookLocation = false;
    }

    /// <summary>
    /// Sets a Vector3 location for the script to rotate torwards
    /// </summary>
    public virtual void SetLookLocation (Vector3 _location)
    {
        lookLocation = _location;
        useLookLocation = true;

        useLookTarget = false;
    }

    /// <summary>
    /// Prevents the script from automatically looking at a target
    /// </summary>
    public virtual void DisableAutolook ()
    {
        useLookTarget = false;
        useLookLocation = false;
    }
}

public enum Direction
{
    Right,
    Left,
    Up,
    Down
}










// From Staiks old project

/*
public class WeaponAim : MonoBehaviour
{

    public event EventHandler<OnShootEventArgs> OnShoot;
    public class OnShootEventArgs : EventArgs
    {
        public Vector3 gunEndPointPosition;
        public Vector3 shootPosition;
        public Vector3 shellPosition;
    }

    //private PlayerLookAt playerLookAt;
    private Transform aimTransform;
    private Transform aimGunEndPointTransform;
    private Transform aimShellPositionTransform;
    private Animator aimAnimator;

    private void Awake()
    {
        //PlayerLookAt = GetComponent<PlayerLookAt>();
        aimTransform = transform.Find("Aim");
        aimAnimator = aimTransform?.GetComponent<Animator>();
        aimGunEndPointTransform = aimTransform?.Find("GunEndPointPosition");
        aimShellPositionTransform = aimTransform?.Find("ShellPosition");
    }

    private void Update()
    {
        HandleAiming();
        HandleShooting();
    }

    private void HandleAiming()
    {
        Vector3 mousePosition = UtilsClass.GetMouseWorldPosition();

        Vector3 aimDirection = (mousePosition - aimTransform.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        aimTransform.eulerAngles = new Vector3(0, 0, angle);

        Vector3 aimLocalScale = Vector3.one;
        if (angle > 90 || angle < -90)
        {
            aimLocalScale.y = -1f;
        }
        else
        {
            aimLocalScale.y = +1f;
        }
        aimTransform.localScale = aimLocalScale;

        playerLookAt.SetLookAtPosition(mousePosition);
    }

    private void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint();

            aimAnimator.SetTrigger("Shoot");

            OnShoot?.Invoke(this, new OnShootEventArgs
            {
                gunEndPointPosition = aimGunEndPointTransform.position,
                shootPosition = mousePosition,
                shellPosition = aimShellPositionTransform.position,
            });
        }
    }
}*/