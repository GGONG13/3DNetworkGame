using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

[RequireComponent(typeof(CharacterMoveAbility))]
[RequireComponent(typeof(CharacterRotateAbility))]
[RequireComponent(typeof(CharacterAttackAbility))]
public class Character : MonoBehaviour, IPunObservable
{
    public Stat stat;
    public PhotonView Photonview { get; private set; }

    private void Awake()
    {
        stat.Init();
        Photonview = GetComponent<PhotonView>();
        if (Photonview.IsMine)
        {
            UI_CharacterStat.instance.MyCharacter = this;
        }
    }

    // 데이터 동기화를 위해 데이터 전송 및 수신 기능을 가진 약속
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // stream은 서버에서 주고 받을 데이터가 담겨 있는 변수
        if (stream.IsWriting)      // 데이터를 전송하는 상황
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);

        }
        else if (stream.IsReading) // 데이터를 수신하는 상황
        {
            // 데이터를 전송한 순서와 똑같이 받은 데이터를 캐스팅(형변환)해야 한다.
            Vector3 receivedPosition   = (Vector3)stream.ReceiveNext();
            Quaternion receivedRotaion = (Quaternion)stream.ReceiveNext();

            if (!Photonview.IsMine) 
            {
                transform.position = receivedPosition;
                transform.rotation = receivedRotaion;
            }
        }
        // info는 송수신 성공/실패 여부에 대한 메세지가 담겨있다.
        Debug.Log(info);
    }
}
