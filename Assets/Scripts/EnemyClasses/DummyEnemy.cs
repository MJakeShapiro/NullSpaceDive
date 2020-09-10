using NaughtyAttributes;
using UnityEngine;

public class DummyEnemy : EntityController
{
    public bool holdTrigger;
    public bool chasePlayer;
    public bool moveInCircles;
    public float range = 6;

    [SerializeField] [ReadOnly]
    private GameObject target = default;

    private void Awake()
    {

    }

    void Start()
    {
        if (PlayerController.players?[0] != null)
            target = PlayerController.players[0].gameObject;
    }

    protected override void HandleMovement()
    {
        if (chasePlayer && target!=null && DistanceFromTarget() > range - 0.5f)
            container.movement.SetMoveDirection(target.transform.position - transform.position);
        else if (moveInCircles)
            container.movement.SetMoveDirection(new Vector2(Mathf.Sin(Time.time * 1.5f), Mathf.Cos(Time.time * 1.5f)));
        else
            container.movement.StopMoving();
        //entityMovement.SetMoveDirection(new Vector2(Mathf.Sin(Time.time * 2f + Mathf.PI / 2), Mathf.Cos(Time.time + Mathf.PI / 2)));
    }

    protected override void HandleAiming()
    {
        if (target != null && DistanceFromTarget() < range + 0.5f)
            container.aiming.SetLookTarget(target.transform);
        else
            container.aiming.LookInDirection(new Vector2(-1, -1));
    }

    protected override void HandleEquipment()
    {
        if (!container.equipment.IsHoldingWeapon())
            return;

        if (holdTrigger)
            container.equipment.TriggerAction1(true);
        else if (target && DistanceFromTarget() < range)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, target.transform.position - transform.position);
            if (hit.collider != null && hit.collider.GetComponentInParent<Entity>()?.faction == Faction.Player)
                container.equipment.TriggerAction1(true);
            else
                container.equipment.TriggerAction1(false);
        }
        else
            container.equipment.TriggerAction1(false);
    }

    private float DistanceFromTarget ()
    {
        if (target == null)
            return float.PositiveInfinity;
        return (target.transform.position - transform.position).magnitude;
    }
}