using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    [SerializeField] private float moveSpeed = 5f;

    private PlayerControls controls;
    private Vector3 moveInput;
    private Rigidbody rb;

    private void Awake()
    {
        controls = new PlayerControls();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
        if(!IsOwner)
        {
            return;
        }

        moveInput = controls.Player.Move.ReadValue<Vector3>();
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        Vector3 nextPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);

    }
}
