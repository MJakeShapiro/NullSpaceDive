using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    #region Properties
    [Header("Interactable Base")]
    public bool destroyOnGet = true;
    public bool deactivateOnGet = false;
    public bool preventSelect = false;

    protected Animator animator;
    protected new Collider2D collider;
    #endregion

    #region Initialization
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider2D>();

        #region Debug
#if UNITY_EDITOR
        if (animator == null)
            Debug.LogWarning("Couldn't find animator component for " + name);
#endif
        #endregion
    }
    #endregion

    #region PublicMethods
    public void SetSelectedState(bool selected)
    {
        animator.SetBool("IsHighlighted", selected);

        if (selected)
            OnHighlight();
        else
            OnUnhighlight();
    }

    public void Select(Entity playerEntity)
    {
        #region Debug
#if UNITY_EDITOR
        if (playerEntity.container.equipment == null)
            Debug.LogWarning("Supplied entity doesnt have an equipment conponent. This script and its children are not deisnged to handle this case.");
#endif
        #endregion

        if (!preventSelect && OnSelected(playerEntity))
        {
            collider.enabled = false;
            animator.SetTrigger("Select");
            // Play audio here

            StartCoroutine(OnRemove());
        }
        else
        {
            animator.SetTrigger("FailedSelect");
            // Play fail audio here
        }
    }
    #endregion

    #region EventMethods
    protected virtual void OnHighlight ()
    {

    }

    protected virtual void OnUnhighlight ()
    {

    }

    protected virtual bool OnSelected (Entity playerEntity)
    {
        return true;
    }

    protected virtual IEnumerator OnRemove ()
    {
        yield return new WaitForSeconds(0.6f);

        if (destroyOnGet)
            Destroy(gameObject);
        else if (deactivateOnGet)
            gameObject.SetActive(false);
    }
    #endregion
}