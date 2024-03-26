using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMinimap : MonoBehaviour
{
    private Transform target;
    private float YDistance = 10f;

    void Start()
    {
        FindLocalPlayer();
    }

    void FindLocalPlayer()
    {
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        foreach (PhotonView view in photonViews)
        {
            if (view.IsMine)
            {
                target = view.transform;
                break;
            }
        }
    }

    void Update()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position;
            targetPosition.y = YDistance;
            transform.position = targetPosition;
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            FindLocalPlayer(); // 타겟을 다시 찾음
        }
    }
}
