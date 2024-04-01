using Photon.Pun;
using System;
using Unity.VisualScripting;
using UnityEngine;
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Collider))]
public class ItemObject : MonoBehaviourPun
{
    [Header("아이템 타입")]
    public ItemType Type;
    public float Value = 100;
/*    private void Start()
    {
        this.gameObject.SetActive(true);
    }*/
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            Character character = other.GetComponent<Character>();
            Debug.Log($"{character}");

            if (!character.Photonview.IsMine || character.State == State.Death)
            {
                return;
            }
            switch (Type) 
            {
                case ItemType.HealthPotion:
                {
                    character.stat.Health += (int)Value;
                    Debug.Log("헬스스탯 올라감?");
                    if (character.stat.Health > character.stat.MaxHealth)
                    {
                        character.stat.Health = character.stat.MaxHealth;
                    }
                    break;
                }
                case ItemType.StaminaPotion: 
                {
                    character.stat.Stamina += (int)Value;
                    Debug.Log("스태미나스탯 올라감?");
                    if (character.stat.Stamina > character.stat.MaxStamina)
                    {
                        character.stat.Stamina = character.stat.MaxStamina;
                    }
                    break;
                }
            }
            gameObject.SetActive(false);
            ItemObjectFactory.Instance.RequestDelete(photonView.ViewID);
        }
    }
}
