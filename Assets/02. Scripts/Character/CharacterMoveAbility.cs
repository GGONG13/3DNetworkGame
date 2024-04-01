using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
// 유니티 안에만 있는 특별한 기능,
// 캐릭터 컨트롤러가 달려있어야만 이 해당 스크립트가 실행 됨
public class CharacterMoveAbility : CharacterAbility
{
    // 목표: W,A,S,D 및 방향키를 누르면 캐릭터를 그 방향으로 이동시키고 싶다.

    private float _gravity = -10;
    private float _yVelocity;
    private CharacterController _characterController;
    private Animator _animator;
    private Character _character;
    public float JumpPower = 3f;

    public bool IsJumping => !_characterController.isGrounded;
    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _character = GetComponent<Character>();
    }
    private void Update()
    {
        if (Owner.State == State.Death || !Owner.Photonview.IsMine)
        {
            return;
        }
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            Vector3 dir = new Vector3(h, 0, v);
            dir.Normalize();
            dir = Camera.main.transform.TransformDirection(dir);
            _animator.SetFloat("Move", dir.magnitude);
            _yVelocity += _gravity * Time.deltaTime;

            dir.y = _yVelocity;

            float Speed = Owner.stat.MoveSpeed;
            if (Input.GetKey(KeyCode.LeftShift) && Owner.stat.Stamina > 1)
            {
                Speed = Owner.stat.RunSpeed;
                Owner.stat.Stamina -= Owner.stat.RunConsumeStamina * Time.deltaTime;
                if (Owner.stat.Stamina <= 0)
                {
                    Owner.stat.Stamina = 0;
                }
            }
            else
            {
                Speed = Owner.stat.MoveSpeed;
                Owner.stat.Stamina += Owner.stat.RecoveryStamina * Time.deltaTime;
                Owner.stat.Stamina = Mathf.Min(Owner.stat.Stamina, Owner.stat.MaxStamina);
            }
        bool haveJumpStamina = Owner.stat.Stamina >= Owner.stat.JumpConsumStamina;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Owner.stat.Stamina -= 20;
            _yVelocity = JumpPower;
          //  _animator.SetTrigger("Jump");
            if (Owner.stat.Stamina <= 0)
            {
                _yVelocity = -10;
                _animator.SetFloat("Move", 0);
            }
        }
        if (_characterController.isGrounded && _yVelocity < 0)
        {
            _yVelocity = 0;
        }

        if (IsJumping && Input.GetMouseButtonDown(0) && !_characterController.isGrounded) 
        {
            _animator.SetTrigger("Attack4");
        }
        _characterController.Move(dir * Speed * Time.deltaTime);


    }
    public void Teleport(Vector3 position)
    {
        _characterController.enabled = false;

        transform.position = position;

        _characterController.enabled = true;
    }
}
