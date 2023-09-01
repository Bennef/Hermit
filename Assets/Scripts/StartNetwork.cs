using Unity.Netcode;
using UnityEngine;

public class StartNetwork : MonoBehaviour
{
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    public void StartHost()
    {
        Debug.Log("Starting Host");
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        Debug.Log("Starting Client");
        NetworkManager.Singleton.StartClient();
    }
}
