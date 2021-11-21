#nullable enable

using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerSyncBehaviour : NetworkBehaviour
{
    private readonly NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>(
        NetworkVariableReadPermission.OwnerOnly
    );
    
    private readonly NetworkVariable<Vector3> networkedInputPosition = new NetworkVariable<Vector3>(
        NetworkVariableReadPermission.OwnerOnly
    );

    private PlayerInputHelper? playerInputHelper = null;

    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float estimateTimeRange = 0.5f;

    private float remainEstiamtionTime = 0f;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            var playerCamera = FindObjectOfType<PlayerChaseCamera>();
            playerCamera.SetAsPlayer(gameObject);
            Camera? activatedCamera = null;
            playerCamera.TryActivatePlayerCamera(Camera.main, out activatedCamera);

            playerInputHelper = FindObjectOfType<PlayerInputHelper>();
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
            SubmitRansomPositionRequestServerRpc();
        }
    }
    
    [ServerRpc]
    void SubmitRansomPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        var position = GetRandomPositionOnPlane();
        networkedPosition.Value = position;
    }

    private static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }

    [ServerRpc]
    void UpdateCyclePositionRequestServerRpc(Vector3 inputPosition)
    {
        remainEstiamtionTime = estimateTimeRange;
        networkedInputPosition.Value = inputPosition;
    }
    
    [ServerRpc]
    void UpdatePositionRequestServerRpc(Vector3 position)
    {
        networkedPosition.Value = position;
    }
    
    private void Update()
    {
        var delta = Time.deltaTime;
        Vector3 position = networkedPosition.Value;

        if (IsOwner)
        {
            var move = playerInputHelper?.move;

            if (move.HasValue)
            {
                var value = move.Value;
                if (value.magnitude > 0.1f)
                {
                    var currentPosition = new Vector3(
                        position.x + delta * value.x * speed, 
                        position.y, 
                        position.z + delta * value.y * speed);
                    
                    if (IsServer)
                    {
                        transform.position = currentPosition;
                        networkedPosition.Value = currentPosition;
                    }
                    else
                    {
                        var estimatingPosition = new Vector3(
                            position.x + estimateTimeRange * value.x * speed, 
                            position.y, 
                            position.z + estimateTimeRange * value.y * speed);
                        
                        transform.position = currentPosition;
                        UpdateCyclePositionRequestServerRpc(new Vector3(value.x, 0f, value.y));
                    }
                }
            }
        }
        else
        {
            if (IsServer && remainEstiamtionTime > 0f)
            {
                networkedPosition.Value = position + (networkedInputPosition.Value * speed * delta);
                remainEstiamtionTime -= delta;
            }
            transform.position = networkedPosition.Value;
        }
    }
}
