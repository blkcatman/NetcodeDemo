#nullable enable

using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSyncBehaviour : NetworkBehaviour
{
    private readonly NetworkVariable<Vector3> networkMovingDirection = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone
    );
    
    private readonly NetworkVariable<Vector3> networkCurrentPosition = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone
    );

    [SerializeField]
    private float speed = 5f;
    
    private PlayerInputHelper? playerInputHelper = null;

    private GameObject? playerLocalDummy = null;

    [SerializeField]
    private UnityEvent<GameObject>? onTrackingObjectPresented;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerInputHelper = FindObjectOfType<PlayerInputHelper>();
            
            if (IsServer)
            {
                onTrackingObjectPresented?.Invoke(gameObject);
            }
            else
            {
                playerLocalDummy = new GameObject("PlayerLocalDummy");
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).SetParent(playerLocalDummy.transform, false);
                }
                onTrackingObjectPresented?.Invoke(playerLocalDummy!);
            }
            
            SetRandomPosition();
        }
    }
    
    private void UpdateVariablesOnServer(Vector3 movingDirection, Vector3 currentPosition)
    {
        networkMovingDirection.Value = movingDirection;
        networkCurrentPosition.Value = currentPosition;
    }

    private void SetRandomPosition()
    {
        var position = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
        
        if (IsServer)
        {
            transform.position = position;
            UpdateVariablesOnServer(Vector3.zero, position);
        }
        else
        {
            SubmitCurrentPositionRequestServerRpc(position);
        }
    }

    [ServerRpc]
    private void SubmitMovingDirectionRequestServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
    {
        UpdateVariablesOnServer(direction, Vector3.zero);
    }
    
    [ServerRpc]
    private void SubmitCurrentPositionRequestServerRpc(Vector3 position, ServerRpcParams rpcParams = default)
    {
        UpdateVariablesOnServer(Vector3.zero, position);
    }

    private void UpdatePlayerInputs(float delta)
    {
        if (playerInputHelper == null) return;
        
        if (playerInputHelper!.HasMoveInput)
        {
            var move = playerInputHelper!.Move;
            var direction = new Vector3(move.x, 0f, move.y);
                    
            if (IsServer)
            {
                UpdateVariablesOnServer(direction, Vector3.zero);
            }
            else
            {
                if (playerLocalDummy != null)
                {
                    var position = playerLocalDummy.transform.position;
                    playerLocalDummy.transform.LookAt(direction + position, Vector3.up);
                    playerLocalDummy.transform.position += direction * speed * delta;
                }
                SubmitMovingDirectionRequestServerRpc(direction);
            }
        }
        else
        {
            if (IsServer)
            {
                UpdateVariablesOnServer(Vector3.zero, transform.position);
            }
            else
            {
                SubmitCurrentPositionRequestServerRpc(playerLocalDummy != null ?
                    playerLocalDummy.transform.position : transform.position);
            }
        }
    }

    private void Update()
    {
        var delta = Time.deltaTime;

        if (IsOwner)
        {
            UpdatePlayerInputs(delta);
        }

        var currentPosition = networkCurrentPosition.Value;
        var movingDirection = networkMovingDirection.Value;

        if (movingDirection.magnitude > 0.001f)
        {
            var position = transform.position;
            var direction = movingDirection.normalized;
            transform.LookAt(direction + position, Vector3.up);
            transform.position += movingDirection * speed * delta;
        }
        else
        {
            transform.position = currentPosition;
        }
    }
}
