using Unity.Netcode;
using UnityEngine;
using TMPro;

public class NetworkGameManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInput;

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void StopConnection()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
    public string GetPlayerName()
    {
        return playerNameInput.text;
    }
}
