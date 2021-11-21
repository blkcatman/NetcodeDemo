#nullable enable

using System.Numerics;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class PlayerSyncBehaviour : NetworkBehaviour
{
    private readonly NetworkVariable<Vector3> networkMovingDirection = new NetworkVariable<Vector3>(
        NetworkVariableReadPermission.Everyone
    );
    
    private readonly NetworkVariable<Vector3> networkCurrentPosition = new NetworkVariable<Vector3>(
        NetworkVariableReadPermission.Everyone
    );

    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private GameObject? playerModel = null;

    [SerializeField]
    private GameObject? playerLocalDummyTemplate = null;

    [SerializeField]
    private UnityEvent<bool>? onEnterInput;
    
    private PlayerInputHelper? playerInputHelper = null;

    private GameObject? playerLocalDummy = null;

    private bool isCycleUpdateEnabled = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            var playerCamera = FindObjectOfType<PlayerChaseCamera>();

            if (!NetworkManager.Singleton.IsServer)
            {
                if (playerLocalDummyTemplate != null)
                {
                    playerModel?.SetActive(false);
                    playerLocalDummy = Instantiate(playerLocalDummyTemplate);
                    playerCamera.SetAsPlayer(playerLocalDummy);
                }
                else
                {
                    playerCamera.SetAsPlayer(gameObject);
                }
            }
            else
            {
                playerCamera.SetAsPlayer(gameObject);
            }
            
            playerCamera.TryActivatePlayerCamera(Camera.main, out _);

            playerInputHelper = FindObjectOfType<PlayerInputHelper>();
            SetRandomPosition();
        }
    }

    private void SetRandomPosition()
    {
        var position = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
        
        if (NetworkManager.Singleton.IsServer)
        {
            transform.position = position;
            networkMovingDirection.Value = Vector3.zero;
            networkCurrentPosition.Value = position;
            SubmitCycleUpdateFlagRequestClientRpc(false);
        }
        else
        {
            SubmitCurrentPositionRequestServerRpc(position);
        }
    }

    [ServerRpc]
    private void SubmitMovingDirectionRequestServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
    {
        if (!isCycleUpdateEnabled)
        {
            SubmitCycleUpdateFlagRequestClientRpc(true);
        }
        isCycleUpdateEnabled = true;
        networkMovingDirection.Value = direction;
        networkCurrentPosition.Value = Vector3.zero;
    }
    
    [ServerRpc]
    private void SubmitCurrentPositionRequestServerRpc(Vector3 position, ServerRpcParams rpcParams = default)
    {
        if (isCycleUpdateEnabled)
        {
            SubmitCycleUpdateFlagRequestClientRpc(false);
        }
        isCycleUpdateEnabled = false;
        networkMovingDirection.Value = Vector3.zero;
        networkCurrentPosition.Value = position;
    }

    [ClientRpc]
    private void SubmitCycleUpdateFlagRequestClientRpc(bool flag, ServerRpcParams rpcParams = default)
    {
        isCycleUpdateEnabled = flag;
    }

    private void Update()
    {
        var delta = Time.deltaTime;

        if (IsOwner)
        {
            var move = playerInputHelper?.move;

            if (move.HasValue)
            {
                var value = move.Value;
                if (value.magnitude > 0.1f)
                {
                    var direction = new Vector3(value.x, 0f, value.y);

                    var translatePosition = direction * speed * delta;
                    
                    if (IsServer)
                    {
                        if (!isCycleUpdateEnabled)
                        {
                            SubmitCycleUpdateFlagRequestClientRpc(true);
                        }
                        isCycleUpdateEnabled = true;
                        networkMovingDirection.Value = direction;
                        networkCurrentPosition.Value = Vector3.zero;
                    }
                    else
                    {
                        if (playerLocalDummy != null)
                        {
                            playerLocalDummy.transform.position += translatePosition;
                        }
                        SubmitMovingDirectionRequestServerRpc(direction);
                    }
                }
                else
                {
                    if (IsServer)
                    {
                        if (isCycleUpdateEnabled)
                        {
                            SubmitCycleUpdateFlagRequestClientRpc(false);
                        }
                        isCycleUpdateEnabled = false;
                        networkMovingDirection.Value = Vector3.zero;
                        networkCurrentPosition.Value = transform.position;
                    }
                    else
                    {
                        if (playerLocalDummy != null)
                        {
                            SubmitCurrentPositionRequestServerRpc(playerLocalDummy.transform.position);
                        }
                    }
                }
            }
        }

        var movingDirection = networkMovingDirection.Value;
        var currentPosition = networkCurrentPosition.Value;
        
        if (movingDirection.magnitude > 0.001f)
        {
            transform.position += movingDirection * speed * delta;
        }
        else if (currentPosition.magnitude > 0.001f)
        {
            transform.position = currentPosition;
        }
    }
}
