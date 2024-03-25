using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
// 유니티 안에만 있는 특별한 기능,
// 캐릭터 컨트롤러가 달려있어야만 이 해당 스크립트가 실행 됨
public class CharacterMoveAbility : MonoBehaviour
{
    // 목표: W,A,S,D 및 방향키를 누르면 캐릭터를 그 방향으로 이동시키고 싶다.

    private float _moveSpeed = 5;
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
        // 순서
        // 1. 사용자의 키보드 입력을 받는다
        // 2. '캐릭터가 바라보는 방향'을 기준으로 방향을 설정한다.
        // 3. 이동속도에 따라 그 방향으로 이동한다.

        // 4. 중력 적용

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3 (h, 0, v);
        dir.Normalize();
        dir = Camera.main.transform.TransformDirection(dir);
        _animator.SetFloat("Move", dir.magnitude);
        _yVelocity += _gravity * Time.deltaTime;

        dir.y = _yVelocity;

        _characterController.Move(dir * _moveSpeed * Time.deltaTime);


    }
}
