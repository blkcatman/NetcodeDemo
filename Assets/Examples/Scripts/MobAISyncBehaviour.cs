#nullable enable

using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MobAISyncBehaviour : NetworkBehaviour
{
    [SerializeField]
    private NavMeshAgent? navMeshAgent;

    private ServerObjectsSupplier? serverObjectsSupplier;

    [SerializeField]
    private float updateRate = 1f;

    private float elapsedTime;
    
    public override void OnNetworkSpawn()
    {
        serverObjectsSupplier = GameObject.FindObjectOfType<ServerObjectsSupplier>();
    }
    
    private void RemoveMobOnServer()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
    }

    [ServerRpc]
    private void SubmitRemovingMobRequestServerRpc()
    {
        RemoveMobOnServer();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            if (IsServer)
            {
                RemoveMobOnServer();
            }
            else
            {
                SubmitRemovingMobRequestServerRpc();
            }
        }
    }

    private void Update()
    {
        if (serverObjectsSupplier == null || navMeshAgent == null) return;
        elapsedTime += Time.deltaTime;

        if (elapsedTime > updateRate)
        {
            NavMeshPath path = new NavMeshPath();

            float minDistance = 1000f;
            Vector3 targetPosition = transform.position;
            bool foundTarget = false;
            
            foreach (var clientId in serverObjectsSupplier.PlayerClientIds)
            {
                var networkObject = serverObjectsSupplier.GetPlayerNetworkObject(clientId);
                if (networkObject == null) continue;

                var source = transform.position;
                var target = networkObject.transform.position;
                
                if (NavMesh.CalculatePath(source, target, navMeshAgent.areaMask, path))
                {
                    float distance = Vector3.Distance(source, target);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        targetPosition = target;
                        foundTarget = true;
                    }
                }
            }

            if (foundTarget)
            {
                navMeshAgent.SetDestination(targetPosition);
            }
            
            elapsedTime -= updateRate;
        }
    }
}
