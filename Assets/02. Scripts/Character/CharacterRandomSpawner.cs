using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRandomSpawner : MonoBehaviour
{
    public static CharacterRandomSpawner Instance { get; private set; }
    public Transform[] SpawnPoints;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public Transform GetRandomSpawnPoint()
    {
        if (SpawnPoints.Length == 0)
        {
            return null;
        }
        int index = Random.Range(0, SpawnPoints.Length);
        return SpawnPoints[index];
    }
}
