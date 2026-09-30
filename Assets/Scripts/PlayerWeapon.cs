using Unity.Netcode;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private NetworkObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireInterval = 0.25f;

    private PlayerControls controls;
    private float nextFireTime;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            controls.Player.Enable();
        }
    }

    public override void OnNetworkDespawn()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
        if(!IsOwner)
        {
            return;
        }

        if(controls.Player.Fire.WasPressedThisFrame())
        {
            RequestFireRpc(firePoint.position, firePoint.rotation);
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestFireRpc(Vector3 spawnPostiion, Quaternion spawnRotation)
    {
        if(Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime = Time.time + fireInterval;

        NetworkObject projectile = Instantiate(projectilePrefab, spawnPostiion, spawnRotation);
        NetworkProjectile projectileData = projectile.GetComponent<NetworkProjectile>();
        projectileData.Initialise(OwnerClientId);
        projectile.Spawn();
    
    }
}
