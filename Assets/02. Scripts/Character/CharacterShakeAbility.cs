using System.Collections;
using UnityEngine;

public class CharacterShakeAbility : CharacterAbility
{
    public float Duration = 0.2f;
    public float Strength = 0.1f;
    public Transform TargetTransform;
    public void Shake()
    {
        StopAllCoroutines();
        StartCoroutine(Shake_Coroutine());
    }

    private IEnumerator Shake_Coroutine()
    {
        float elapsedTime = 0f;
        Vector3 startPostion = TargetTransform.localPosition;
        while (elapsedTime <= Duration)
        {
            elapsedTime += Time.deltaTime;
            Vector3 randomPosition = Random.insideUnitSphere.normalized * Strength;
            randomPosition.y = startPostion.y;
            TargetTransform.localPosition += randomPosition;
            
            yield return null;
        }
        TargetTransform.localPosition = startPostion;
    }
}
