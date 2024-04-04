using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
public enum MonsterState
{
    Idle,
    Patrol,
    Trace,
    Attack,
    Return,
    Damaged,
    Die
}
public class Monster : MonoBehaviour
{
    private MonsterState _state = MonsterState.Idle;
    private float _idleDistant = 5;
    private float _attackDistant = 3;
    private float _coolTimer = 1;
    private float _currentTime;
    private float _speed = 5;
    private int _health = 100;
    private Transform _target;
    private Animator _animator;
    private CharacterController _characterController;
    void Start()
    {
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach (var view in views)
        {
            if (view.IsMine && view.CompareTag("Player"))
            {
                _target = view.gameObject.transform;
                break;
            }
        }
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        FindClosestPlayer();
        switch (_state)
        {
            case MonsterState.Idle:
            {
                Idle();
                break;
            }
            case MonsterState.Patrol:
            {
                Patrol();
                break;
            }
            case MonsterState.Trace:
            {
                Trace();
                break;
            }
            case MonsterState.Attack:
            {
                Attack();
                break;
            }
            case MonsterState.Return:
            {
                Return();
                break;
            }
            case MonsterState.Damaged:
            {
                Damaged();
                break;
            }
            case MonsterState.Die:
            {
                Die();
                break;
            }
        }
    }
    void FindClosestPlayer()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (var player in PhotonNetwork.PlayerListOthers)
        {
            GameObject playerGameObject = (player.TagObject as GameObject);
            if (playerGameObject != null)
            {
                float distance = Vector3.Distance(transform.position, playerGameObject.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = playerGameObject.transform;
                }
            }
        }

        _target = closestPlayer;
    }
    void Idle()
    {
        _state = MonsterState.Idle;
        if (Vector3.Distance(transform.position, _target.transform.position) < _idleDistant)
        {
            _state = MonsterState.Patrol;
        }
    }
    void Patrol()
    {

        if (Vector3.Distance(transform.position, _target.transform.position) < _attackDistant)
        {
            // 이동 방향 설정
            Vector3 dir = (_target.position - transform.position).normalized;

            // 캐릭터 콘트롤러를 이용해 이동하기
            _characterController.Move(dir * _speed * Time.deltaTime);
            _state = MonsterState.Attack;
        }

    }
    void Trace()
    {
        
    }
    void Attack()
    {
        _currentTime += Time.deltaTime;
        if (_currentTime >= _coolTimer)
        {

            _currentTime = 0;
        }
    }
    void Return() 
    {

    }
    void Damaged() 
    {

    }
    void Die() 
    {

    }

}
