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
    
    [SerializeField]
    private GameObject? playerModel = null;

    [SerializeField]
    private GameObject? playerLocalDummyTemplate = null;
    
    private GameObject? playerLocalDummy = null;

    private float remainEstimationTime = 0f;

    private bool hasInputReleased = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            var playerCamera = FindObjectOfType<PlayerChaseCamera>();

            if (!IsServer && playerLocalDummyTemplate != null)
            {
                playerModel?.SetActive(false);
                playerLocalDummy = Instantiate(playerLocalDummyTemplate);
                playerCamera.SetAsPlayer(playerLocalDummy);
            }
            else
            {
                playerCamera.SetAsPlayer(gameObject);
            }
            
            playerCamera.TryActivatePlayerCamera(Camera.main, out var activatedCamera);

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
    
    private static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }

    [ServerRpc]
    private void SubmitRansomPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        var position = GetRandomPositionOnPlane();
        networkedPosition.Value = position;
    }

    [ServerRpc]
    private void UpdateCyclePositionRequestServerRpc(Vector3 inputPosition)
    {
        remainEstimationTime = estimateTimeRange;
        networkedInputPosition.Value = inputPosition;
    }
    
    [ServerRpc]
    private void UpdatePositionRequestServerRpc(Vector3 position)
    {
        networkedPosition.Value = position;
        remainEstimationTime = 0f;
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
                    var translatePosition = new Vector3(
                         value.x * speed * delta, 
                        0f, 
                        value.y * speed * delta);
                    
                    if (IsServer)
                    {
                        transform.position += translatePosition;
                        networkedPosition.Value += translatePosition;
                    }
                    else
                    {
                        Vector3 lag = Vector3.zero;
                        
                        if (playerLocalDummy != null)
                        {
                            playerLocalDummy.transform.position += translatePosition;
                            lag = playerLocalDummy.transform.position - position;
                        }

                        UpdateCyclePositionRequestServerRpc(new Vector3(value.x, 0f, value.y) + lag);
                        hasInputReleased = false;
                    }
                }
                else
                {
                    if (!IsServer && playerLocalDummy != null && !hasInputReleased)
                    {
                        UpdatePositionRequestServerRpc(playerLocalDummy.transform.position);
                        hasInputReleased = true;
                    }
                }
            }
        }
        else
        {
            if (IsServer && remainEstimationTime > 0f)
            {
                networkedPosition.Value = position + (networkedInputPosition.Value * speed * delta);
                remainEstimationTime -= delta;
            }
            transform.position = networkedPosition.Value;
        }
    }
}
