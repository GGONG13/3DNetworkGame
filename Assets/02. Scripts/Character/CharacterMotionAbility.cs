using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class CharacterMotionAbility : CharacterAbility
{
    private void Update()
    {
        if (Owner.State == State.Death || !Owner.Photonview.IsMine)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Owner.Photonview.RPC(nameof(PlayMotion), RpcTarget.All, 1);
        }
    }

    [PunRPC]
    private void PlayMotion(int number)
    {
        GetComponent<Animator>().SetTrigger($"Motion{number}");
    }
}