using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCanvasAbility : CharacterAbility
{
    public Text NicknameTextUI;
    public Canvas MyCanvas;
    public Slider HealthSlider;
    public Slider StaminaSlider;

   
    void Start()
    {
        NicknameTextUI.text = Owner.Photonview.Controller.NickName;
    }

    void Update()
    {
        transform.forward = Camera.main.transform.forward;

        HealthSlider.value = (float)Owner.stat.Health / Owner.stat.MaxHealth;
        StaminaSlider.value = Owner.stat.Stamina / Owner.stat.MaxStamina;
    }
}
