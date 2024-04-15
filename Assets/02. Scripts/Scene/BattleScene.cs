using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
public class BattleScene : MonoBehaviourPunCallbacks
{
    public static BattleScene Instance { get; private set; }

    public List<Transform> SpawnPoints;

    private bool _init = false;

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetRandomSpawnPoint()
    {
        int randomIndex = UnityEngine.Random.Range(0, SpawnPoints.Count);
        return SpawnPoints[randomIndex].position;
    }

    private void Start()
    {
        if (!_init)
        {
            Init();
        }
    }

    public override void OnJoinedRoom()
    {
        if (!_init)
        {
            Init();
        }
    }



    public void Init()
    {
        _init = true;

        // 캐릭터여자
        // 캐릭터남자
        PhotonNetwork.Instantiate($"Character_{UI_Lobby.SelectedCharacterType}", Vector3.zero, Quaternion.identity);
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        GameObject[] points = GameObject.FindGameObjectsWithTag("BearSpawnPoint");
        foreach (GameObject point in points)
        {
            PhotonNetwork.InstantiateRoomObject("Bear_4", point.transform.position, Quaternion.identity);
        }
    }



}