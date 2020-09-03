using NaughtyAttributes;
using UnityEditor.XR;
using UnityEngine;

public class EntityAiming : MonoBehaviour
{
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


    public virtual void LookAt(Vector2 _target)
    {
        LookInDirection(_target - (Vector2)rotator.position);
    }


    public virtual void LookInDirection(Vector2 _direction)
    {
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

    public virtual void FlipSprite(bool flip)
    {
        rotator.transform.localScale = new Vector3(rotator.transform.localScale.x, (flip ? -1 : +1) * Mathf.Abs(rotator.transform.localScale.y), rotator.transform.localScale.z);
        spriteFlipped = flip;

        //Debug.Log("Sprite Flipped!\n" + gameObject.name);
        //Call other events here if needed
    }

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

    public virtual void SetLookTarget (Transform _target)
    {
        lookTarget = _target;
        useLookTarget = true;

        useLookLocation = false;
    }

    public virtual void SetLookLocation (Vector3 _location)
    {
        lookLocation = _location;
        useLookLocation = true;

        useLookTarget = false;
    }

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