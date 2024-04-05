using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
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
public class Monster : MonoBehaviour, IDamaged
{
    private MonsterState _state = MonsterState.Idle;
    public Stat Stat;
    private Character _target;
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private List<Character> _characterList = new List<Character>();
    public SphereCollider CharacterDetectCollider;
    // idle
    public float _waitTime;
    public float IdleMaxTime = 5;
    // patrol
    private Transform PatrolTrans;
    public float TraceDetectRange = 5f;
    // return
    private Vector3 _idlePosition;
    // attack
    private float _attackDistant = 3f;
    private float _coolTimer = 1;
    public float _currentTime;

    private float _idleDistant = 5;
    void Start()
    {
        _state = MonsterState.Idle;
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _idlePosition = transform.position;
        CharacterDetectCollider.radius = TraceDetectRange;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return; // 조기반환문
        }
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
               // Damaged(10, Photonview.ControllerActorNr);
                break;
            }
            case MonsterState.Die:
            {
                Die();
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Character character = other.GetComponent<Character>();
            if (!_characterList.Contains(character))
            {
                _characterList.Add(character);
                Debug.Log("새로운 인간 발견 후후");
            }

        }
    }
    // 나와의 거리가 distance보다 짧은 플레이어를 반환
    private Character FindTarget(float distance)
    {
        if (_target != null && _target.State == State.Death)
        {
            return null;
        }
        _characterList.RemoveAll(c => c == null || c.State == State.Death);
        Vector3 myPotision = transform.position;
        foreach (Character character in _characterList)
        {
            if (Vector3.Distance(character.transform.position, myPotision) <= distance)
            {
                return character;
            }
        }
        return null;
    }
    void Idle()
    {
       // _state = MonsterState.Idle;
       // _animator.SetBool("Idle", true);
       // _animator.SetTrigger("Idle");

        // 가만히 있다가
        // [대기 시간]이 흘러가면 "정찰 상태로 전이"
        _waitTime += Time.deltaTime;
        if (_waitTime >= IdleMaxTime)
        {
            _state = MonsterState.Patrol;
            Debug.Log("Idle -> Patrol");
            _waitTime = 0;
        }
        // 플레이어가 [감지범위] 안에 들어오면 플레이어 "추적상태로 전이"
        _target = FindTarget(TraceDetectRange);
        if (_target != null)
        {
            _state = MonsterState.Trace;
            Debug.Log("Idle -> Trace");
        }
    }
    void Patrol()
    {
        if (PatrolTrans == null)
        {
            PatrolTrans = GameObject.Find("Patrol").transform;
        }
        // [패트롤 구역]까지 간다
        // todo: 네비게이션으로 이동
        _navMeshAgent.stoppingDistance = 0;
        if (_navMeshAgent.destination != PatrolTrans.position)
        {
            _navMeshAgent.SetDestination(PatrolTrans.position);
        }
        RequestPlayAnimation("Walk");
        // IF [플레이어]가 [감지범위] 안에 들어오면 플레이어 "추적상태로 전이"
        _target = FindTarget(TraceDetectRange);
        if (_target != null)
        {
            _state = MonsterState.Trace;
            Debug.Log("Patrol -> Trace");
        }
        // IF [패트롤 구역]에 도착하면 "복귀상태로 전이"
        if (Vector3.Distance(PatrolTrans.position, transform.position) <= 0.1f)
        {
            Debug.Log("Patrol -> Return");
            _state = MonsterState.Return;
        }
    }
    void Trace()
    {
        if (_target == null)
        {
            _state = MonsterState.Return;
            return;
        }
        _navMeshAgent.stoppingDistance = _attackDistant;
        RequestPlayAnimation("Run");
        // 플레이어가 죽거나 너무 멀어지면 복귀
        _navMeshAgent.destination = _target.transform.position; 
        if (_target.State == State.Death || GetDistance(_target.transform) > TraceDetectRange) 
        {
            _state = MonsterState.Return;
            Debug.Log("Trace -> Return");
            return;
        }
        if (GetDistance(_target.transform) <= _attackDistant)
        {
            Debug.Log("Trace -> Attack");
            _state = MonsterState.Attack;
        }
        if (Vector3.Distance(PatrolTrans.position, transform.position) > 40)
        {
            Debug.Log("Trace -> Return");
            _state = MonsterState.Return;
        }
    }
    void Attack()
    {
        _navMeshAgent.ResetPath();
        _navMeshAgent.isStopped = true;
        
        _currentTime += Time.deltaTime;
        if (_currentTime >= _coolTimer)
        {
            RequestPlayAnimation("Attack");
            _currentTime = 0;
        }
        _navMeshAgent.destination = _target.transform.position;
        if (_target.State == State.Death || GetDistance(_target.transform) > _attackDistant)
        {
            _state = MonsterState.Idle;
            _idlePosition = transform.position;
            _navMeshAgent.isStopped = false;
            Debug.Log("Attack -> Idle");
            return;
        }
        if (_target == null)
        {
            _state = MonsterState.Idle;
            _idlePosition = transform.position;
            _navMeshAgent.isStopped = false;
            Debug.Log("Attack -> Idle");
            return;
        }
    }
    [PunRPC]
    public void MonsterAttackAni(int index)
    {
        _animator.SetTrigger($"Attack{index}");
    }
    void Return() 
    {
        // [시작 위치]로 간다
        RequestPlayAnimation("Walk");
        _navMeshAgent.stoppingDistance = 0;
        _navMeshAgent.destination = _idlePosition;

        if (!_navMeshAgent.pathPending || _navMeshAgent.remainingDistance <= 0.1f)
        {
            Debug.Log("Retrun -> Idle");
            _state = MonsterState.Idle;
            RequestPlayAnimation("Idle");
        }
        _target = FindTarget(TraceDetectRange);
        if (_target != null)
        {
            _state = MonsterState.Trace;
            Debug.Log("Return -> Trace");
        }
    }

    [PunRPC]
    public void Damaged(int damage, int actorNumber)
    {
        Stat.Health -= damage;
        RequestPlayAnimation("Damaged");
        if (Stat.Health <= 0)
        {
            _state = MonsterState.Die;
        }
        if (_state == MonsterState.Die)
        {
            return;
        }
    }
    [PunRPC]
    void Die() 
    {
        StartCoroutine(Death_Coroutine());
    }
    IEnumerator Death_Coroutine()
    {
        RequestPlayAnimation("Death");
        yield return new WaitForSeconds(2);
        this.gameObject.SetActive(false);
    }

    private float GetDistance (Transform otherTransform) 
    {
        return Vector3.Distance (transform.position, otherTransform.position);
    }

    public void AttackAction()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        Debug.Log("AttackAction!");
        // 일정 범위 안에 있는 모든 플레이어에게 데미지를 주고 싶다.
        List<Character> targets = FindTargets(_attackDistant + 0.2f);
        foreach (Character target in targets) 
        {
            Vector3 dir = (target.transform.position - transform.position).normalized;
            int viewAngle = 160 / 2;
            if (Vector3.Angle(transform.forward, dir) < viewAngle)
            {
                target.Photonview.RPC("Damaged", RpcTarget.All, Stat.Damage, -1);
            }
        }
    }
    private List<Character> FindTargets(float distance)
    {
        _characterList.RemoveAll(c => c == null);
        List<Character> characters = new List<Character>();
        Vector3 myPotision = transform.position;
        foreach (Character character in _characterList)
        {
            if (Vector3.Distance(character.transform.position, myPotision) <= distance)
            {
                characters.Add(character);
            }
        }
        return null;
    }
    private void PlayAnimation(string animationName)
    {
        GetComponent<PhotonView>().RPC(nameof(PlayAnimation), RpcTarget.All, animationName);
    }
    [PunRPC]
    private void RequestPlayAnimation(string animationName)
    {
        _animator.Play(animationName);
    }
}
