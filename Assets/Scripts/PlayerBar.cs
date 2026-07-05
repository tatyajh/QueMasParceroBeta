using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BarType
{
    healthBar,
    manaBar
}

public class PlayerBar : MonoBehaviour
{
    private Slider slider;
    private PlayerController player;
    public BarType type;
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        switch (type)
        {
            case BarType.healthBar:
                slider.maxValue = PlayerController.MAX_HEALTH;
                break;
            case BarType.manaBar:
                slider.maxValue = PlayerController.MAX_MANA;
                               break;

        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (type)
        {
            case BarType.healthBar:
                slider.value = player.GetHealth();
                break;

            case BarType.manaBar:
                slider.value = player.GetMana();
                break;
        }

    }
}
