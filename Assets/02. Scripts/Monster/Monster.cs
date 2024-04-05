using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.AI;
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
    private float _idleDistant = 5;
    private float _attackDistant = 2;
    private float _coolTimer = 1;
    public float _currentTime;
    public float _waitTime;
    private float _speed = 5;
    private int _health = 100;
    private Transform _target;
    private Vector3 _idlePosition;
    public Transform PatrolTrans;
    private Animator _animator;
    private CharacterController _characterController;
    private NavMeshAgent _navMeshAgent;
    public PhotonView Photonview { get; private set; }
 /*   void Start()
    {
        Photonview = GetComponent<PhotonView>();
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
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _speed;   
        Vector3 randomVector = UnityEngine.Random.insideUnitSphere;
        _patrolTrans = randomVector;
        _idlePosition = transform.position;
    }*/
    IEnumerator Start()
    {
        _state = MonsterState.Idle;
        yield return new WaitForSeconds(0.5f); // 0.5초마다 체크
        InitializeMonster();
    }
    void InitializeMonster() 
    {
        Debug.Log("실행?");
         Photonview = GetComponent<PhotonView>();
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _speed;
        _idlePosition = transform.position;
        LocalPlayerFind();
       // FindClosestPlayer(); // 게임 시작 시 바로 가장 가까운 플레이어 찾기
    }

    void Update()
    {
        if (_target != null && _state == MonsterState.Patrol || _state == MonsterState.Idle)
        {
          //  FindClosestPlayer();
        }
        LocalPlayerFind();
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
                Damaged(10, Photonview.ControllerActorNr);
                break;
            }
            case MonsterState.Die:
            {
                Die();
                break;
            }
        }
    }

    void LocalPlayerFind()
    {

        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        // 주변의 모든 콜라이더를 검사 범위와 레이어 마스크를 이용하여 검색합니다.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 100f, LayerMask.GetMask("Player"));

        foreach (var hitCollider in hitColliders)
        {
            // 각 콜라이더에 대해 CharacterController 컴포넌트를 찾습니다.
            CharacterController characterController = hitCollider.GetComponent<CharacterController>();

            // CharacterController 컴포넌트가 있는 경우, 거리를 계산합니다.
            if (characterController != null)
            {
                float distance = Vector3.Distance(transform.position, characterController.transform.position);

                // 가장 가까운 플레이어를 찾습니다.
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = characterController.transform;
                }
            }
        }

        // 가장 가까운 플레이어를 _target으로 설정합니다.
        _target = closestPlayer;

        // 디버그 로그로 결과를 출력합니다.
        if (_target != null)
        {
            Debug.Log($"Closest player found: {_target.name} at distance {closestDistance}");
        }
        else
        {
            Debug.Log("No player found.");
        }
    }

    void FindClosestPlayer()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;
        // 모든 콜라이더를 찾습니다.
        Collider[] colliders = Physics.OverlapSphere(transform.position, 100f, LayerMask.GetMask("Player"));

        foreach (Collider collider in colliders)
        {
            // 각 콜라이더에서 CharacterController를 찾습니다.
            CharacterController characterController = collider.GetComponent<CharacterController>();
            // CharacterController가 있고, 그것이 PhotonView를 가지며, 그것이 다른 플레이어의 것인지 확인합니다.
            if (characterController != null)
            {
                PhotonView view = characterController.GetComponent<PhotonView>();
                if (view != null && !view.IsMine)
                {
                    float distance = Vector3.Distance(transform.position, characterController.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlayer = characterController.transform;
                    }
                }
            }
        }

        if (_target != closestPlayer)
        {
            _target = closestPlayer;
            // 새 타겟을 찾았을 때만 목적지를 재설정
            _navMeshAgent.SetDestination(_target.position);
        }
        Debug.Log($"Found player: {_target?.name}");
    }

    void Idle()
    {
       // _state = MonsterState.Idle;
       // _animator.SetBool("Idle", true);
       // _animator.SetTrigger("Idle");
        _waitTime += Time.deltaTime;
        if (_waitTime > 3)
        {
            _state = MonsterState.Patrol;
            _waitTime = 0;
        }
        if (_target != null && Vector3.Distance(transform.position, _target.position) <= _idleDistant)
        {
            _state = MonsterState.Trace;
            _waitTime = 0;
        }
    }
    void Patrol()
    {
        _navMeshAgent.stoppingDistance = 0;
        if (_navMeshAgent.destination != PatrolTrans.position)
        {
            _navMeshAgent.SetDestination(PatrolTrans.position);
        }
        _animator.SetTrigger("Walk");
        if (_target != null && Vector3.Distance(transform.position, _target.position) < 10)
        {
            _state = MonsterState.Attack;
        }
        if (Vector3.Distance(PatrolTrans.position, transform.position) > 40)
        {
            _state = MonsterState.Return;
        }
    }
    void Trace()
    {
        _navMeshAgent.stoppingDistance = _attackDistant;
        _animator.SetTrigger("Run");
        Vector3 dir = (_target.position - transform.position).normalized;
        _navMeshAgent.destination = dir;
        if (Vector3.Distance(transform.position, _target.transform.position) <= _attackDistant)
        {
            _state = MonsterState.Attack;
        }
        if (Vector3.Distance(PatrolTrans.position, transform.position) > 40)
        {
            _state = MonsterState.Return;
        }
    }
    void Attack()
    {
        _currentTime += Time.deltaTime;
        if (_currentTime >= _coolTimer)
        {
            _animator.SetTrigger("Attack");
           // Photonview.RPC(nameof(MonsterAttackAni), RpcTarget.All);
            _currentTime = 0;
        }
        if (Vector3.Distance(transform.position, _target.transform.position) > _attackDistant)
        {
            _state = MonsterState.Trace;
        }
    }
    [PunRPC]
    public void MonsterAttackAni(int index)
    {
        _animator.SetTrigger($"Attack{index}");
    }
    void Return() 
    {
        _animator.SetTrigger("Walk");
        _navMeshAgent.stoppingDistance = 2;
        _navMeshAgent.destination = _idlePosition;
    }

    void MonsterDamaged()
    {
        
    }
    [PunRPC]
    public void Damaged(int damage, int actorNumber)
    {
        _health -= damage;
        _animator.SetTrigger("Damaged");
        if (_health <= 0)
        {
            _state = MonsterState.Die;
        }
        if (_state == MonsterState.Die)
        {
            return;
        }
    }
   // [PunRPC]
/*    public void Damaged(int damage, int actorNumber)
    {
        if (_state == MonsterState.Die)
        {
            return;
        }
        _health -= damage;
      //  _animator.SetTrigger("Get Hit Front");
        if (_health <= 0)
        {
            _state = MonsterState.Die;
            Photonview.RPC(nameof(Die), RpcTarget.All);
        }
    }*/
    [PunRPC]
    void Die() 
    {
        StartCoroutine(Death_Coroutine());
    }
    IEnumerator Death_Coroutine()
    {
        _animator.SetTrigger("Death");
        yield return new WaitForSeconds(2);
        this.gameObject.SetActive(false);
    }
}
