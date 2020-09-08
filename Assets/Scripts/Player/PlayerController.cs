using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : EntityController
{
    public static List<PlayerController> players = new List<PlayerController>();

    public PlayerInput input;

    [SerializeField]
    private List<Rumble> rumbles = new List<Rumble>();

    [HorizontalLine]
    [Header("Aiming")]
    [Tooltip("Transform of the players crosshair")]
    public Transform crosshair;
    public bool useLookΔ = false;

    [ReadOnly]
    public Vector2 mousePosition; // Screen position of mouse, OR input direction in Δ mode
    [ReadOnly]
    public Vector2 crosshairPosition; // World position of crosshair
    [ReadOnly]
    public Vector2 lookDirection; // Direction player is looking
    [Range(0,10)]
    public float lookΔDistance; // crosshair distance from player
    [Range(0,10)]
    public int lookΔSmoothing;

    private List<Vector2> smoothingList = new List<Vector2>();

    [HorizontalLine]
    //public EntityEquipment entityEquipment;
    public Weapon activeWeapon;


    protected void OnEnable()
    {
        players.Add(this);
    }

    protected void OnDisable()
    {
        players.Remove(this);
    }

    protected void Start()
    {
        activeWeapon.Initialize(container.entity);
    }

    protected override void Update()
    {
        base.Update();
        HandleRumble();
    }

    protected override void HandleMovement()
    {
        Vector2 dir = input.actions.FindAction("Move").ReadValue<Vector2>();
        container.movement.SetMoveDirection(dir, dir.magnitude);
    }

    protected override void HandleAiming ()
    {
        if (useLookΔ)
        {
            mousePosition = input.actions.FindAction("LookΔ").ReadValue<Vector2>(); // mousePosition is used to display input direction

            if (mousePosition != Vector2.zero)
            {
                //float distanceScalar = Camera.main.aspect * Camera.main.orthographicSize * 2 * lookΔDistance; // Converts normalized vector in to screen units
                if (lookΔSmoothing > 0)
                {
                    smoothingList.Add(mousePosition);
                    if (smoothingList.Count > lookΔSmoothing)
                        smoothingList.RemoveAt(0);
                    lookDirection = Vector2.zero;
                    for (int i = 0; i < smoothingList.Count; i++)
                        lookDirection += smoothingList[i] * (6 + i); // 6 is weight factor, lower -> more weight to recent values
                    lookDirection.Normalize();
                }
                else
                    lookDirection = mousePosition;
            }
            crosshairPosition = new Vector2(transform.position.x, transform.position.y) + (lookDirection * lookΔDistance);
        }
        else
        {
            mousePosition = input.actions.FindAction("Look").ReadValue<Vector2>();
            crosshairPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            lookDirection = (crosshairPosition - (Vector2)transform.position).normalized;
        }

        container.aiming.LookInDirection(lookDirection);
        UpdateCrosshair();
    }

    protected override void HandleEquipment()
    {
        activeWeapon?.Action1(input.actions.FindAction("Fire").ReadValue<float>()>=0.5f);
    }

    private void HandleRumble ()
    {
        for (int i=0; i<rumbles.Count; i++)
        {
            if (!rumbles[i].StillValid(Time.time))
            {
                rumbles.RemoveAt(i);
                i--;
            }
        }

        if (rumbles.Count == 0)
            Gamepad.current?.SetMotorSpeeds(0, 0);
        else
            Gamepad.current?.SetMotorSpeeds(rumbles[0].speed.x, rumbles[0].speed.y);
    }

    public void StartRumble (Vector2 _speed, float _priority, int _duration)
    {
        StartRumble(new Rumble(_speed, _priority, _duration));
    }

    public void StartRumble(Rumble _rumble)
    {
        if (rumbles.Count == 0)
            Gamepad.current?.SetMotorSpeeds(_rumble.speed.x, _rumble.speed.y);


        _rumble.Start(Time.time);
        rumbles.Add(_rumble);
        rumbles = rumbles.OrderBy(r => r.priority).ToList(); // Linq magic from the internet
    }

    public void CancelRumble (Rumble _rumble)
    {
        rumbles.Remove(_rumble);
    }

    public void ClearRumbles ()
    {
        rumbles.Clear();
        Gamepad.current.SetMotorSpeeds(0, 0);
    }

    private void UpdateCrosshair()
    {
        if (crosshair)
            crosshair.position = crosshairPosition;
    }

    /// <summary> Triggers the PlayerCharacter object to update its handling methods of player input </summary>
    public void ControlsChanged ()
    {
        Debug.Log("Controls changed to: " + input.currentControlScheme);
        Debug.Log("Device info: " + input.devices[0].device + "\nShort Name: " + input.devices[0].shortDisplayName + "\nFull Name: " + input.devices[0].displayName + "\nDiscription: " + input.devices[0].description);
        if (input.currentControlScheme == "Keyboard&Mouse")
        {
            useLookΔ = false;
        }
        else if (input.currentControlScheme == "TouchScreen")
        {
            // Future handler for mobile input
        }
        else
        {
            useLookΔ = true;
        }
    }
}

[System.Serializable]
public class Rumble
{
    public Vector2 speed;
    public float duration;
    public int priority;
    public float endTime = -1;

    public static Rumble bullet = new Rumble(new Vector2(0.45f, 0.15f), 0.1f, 41);
    public static Rumble bullet2 = new Rumble(new Vector2(0.2f, 0.05f), 0.15f, 42);

    public Rumble ()
    {
        speed = Vector2.zero;
        duration = 1;
        priority = 1;
    }

    public Rumble(Vector2 _speed, float _duration, int _priority)
    {
        speed = _speed;
        duration = _duration;
        priority = _priority;
    }

    public Rumble(Rumble _rumble)
    {
        speed = _rumble.speed;
        duration = _rumble.duration;
        priority = _rumble.priority;
    }

    public bool Start (float _time)
    {
        if (endTime != -1)
            return false;

        endTime = _time + duration;
        return true;
    }

    public bool StillValid (float _time)
    {
        if (_time >= endTime)
            return false;
        else
            return true;
    }

    public static Rumble[] GetReloadAnim ()
    {
        Rumble[] anim = new Rumble[6];
        anim[0] = new Rumble(new Vector2(0.0f, 0.0f), 0.1f, 51);
        anim[1] = new Rumble(new Vector2(0.20f, 0.05f), 0.125f, 52);
        anim[2] = new Rumble(new Vector2(0.05f, 0.15f), 0.125f, 53);
        anim[3] = new Rumble(new Vector2(0.0f, 0.0f), 0.40f, 54);
        anim[4] = new Rumble(new Vector2(0.05f, 0.15f), 0.125f, 55);
        anim[5] = new Rumble(new Vector2(0.20f, 0.05f), 0.125f, 56);
        return anim;
    }

    public static Rumble[] ScaleAnimation (Rumble[] _anim, float _duration)
    {
        float sum = 0;
        foreach (Rumble r in _anim)
            sum += r.duration;

        float fac = _duration/sum;
        sum = 0;

        foreach (Rumble r in _anim)
        {
            r.duration = sum + (r.duration*fac);
            sum = r.duration;
        }

        return _anim;
    }
}