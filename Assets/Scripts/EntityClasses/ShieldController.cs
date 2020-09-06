using UnityEngine;

public class ShieldController : EntityController
{
    public float duration = -1;
    public Animator animator;

    protected float awakeTime;

    private void Awake ()
    {
        awakeTime = Time.time;
    }

    protected override void HandleEquipment ()
    {
        if (duration>0 && Time.time>=awakeTime+duration)
            BreakShield();
    }

    public override bool OnDeath ()
    {
        BreakShield();
        return true;
    }

    protected virtual void BreakShield ()
    {
        animator.SetTrigger("Break");
        //Temporary
        Destroy(gameObject, 1);
    }
}