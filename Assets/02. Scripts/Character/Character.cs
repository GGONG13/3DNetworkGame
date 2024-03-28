using Cinemachine;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterMoveAbility))]
[RequireComponent(typeof(CharacterRotateAbility))]
[RequireComponent(typeof(CharacterAttackAbility))]
[RequireComponent(typeof(CharacterShakeAbility))]
public class Character : MonoBehaviour, IPunObservable, IDamaged
{
    public Stat stat;
    public PhotonView Photonview { get; private set; }
    private Vector3 _receivedPosition;
    private Quaternion _receivedRotaion;
    private void Awake()
    {
        stat.Init();
        Photonview = GetComponent<PhotonView>();
        if (Photonview.IsMine)
        {
            UI_CharacterStat.instance.MyCharacter = this;
            CharacterMinimap.instance.MyCharacter = this;
        }
    }

    private void Update()
    {
        if (!Photonview.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, _receivedPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, _receivedRotaion, Time.deltaTime * 10f);
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
            stream.SendNext(stat.Health);
            stream.SendNext(stat.Stamina);
        }
        else if (stream.IsReading) // 데이터를 수신하는 상황
        {
            _receivedPosition = (Vector3)stream.ReceiveNext();
            _receivedRotaion = (Quaternion)stream.ReceiveNext();
            // 데이터를 전송한 순서와 똑같이 받은 데이터를 캐스팅(형변환)해야 한다.
            if (!Photonview.IsMine)
            {
                stat.Health = (int)stream.ReceiveNext();
                stat.Stamina = (float)stream.ReceiveNext();
            }
        }
        // info는 송수신 성공/실패 여부에 대한 메세지가 담겨있다.
    }
    [PunRPC]
    public void Damaged (int damage)
    {
        GetComponent<CharacterShakeAbility>().Shake();
        if (Photonview.IsMine)
        {
            stat.Health -= damage;
            UI_DamagedEffect.Instance.Show(0.5f);
            if (TryGetComponent<CinemachineImpulseSource>(out CinemachineImpulseSource cinemachineImpulseSource))
            {
                float strength = 0.2f;
                cinemachineImpulseSource.GenerateImpulseWithVelocity(UnityEngine.Random.insideUnitSphere.normalized * strength);
            }
        }
    }

}

