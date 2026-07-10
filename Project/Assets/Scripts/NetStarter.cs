using UnityEngine;
using Unity.Netcode;

public class NetStarter : MonoBehaviour
{
    public void StartHost()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.StartClient();
    }

    public void StartServer()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.StartServer();
    }
}