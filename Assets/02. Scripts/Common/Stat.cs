using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stat
{
    public int Health;
    public int MaxHealth;

    public float Stamina;
    public float MaxStamina = 100;
    public float RunConsumeStamina = 10;
    public float RecoveryStamina = 5;

    public float MoveSpeed;
    public float RunSpeed;

    public float RotationSpeed;

    public float AttactCoolTime;
    public float AttackConsumeStamina = 20;

    public void Init()
    {
        Health = MaxHealth;
        Stamina = MaxStamina;
    }
}
