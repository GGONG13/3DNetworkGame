using UnityEngine;

public class Weapon : MonoBehaviour
{
    public CharacterAttackAbility MyCharacterAttackAbiliy;

    private void OnTriggerEnter(Collider other)
    {
        MyCharacterAttackAbiliy.OnTriggerEnter(other);   
    }
}
