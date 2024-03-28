using UnityEngine;

public abstract class CharacterAbility : MonoBehaviour
{
    protected Character Owner { get; private set; }
    protected void Awake()
    {
        Owner = GetComponentInParent<Character>();
    }
}
