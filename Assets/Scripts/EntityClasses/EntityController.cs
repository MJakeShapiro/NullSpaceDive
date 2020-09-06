using UnityEngine;

public class EntityController : MonoBehaviour
{
    protected Entity.EntityReferenceContainer container;

    protected virtual void Update()
    {
        HandleMovement();
        HandleAiming();
        HandleEquipment();
    }


    /// <summary>
    /// Sets the Entity references to other Entity componenets
    /// </summary>
    /// <param name="_container">Reference Container Class</param>
    public virtual void SetEntityReference(Entity.EntityReferenceContainer _container)
    {
        container = _container;
    }

    protected virtual void HandleMovement() { }
    protected virtual void HandleAiming() { }
    protected virtual void HandleEquipment() { }

    /// <summary>
    /// Is called whenever the attached Entity runs out of shields
    /// </summary>
    public virtual void OnShieldBreak() { }

    /// <summary>
    /// Is called whenever the attached Entity runs out of armor
    /// </summary>
    public virtual void OnArmorBreak() { }

    /// <summary>
    /// Is called whenever the attached Entity runs out of health
    /// </summary>
    public virtual void OnHealthBreak() { }

    /// <summary>
    /// Is called whenever the attached Entity is killed
    /// </summary>
    /// <returns>True to override default death</returns>
    public virtual bool OnDeath() { return false; }
}