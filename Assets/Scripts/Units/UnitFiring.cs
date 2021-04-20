using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField]
    private Targeter target;

    [SerializeField]
    private GameObject projectilePrefab;

    [SerializeField]
    private Transform projectileSpawnPoint;

    [SerializeField]
    private float fireRange = 5f;

    [SerializeField]
    private float fireRate = 1f;

    [SerializeField]
    private float rotationSpeed = 20f;

    private float lastFireTime;

    #region Server

    [ServerCallback]
    private void Update()
    {
        Targetable targetable = target.Targetable;
        if (targetable == null)
        {
            return;
        }
        
        if (!CanFireAtTarget())
        {
            return;
        }
        
        Quaternion targetRotation = 
            Quaternion.LookRotation(targetable.transform.position - transform.position);
        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Time.time > (1 / fireRate) + lastFireTime)
        {
            Quaternion projectileRotation =
                Quaternion.LookRotation(targetable.AimAtPoint.position - projectileSpawnPoint.position);
            
            var projectileInstance = 
                Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation);
            
            NetworkServer.Spawn(projectileInstance, connectionToClient);
            
            lastFireTime = Time.time;
        }
    }
    
    [Server]
    private bool CanFireAtTarget()
    {
        return (target.Targetable.transform.position - transform.position).sqrMagnitude 
               <= fireRange * fireRange;
    }
    
    #endregion Server

    #region Client



    #endregion Client
}
