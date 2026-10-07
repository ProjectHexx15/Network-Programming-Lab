using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetworkData : NetworkBehaviour
{
    [SerializeField] private TMP_Text playerInfoText;
    [SerializeField] private Transform playerVisual;

    public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>("Player");
    public NetworkVariable<float> FacingAngle = new NetworkVariable<float>(0f);

    public NetworkVariable<int> Health = new NetworkVariable<int>(100);
    public NetworkVariable<bool> IsAlive = new NetworkVariable<bool>(true);

    public NetworkVariable<int> Score = new NetworkVariable<int>(0);

    public override void OnNetworkSpawn()
    {
        PlayerName.OnValueChanged += OnNameChanged;
        Health.OnValueChanged += OnIntValueChanged;
        Score.OnValueChanged += OnIntValueChanged;
        FacingAngle.OnValueChanged += OnFacingAngleChanged;
        IsAlive.OnValueChanged += OnAliveChanged;

        UpdatePlayerDisplay();
        UpdateAliveDisplay();
        ApplyFacingAngle(FacingAngle.Value);

        if (!IsOwner)
        {
            return;
        }

        NetworkGameManager gameManager = FindFirstObjectByType<NetworkGameManager>();

        string requestedName = gameManager.GetPlayerName();

        if (string.IsNullOrWhiteSpace(requestedName))
        {
            requestedName = $"Player {OwnerClientId}";
        }

        SetPlayerNameRpc(new FixedString64Bytes(requestedName));
    }

    public override void OnNetworkDespawn()
    {
        PlayerName.OnValueChanged -= OnNameChanged;
        Health.OnValueChanged -= OnIntValueChanged;
        Score.OnValueChanged -= OnIntValueChanged;
        FacingAngle.OnValueChanged -= OnFacingAngleChanged;
        IsAlive.OnValueChanged -= OnAliveChanged;
    }

    private void OnNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        UpdatePlayerDisplay();
    }

    private void OnIntValueChanged(int oldValue, int newValue)
    {
        UpdatePlayerDisplay();
    }

    private void OnAliveChanged(bool previousValue, bool newValue)
    {
        UpdatePlayerDisplay();
        UpdateAliveDisplay();

        if(!newValue)
        {
            Debug.Log($"{PlayerName.Value} is now dead on this client.");
        }
    }

    private void OnFacingAngleChanged(float oldAngle, float newAngle)
    {
        if (IsOwner)
        {
            return;
        }

        ApplyFacingAngle(newAngle);
    }

    private void UpdatePlayerDisplay()
    {
        if (playerInfoText == null)
        {
            return;
        }

        string status = "";

        if (!IsAlive.Value)
        {
            status = "\nDead";
        }

        playerInfoText.text =
            $"{PlayerName.Value}\n" +
            $"HP: {Health.Value}\n" +
            $"Score: {Score.Value}" +
            status;
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerNameRpc(FixedString64Bytes newName)
    {
        string value = newName.ToString().Trim();

        if (string.IsNullOrEmpty(value))
        {
            value = $"Player {OwnerClientId}";
        }

        if (value.Length > 16)
        {
            value = value.Substring(0, 16);
        }

        PlayerName.Value = new FixedString64Bytes(value);
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer)
        {
            return;
        }

        if(!IsAlive.Value)
        {
            return;
        }

        Health.Value = Mathf.Max(Health.Value - damage, 0);

        if (Health.Value == 0)
        {
            IsAlive.Value = false;

            Debug.Log($"{PlayerName.Value} has died");
        }
    }

    public void AddScore(int amount)
    {
        if (!IsServer)
        {
            return;
        }

        Score.Value += amount;
    }

    private void ApplyFacingAngle(float angle)
    {
        if(playerVisual == null)
        {
            return;
        }

        playerVisual.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void UpdateAliveDisplay()
    {
        if(playerVisual != null)
        {
            playerVisual.gameObject.SetActive(IsAlive.Value);
        }
    }

    [Rpc(SendTo.Server)]
    public void SetFacingAngleRpc(float angle)
    {
        FacingAngle.Value = angle;
    }

    private void Update()
    {
        // Temporary test code for Lab 01 only. 
        if (!IsServer)
        {
            return;
        }

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            TakeDamage(10);
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            AddScore(1);
        }
    }
}