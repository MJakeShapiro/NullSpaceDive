using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyEnemy : MonoBehaviour
{
    public EntityMovement entityMovement;
    public EntityAiming entityAiming;
    public Weapon weapon;
    public bool holdTrigger;
    public bool moveInCircles;

    [SerializeField][ReadOnly]
    private GameObject target;

    private void Awake()
    {

    }

    void Start()
    {
        if (PlayerController.players?[0] != null)
            target = PlayerController.players[0].gameObject;
    }


    void Update()
    {
        Move();
        if (holdTrigger)
            weapon.Action1(true);
    }

    private void Move()
    {
        if (moveInCircles)
            entityMovement.SetMoveDirection(new Vector2(Mathf.Sin(Time.time*1.5f), Mathf.Cos(Time.time*1.5f)));
        //entityMovement.SetMoveDirection(new Vector2(Mathf.Sin(Time.time * 2f + Mathf.PI / 2), Mathf.Cos(Time.time + Mathf.PI / 2)));
    }
}