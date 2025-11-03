using UnityEngine;
using TMPro;

public class PlayerStaminaManager : MonoBehaviour, IEstaminable
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina { get; private set; }
    public float Stamina { get; set; }

    [Header("Rates Per Second")]
    public float walkingRegen = 25f;
    public float idleRegen = 50f;

    private float staminaRate = 0f; // >0 regen, <0 drain

    public bool isTired = false;

    [Header("UI")]
    public TextMeshProUGUI staminaTextDisplay;

    private void Awake()
    {
        currentStamina = maxStamina;
        DisplayStamina();
    }

    private void Update()
    {
        if (Mathf.Abs(staminaRate) > 0.01f)
        {
            ModifyStamina(staminaRate * Time.deltaTime);
        }

        if (currentStamina <= 0)
        {
            isTired = true;
        }
        else if (currentStamina >= 20f)
        {
            isTired = false;
        }
    }

    // Called by states on enter
    public void SetRunning(float staminaCostPerSecond) => staminaRate = -staminaCostPerSecond;
    public void SetWalking() => staminaRate = walkingRegen;
    public void SetIdle() => staminaRate = idleRegen;
    public void SetTired() => staminaRate = walkingRegen;
    public void StopAllRegenDrain() => staminaRate = 0f; // optional

    private void ModifyStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
        DisplayStamina();
    }

    private void DisplayStamina()
    {
        if (staminaTextDisplay != null)
            staminaTextDisplay.text = $"Stamina: {currentStamina:F0}%";
    }

    public void UseStamina(float amount)
    {
        throw new System.NotImplementedException();
    }
}