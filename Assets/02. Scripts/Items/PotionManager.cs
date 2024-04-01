using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PotionManager : MonoBehaviour
{
    public static PotionManager Instance { get; private set; }
    public List<GameObject> ItemPrefabs;
    private Character _character;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        _character = GetComponent<Character>();
    }

    public void Make(ItemType ItemType, Vector3 Position)
    {
        if (_character.Photonview.IsMine)
        {
            PhotonNetwork.Instantiate(ItemPrefabs[0].name, Position, Quaternion.identity);
            PhotonNetwork.Instantiate(ItemPrefabs[1].name, Position, Quaternion.identity);
        }
    }
}
