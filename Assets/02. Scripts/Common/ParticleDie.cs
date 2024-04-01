using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ParticleDie : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Die_Coroutine());
    }

    IEnumerator Die_Coroutine()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}
