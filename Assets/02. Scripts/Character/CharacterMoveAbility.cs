using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
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

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }
    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3 (h, 0, v);
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
        _characterController.Move(dir * Speed * Time.deltaTime);
    }
}
