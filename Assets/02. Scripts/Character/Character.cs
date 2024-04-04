using Cinemachine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(CharacterMoveAbility))]
[RequireComponent(typeof(CharacterRotateAbility))]
[RequireComponent(typeof(CharacterAttackAbility))]
[RequireComponent(typeof(CharacterShakeAbility))]
public class Character : MonoBehaviour, IPunObservable, IDamaged
{
    public Stat stat;
   // public GameObject Weapon;
    public State State { get; private set; } = State.Live;
    public PhotonView Photonview { get; private set; }
    private Vector3 _receivedPosition;
    private Quaternion _receivedRotaion;
    private Animator _animator;
    private int _halfScore;
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
        if (!Photonview.IsMine)
        {
            return;
        }

        SetRandomPositionAndRotation();
        
        ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
        hashtable.Add("Score", 0);
        hashtable.Add("KillCount", 0);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
    }
    [PunRPC]
    public void AddPropertyIntValue(string key, int value)
    {
        ExitGames.Client.Photon.Hashtable MyHashtable = PhotonNetwork.LocalPlayer.CustomProperties;
        int currentValue = MyHashtable.ContainsKey(key) ? (int)MyHashtable[key] : 0;
        MyHashtable[key] = currentValue + value;
        PhotonNetwork.LocalPlayer.SetCustomProperties(MyHashtable);
    }
    [PunRPC]
    public void SetPropertyIntValue(string key, int value)
    {
        ExitGames.Client.Photon.Hashtable MyHashtable = PhotonNetwork.LocalPlayer.CustomProperties;
        MyHashtable[key] = value;
        PhotonNetwork.LocalPlayer.SetCustomProperties(MyHashtable);
        GetComponent<CharacterAttackAbility>().RefrechWeaponScale();
    }
    [PunRPC]
    public int GetPropertyIntValue(string key)
    {
        ExitGames.Client.Photon.Hashtable MyHashtable = PhotonNetwork.LocalPlayer.CustomProperties;
        return (int)MyHashtable[key];
    }

    [PunRPC]
    private void IncreaseKillCount(PhotonView killerPhotonView)
    {
        int currentKillCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["KillCount"];
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash["KillCount"] = currentKillCount + 1;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
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
          //  PhotonView killerPhotonView = PhotonView.Find(actorNumber);
            if (Photonview.IsMine)
            {
                OnDeath(actorNumber);
            }
            Photonview.RPC(nameof(Death), RpcTarget.All);
            //IncreaseKillCount(killerPhotonView);
/*            if (killerPhotonView != null && killerPhotonView.IsMine)
            {
                int victimScore = (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"];
                int scoreToGain = victimScore / 2;

                AddScore(scoreToGain);

                ScatterScoreObjects(victimScore - scoreToGain, transform.position);

                ExitGames.Client.Photon.Hashtable victimProps = new ExitGames.Client.Photon.Hashtable { { "Score", scoreToGain } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(victimProps);
            }*/

        }
        if (Photonview.IsMine)
        {
   
            OnDamagedMine();
        }
    }
    [PunRPC]
    void ScatterScoreObjects(int scoreAmount, Vector3 position)
    {
        int objectsToCreate = scoreAmount / 10; // 생성할 오브젝트 수, 예를 들어 점수 1당 오브젝트 1개로 가정

        for (int i = 0; i < objectsToCreate; i++)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 5; // 무작위 방향
            Vector3 spawnPosition = position + randomDirection;
            PhotonNetwork.Instantiate(ItemType.ScoreStone.ToString(), spawnPosition, Quaternion.identity);
        }
    }

    private void OnDeath(int actorNumber)
    {
        if (!Photonview.IsMine)
        {
            return;
        }
        int _halfScore = GetPropertyIntValue("Score");
        SetPropertyIntValue("Score", 0);
        ScatterScoreObjects(_halfScore, transform.position);
        if (actorNumber >= 0)
        {
            string nickname = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber).NickName;
            string logMessage = $"\n{nickname}님이 {Photonview.Owner.NickName}을 처치하였습니다.";
            Photonview.RPC(nameof(AddLog), RpcTarget.All, logMessage);

            Player targetPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            Photonview.RPC(nameof(AddPropertyIntValue), targetPlayer, "Score", _halfScore);
            Photonview.RPC(nameof(AddPropertyIntValue), targetPlayer, "KillCount", 1);
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
    public int Score = 0;
    [PunRPC]
    private void Death()
    {
        State = State.Death;

        GetComponent<Animator>().SetTrigger("Death");
        GetComponent<CharacterAttackAbility>().InactiveCollider();

        if (Photonview.IsMine)
        {
            DropItems();

            StartCoroutine(Death_Coroutine());
        } 
    }

    private void DropItems()
    {/*- 70%: Player 스크립트에 점수가 있고 먹으면 점수가 1점씩 오른다. (3~5개 랜덤 생성)
            - (score 변수는 일단 Character에 생성)
        - 20%: 먹으면 체력이 꽉차는 아이템 1개
            - 10%: 먹으면 스태미나 꽉차는 아이템 1개*/
        // 팩토리패턴: 객체 생성과 사용 로직을 분리해서 캡슐화하는 패턴
        int randomValue = UnityEngine.Random.Range(0, 100);
        if (randomValue > 30)      // 70%
        {
            int randomCount = _halfScore / 100;
            for (int i = 0; i < randomCount; ++i)
            {
                ItemObjectFactory.Instance.RequestCreate(ItemType.ScoreStone, transform.position);
            }
        }
        else if (randomValue > 10) // 20%
        {
            ItemObjectFactory.Instance.RequestCreate(ItemType.HealthPotion, transform.position);
        }
        else                       // 10%
        {
            ItemObjectFactory.Instance.RequestCreate(ItemType.StaminaPotion, transform.position);
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

