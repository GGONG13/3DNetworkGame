using UnityEngine;

public class CharacterMinimap : MonoBehaviour
{
    public static CharacterMinimap instance { get; private set; }
    public Character MyCharacter;



    public float YDistance = 10f;
    private Vector3 _initialEulerAngles;

    //private float _mx = 0;
    //private float _my = 0;

    public float RotationSpeed = 500;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _initialEulerAngles = transform.eulerAngles;
    }
    private void LateUpdate()
    {
        if (MyCharacter == null)
        {
            return;
        }
        Vector3 targetPosition = MyCharacter.transform.position;
        targetPosition.y = YDistance;

        transform.position = targetPosition;

        Vector3 targetEulerAngles = MyCharacter.transform.eulerAngles;
        targetEulerAngles.x = _initialEulerAngles.x;
        targetEulerAngles.z = _initialEulerAngles.z;
        transform.eulerAngles = targetEulerAngles;
    }

}
