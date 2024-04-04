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
    private void Start()
    {
        if (photonView.IsMine)
        {
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            Vector3 randomVector = UnityEngine.Random.insideUnitSphere;
            randomVector.y = 1f;
            randomVector.Normalize();
            randomVector *= UnityEngine.Random.Range(3, 7f);
            rigidbody.AddForce(randomVector, ForceMode.Impulse);
            rigidbody.AddTorque(randomVector, ForceMode.Impulse);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            Character character = other.GetComponent<Character>();
            if (!character.Photonview.IsMine || character.State == State.Death)
            {
                return;
            }
            switch (Type) 
            {
                case ItemType.HealthPotion:
                {
                    character.stat.Health += (int)Value;
                    if (character.stat.Health > character.stat.MaxHealth)
                    {
                        character.stat.Health = character.stat.MaxHealth;
                    }
                    break;
                }
                case ItemType.StaminaPotion: 
                {
                    character.stat.Stamina += (int)Value;
                    if (character.stat.Stamina > character.stat.MaxStamina)
                    {
                        character.stat.Stamina = character.stat.MaxStamina;
                    }
                    break;
                }
                case ItemType.ScoreStone:
                {
                    // character.Score += (int)Value;
                    character.AddPropertyIntValue("Score", (int)Value);
                    break;
                }
            }
            gameObject.SetActive(false);
            ItemObjectFactory.Instance.RequestDelete(photonView.ViewID);
        }
    }
}
