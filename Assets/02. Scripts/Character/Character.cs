using Cinemachine;
using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public enum CharacterState
{
    Live,
    Death
}
[RequireComponent(typeof(CharacterMoveAbility))]
[RequireComponent(typeof(CharacterRotateAbility))]
[RequireComponent(typeof(CharacterAttackAbility))]
[RequireComponent(typeof(CharacterShakeAbility))]
public class Character : MonoBehaviour, IPunObservable, IDamaged
{
    public Stat stat;
    public CharacterState currentState = CharacterState.Live;
    public PhotonView Photonview { get; private set; }
    private Vector3 _receivedPosition;
    private Quaternion _receivedRotaion;
    private Animator _animator;
    private bool _isRespwning = false;
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
            stream.SendNext(currentState == CharacterState.Death);
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
            bool isDead = (bool)stream.ReceiveNext(); // 죽음 상태 수신
            currentState = isDead ? CharacterState.Death : CharacterState.Live;
        }
        // info는 송수신 성공/실패 여부에 대한 메세지가 담겨있다.
    }
    [PunRPC]
    public void Damaged (int damage)
    {
        if (currentState == CharacterState.Death)
        {
            return; 
        }
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
        if (stat.Health <= 0)
        {
            stat.Health = 0;
            Debug.Log("죽어라");
            _animator.SetTrigger("Death");
            currentState = CharacterState.Death;
            Photonview.RPC("Die", RpcTarget.All);
            StartCoroutine(Respawn_Coroutine());
        }
    }
    [PunRPC]
    public void Die()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("Death");
            currentState = CharacterState.Death;
        }
    }
    IEnumerator Respawn_Coroutine()
    {
        yield return new WaitForSeconds(5);
        Photonview.RPC("Respawner", RpcTarget.All);
      //  int index = UnityEngine.Random.Range(0, CharacterRandomSpawner.Instance.SpawnPoints.Length);
    }
    [PunRPC]
    public void Respawner()
    {
        _animator.SetTrigger("Getup");
        if (Photonview.IsMine)
        {
            CharacterRandomSpawner spawnManager = FindObjectOfType<CharacterRandomSpawner>();
            if (spawnManager.SpawnPoints.Length > 0)
            {
                int number = UnityEngine.Random.Range(0, spawnManager.SpawnPoints.Length);
                Vector3 spawnPosition = spawnManager.SpawnPoints[number].position;
                this.transform.position = spawnPosition;
            }
            stat.Health = stat.MaxHealth;
            stat.Stamina = stat.MaxStamina;
            currentState = CharacterState.Live;
        }
        _animator.SetTrigger("Idle");
    }    
}

