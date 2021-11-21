using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class NetworkSelector : MonoBehaviour
{
    [SerializeField]
    UnityEvent<string> onStartNetwork;
    
    public void StartNetworkAsHost()
    {
        NetworkManager.Singleton.StartHost();
        onStartNetwork.Invoke("Started as a host");
    }

    public void StartNetworkAsClient()
    {
        NetworkManager.Singleton.StartClient();
        onStartNetwork.Invoke("Started as a client");
    }

    public void StartNetworkAsServer()
    {
        NetworkManager.Singleton.StartServer();
        onStartNetwork.Invoke("Started as a server");
    }
}
