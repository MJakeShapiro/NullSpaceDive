using NaughtyAttributes;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;

namespace Staik
{
    public class PlayerMovement : MonoBehaviour
    {
        public float moveSpeed;

        public bool useLookΔ;
        public bool useVelocity;

        [SerializeField]
        private Vector3 lookAtPosition = default;

        [HorizontalLine]

        public InputAction moveAction;
        public InputAction lookΔAction;
        public InputAction lookAction;

        private Rigidbody2D rb;
        private Vector2 moveDirection;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            moveAction.started += OnMove;
            moveAction.performed += OnMove;
            moveAction.canceled += OnMove;
            lookΔAction.performed += OnΔLook;
            lookAction.performed += OnLook;
        }

        void OnEnable()
        {
            moveAction.Enable();
            lookΔAction.Enable();
            lookAction.Enable();
        }

        void OnDisable()
        {
            moveAction.Disable();
            lookΔAction.Disable();
            lookAction.Disable();
        }

        private void Update()
        {
            HandleInput();
            //HandleLooking();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        public void SetLookAtPosition(Vector3 lookAtPosition)
        {
            this.lookAtPosition = lookAtPosition;
        }

        private void HandleInput()
        {
            moveDirection = moveAction.ReadValue<Vector2>();
        }

        private void HandleMovement()
        {
            //Debug.Log("Direction: " + moveDirection);

            if (useVelocity)
                rb.velocity = moveDirection * moveSpeed;
            else
                rb.MovePosition(rb.position + (moveDirection*moveSpeed*Time.fixedDeltaTime));
        }


        private void HandleLooking()
        {

        }

        private void OnMove(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:

                    return;
                case InputActionPhase.Performed:

                    return;
                case InputActionPhase.Canceled:

                    return;
            }
        }

        private void OnΔLook(InputAction.CallbackContext context)
        {
            if (useLookΔ)
                return;
            //Debug.Log("OnΔLook: " + context.phase);
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            if (!useLookΔ)
                return;
            //Debug.Log("OnLook: " + context.phase);
        }
    }
}




/*
void Update()
{
    var moveDirection = moveAction.ReadValue<Vector2>();
    //position += moveDirection * moveSpeed * Time.deltaTime;
    //rb.velocity = moveDirection * moveSpeed;
}
*/



/*
    void Awake()
    {
        fireAction.performed += OnFire;
        lookAction.performed += OnLook;

        gameplayActions["fire"].performed += OnFire;
    }

    void OnEnable()
    {
        fireAction.Enable();
        lookAction.Enable();

        gameplayActions.Enable();
    }

    void OnDisable()
    {
        fireAction.Disable();
        lookAction.Disable();

        gameplayActions.Disable();
    }
*/