using System;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpUI : MonoBehaviour
{
    public Text text;

    // Singleton.
    public static PowerUpUI Instance;

    private string Current;

    void Awake()
    {
        // Initialise variables.
        Instance = this;
    }

    void Start()
    {
        // Set our power up to none.
        Reset();
    }

    public void Set(PowerUp.Abilities powerUp)
    {
        var ability = Enum.GetName(typeof(PowerUp.Abilities), powerUp);

        Current = ability.ToLower();
    }

    public void Reset()
    {
        // Set our ability to none.
        Set(PowerUp.Abilities.None);
    }

    void OnGUI()
    {
        // Set the text.
        text.text = "Power up: " + Current;
    }
}
