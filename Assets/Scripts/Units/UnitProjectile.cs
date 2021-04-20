using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private int damageToDeal = 20;
    
    [SerializeField]
    private float destroyAfterSecond = 5;

    [SerializeField]
    private float launchForce = 10;

    private void Start()
    {
        rb.velocity = transform.forward * launchForce;
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfterSecond);
    }
    
    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
    
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NetworkIdentity>(out var networkIdentity))
        {
            if (networkIdentity.connectionToClient == connectionToClient)
            {
                return;
            }

            if (other.TryGetComponent<Health>(out var health))
            {
                health.DealDamage(damageToDeal);
            }
            DestroySelf();
        }
    }
}
