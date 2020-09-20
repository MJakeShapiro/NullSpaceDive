using UnityEngine;

public class ShieldController : EntityController
{
    #region Properties
    public float duration = -1;
    public Animator animator;

    protected float awakeTime;
    #endregion Properties

    #region Initialization
    private void Awake ()
    {
        awakeTime = Time.time;
    }
    #endregion

    #region UpdateMethods
    protected override void HandleEquipment ()
    {
        if (duration>0 && Time.time>=awakeTime+duration)
            BreakShield();
    }
    #endregion

    #region EventMethods
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
    #endregion
}