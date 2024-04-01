using Photon.Pun;
using UnityEngine;

public class ItemObjectFactory : MonoBehaviourPun
{
    public static ItemObjectFactory Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public void RequestCreate(ItemType itemType, Vector3 position)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Create(itemType, position);
        }
        else
        {
            photonView.RPC(nameof(Create), RpcTarget.MasterClient, itemType, position);    
        }
    }
    [PunRPC]
    private void Create(ItemType itemType, Vector3 position)
    {
        Vector3 dropPos = position + new Vector3(0, 0.5f, 0) + UnityEngine.Random.insideUnitSphere;
        PhotonNetwork.Instantiate(itemType.ToString(), dropPos, Quaternion.identity);
    }
    public void RequestDelete(int viewID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Delete(viewID);
        }
        else
        {
            photonView.RPC(nameof(Delete), RpcTarget.MasterClient, viewID);
        }
    }
    [PunRPC]
    private void Delete (int viewID)
    {
        GameObject objectToDelete = PhotonView.Find(viewID)?.gameObject;
        if ( objectToDelete != null)
        {
            PhotonNetwork.Destroy(objectToDelete);
        }
    }
}
