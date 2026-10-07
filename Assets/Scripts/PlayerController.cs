using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float aimAngleOffset = -90f;

    [SerializeField] private Transform playerVisual;
    [SerializeField] private GameObject localPlayerMarker;

    private PlayerNetworkData networkData;
    private Vector2 aimInput;

    private PlayerControls controls;
    private Vector3 moveInput;
    private Rigidbody rb;

    private void Awake()
    {
        controls = new PlayerControls();
        rb = GetComponent<Rigidbody>();
        networkData = GetComponent<PlayerNetworkData>();
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

        if (!networkData.IsAlive.Value)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = new Vector3(controls.Player.Move.ReadValue<Vector2>().x, 0f, controls.Player.Move.ReadValue<Vector2>().y);
        aimInput = controls.Player.Aim.ReadValue<Vector2>();

        float distFromCamera = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(aimInput.x, aimInput.y, distFromCamera));

        Vector3 direction = mouseWorld - transform.position;
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg + aimAngleOffset;

        playerVisual.localRotation = Quaternion.Euler(0f, angle, 0f);
        networkData.SetFacingAngleRpc(angle);

        if(moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }

        /*

        if(moveInput.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        */

    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        if(!networkData.IsAlive.Value)
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
