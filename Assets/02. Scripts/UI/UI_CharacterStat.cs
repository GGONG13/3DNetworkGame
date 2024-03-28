using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterStat : MonoBehaviour
{
    public static UI_CharacterStat instance { get; private set; }
    public Slider HealthUI;
    public Slider StaminaUI;
    public Character MyCharacter;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Update()
    {
        if (MyCharacter == null)
        {
            return;
        }

        HealthRefresh();
        StaminaRefresh();
    }

    public void HealthRefresh()
    {
        HealthUI.value = (float)MyCharacter.stat.Health / (float)MyCharacter.stat.MaxHealth;
    }
    public void StaminaRefresh()
    {
        StaminaUI.value = MyCharacter.stat.Stamina / MyCharacter.stat.MaxStamina;
    }
}
