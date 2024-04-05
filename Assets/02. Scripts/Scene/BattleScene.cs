using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
public class BattleScene : MonoBehaviourPunCallbacks
{
    public static BattleScene Instance { get; private set; }

    public List<Transform> SpawnPoints;
    

    private void Awake()
    {
        Instance = this;
    }
    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        GameObject[] points = GameObject.FindGameObjectsWithTag("BearSpawnPoint");
        foreach (GameObject point in points) 
        {
            PhotonNetwork.InstantiateRoomObject("Bear_4", point.transform.position, point.transform.rotation);
        }
    }

    public Vector3 GetRandomSpawnPoint()
    {
        int randomIndex = UnityEngine.Random.Range(0, SpawnPoints.Count);
        return SpawnPoints[randomIndex].position;
    }
}
