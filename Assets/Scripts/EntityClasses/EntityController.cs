using UnityEngine;

public class EntityController : MonoBehaviour
{
    #region Properties
    protected Entity.EntityReferenceContainer container;
    #endregion Properties

    #region Initialization
    /// <summary>
    /// Sets the Entity references to other Entity componenets
    /// </summary>
    /// <param name="_container">Reference Container Class</param>
    public virtual void SetEntityReference(Entity.EntityReferenceContainer _container)
    {
        container = _container;
    }
    #endregion Initialization

    #region UpdateMethods
    protected virtual void Update()
    {
        HandleMovement();
        HandleAiming();
        HandleEquipment();
    }

    protected virtual void HandleMovement() { }
    protected virtual void HandleAiming() { }
    protected virtual void HandleEquipment() { }
    #endregion UpdateMethods

    #region EventMethods
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
    #endregion EventMethods
}