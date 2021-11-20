using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerSyncBehaviour : NetworkBehaviour
{
    private readonly NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SetRandomPosition();
        }
    }

    private void SetRandomPosition()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var position = GetRandomPositionOnPlane();
            transform.position = position;
            networkedPosition.Value = position;
        }
        else
        {
            SubmitPositionRequestServerRpc();
        }
    }
    
    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        networkedPosition.Value = GetRandomPositionOnPlane();
    }
    
    private static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
    }
    
    private void Update()
    {
        transform.position = networkedPosition.Value;
    }
}
