using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class DummyEnemy : EntityController
{
    public Weapon weapon;
    public bool holdTrigger;
    public bool chasePlayer;
    public bool moveInCircles;
    public float range = 6;

    [SerializeField] [ReadOnly]
    private GameObject target = default;

    void Start()
    {
        if (PlayerController.players?[0] != null)
            target = PlayerController.players[0].gameObject;
    }

    protected override void HandleMovement()
    {
        if (chasePlayer && DistanceFromTarget() > range - 0.5f)
            container.movement.SetMoveDirection(target.transform.position - transform.position);
        else if (moveInCircles)
            container.movement.SetMoveDirection(new Vector2(Mathf.Sin(Time.time * 1.5f), Mathf.Cos(Time.time * 1.5f)));
        else
            container.movement.StopMoving();
        //entityMovement.SetMoveDirection(new Vector2(Mathf.Sin(Time.time * 2f + Mathf.PI / 2), Mathf.Cos(Time.time + Mathf.PI / 2)));
    }

    protected override void HandleAiming()
    {
        if (DistanceFromTarget() < range + 0.5f)
            container.aiming.SetLookTarget(target.transform);
        else
            container.aiming.LookInDirection(new Vector2(-1,-1));
    }

    protected override void HandleEquipment()
    {
        if (holdTrigger)
            weapon.Action1(true);
        else if (target && DistanceFromTarget() < range)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, target.transform.position - transform.position);
            if (hit.collider != null)
            {
                Debug.Log(hit.collider.name);
                if (hit.collider.GetComponentInParent<Entity>()?.faction == Faction.Player) // Can see player
                {
                    weapon.Action1(true);
                }
            }
        }
    }

    private float DistanceFromTarget ()
    {
        if (target == null)
            return float.PositiveInfinity;
        return (target.transform.position - transform.position).magnitude;
    }
}