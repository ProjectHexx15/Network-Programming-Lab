using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private GameObject localPlayerMarker;

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

        moveInput = new Vector3(controls.Player.Move.ReadValue<Vector2>().x, 0f, controls.Player.Move.ReadValue<Vector2>().y);

        if(moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }

        if(moveInput.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);

    }

    public override void OnNetworkSpawn()
    {
        if(localPlayerMarker != null)
        {
            localPlayerMarker.SetActive(IsOwner); 
        }

        if(!IsOwner)
        {
            return;
        }

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();

        cameraFollow.SetTarget(transform);

    }
}
