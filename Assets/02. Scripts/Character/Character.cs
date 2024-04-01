using Cinemachine;
using Photon.Pun;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterMoveAbility))]
[RequireComponent(typeof(CharacterRotateAbility))]
[RequireComponent(typeof(CharacterAttackAbility))]
[RequireComponent(typeof(CharacterShakeAbility))]
public class Character : MonoBehaviour, IPunObservable, IDamaged
{
    public Stat stat;
    public State State { get; private set; } = State.Live;
    public PhotonView Photonview { get; private set; }
    private Vector3 _receivedPosition;
    private Quaternion _receivedRotaion;
    private Animator _animator;

    private void Awake()
    {
        stat.Init();
        Photonview = GetComponent<PhotonView>();
        _animator = GetComponent<Animator>();
        if (Photonview.IsMine)
        {
            UI_CharacterStat.instance.MyCharacter = this;
            CharacterMinimap.instance.MyCharacter = this;
        }
    }
    private void Start()
    {
        SetRandomPositionAndRotation();
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
    public void AddLog(string logMessage)
    {
        UI_RoomInfo.Instance.AddLog(logMessage);
    }
    [PunRPC]
    public void Damaged(int damage, int actorNumber)
    {
        if (State == State.Death)
        {
            return;
        }
        stat.Health -= damage;
        GetComponent<CharacterShakeAbility>().Shake();
        if (stat.Health <= 0)
        {
            State = State.Death;

            if (Photonview.IsMine)
            {
                OnDeath(actorNumber);
            }

            Photonview.RPC(nameof(Death), RpcTarget.All);
        }
        if (Photonview.IsMine)
        {
   
            OnDamagedMine();
        }
    }
    private void OnDeath(int actorNumber)
    {
        if (actorNumber >= 0)
        {
            string nickname = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber).NickName;
            string logMessage = $"\n{nickname}님이 {Photonview.Owner.NickName}을 처치하였습니다.";
            Photonview.RPC(nameof(AddLog), RpcTarget.All, logMessage);
        }
        else
        {
            string logMessage = $"\n{Photonview.Owner.NickName}이 운명을 다했습니다.";
            Photonview.RPC(nameof(AddLog), RpcTarget.All, logMessage);
        }
    }

    private void OnDamagedMine()
    {
        // 카메라 흔들기 위해 Impulse를 발생시킨다.
        CinemachineImpulseSource impulseSource;
        if (TryGetComponent<CinemachineImpulseSource>(out impulseSource))
        {
            float strength = 0.4f;
            impulseSource.GenerateImpulseWithVelocity(UnityEngine.Random.insideUnitSphere.normalized * strength);
        }

        UI_DamagedEffect.Instance.Show(0.5f);
    }

    [PunRPC]
    private void Death()
    {
        State = State.Death;

        GetComponent<Animator>().SetTrigger("Death");
        GetComponent<CharacterAttackAbility>().InactiveCollider();

        if (Photonview.IsMine)
        {
            ItemObjectFactory.Instance.RequestCreate(ItemType.HealthPotion, transform.position);
            ItemObjectFactory.Instance.RequestCreate(ItemType.StaminaPotion, transform.position);

            StartCoroutine(Death_Coroutine());
        }
    }
 

    private IEnumerator Death_Coroutine()
    {
        yield return new WaitForSeconds(5f);

        SetRandomPositionAndRotation();

        Photonview.RPC(nameof(Live), RpcTarget.All);
    }

    private void SetRandomPositionAndRotation()
    {
        Vector3 spawnPoint = BattleScene.Instance.GetRandomSpawnPoint();
        GetComponent<CharacterMoveAbility>().Teleport(spawnPoint);
        GetComponent<CharacterRotateAbility>().SetRandomRotation();
    }
    [PunRPC]
    private void Live()
    {
        State = State.Live;

        stat.Init();

        GetComponent<Animator>().SetTrigger("Live");
    }
}

