#nullable enable

using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ServerObjectsSupplier : MonoBehaviour
{
    [SerializeField]
    private NetworkManager? networkManager;
    
    [SerializeField]
    private List<GameObject>? supplyingObjects;
    
    private readonly Dictionary<ulong, NetworkObject> playerNetworkObjects = new Dictionary<ulong, NetworkObject>();

    public IReadOnlyList<ulong> PlayerClientIds => playerNetworkObjects.Keys.ToList();

    public NetworkObject? GetPlayerNetworkObject(ulong clientId) => playerNetworkObjects[clientId];

    public NetworkObject? GetNetworkObjectFromId(ulong networkObjectId)
    {
        if (networkManager != null)
        {
            return networkManager.SpawnManager.SpawnedObjects[networkObjectId];
        }
        else
        {
            return null;
        }
    }

    private void Start()
    {
        if (networkManager != null)
        {
            networkManager.OnServerStarted += SupplyObjects;
            networkManager.OnClientConnectedCallback += OnClientConnected;
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }
    
    private void SupplyObjects()
    {
        if (supplyingObjects == null) return;

        foreach (var supplyingObject in supplyingObjects)
        {
            if (supplyingObject != null)
            {
                Instantiate(supplyingObject);
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (networkManager == null) return;

        var networkObject = networkManager.SpawnManager.GetPlayerNetworkObject(clientId);
        if (networkObject != null)
        {
            playerNetworkObjects.Add(clientId, networkObject);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (networkManager == null) return;

        playerNetworkObjects.Remove(clientId);
    }
}
