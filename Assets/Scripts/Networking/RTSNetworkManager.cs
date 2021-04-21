using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField]
    private GameObject unitSpawnerPrefab;

    [SerializeField]
    private GameOverHandler gameOverHandlerPrefab;

    public override void OnServerAddPlayer(NetworkConnection connection)
    {
        base.OnServerAddPlayer(connection);

        var player = connection.identity.GetComponent<RTSPlayer>();

        player.TeamColor = new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f)
        );

        var unitSpawnerInstance = Instantiate(unitSpawnerPrefab,
            connection.identity.transform.position,
            connection.identity.transform.rotation);

        NetworkServer.Spawn(unitSpawnerInstance, connection);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {
            var gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
        }
    }
}