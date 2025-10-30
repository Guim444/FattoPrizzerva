using TMPro;
using UnityEngine;

public class PlayerStaminaManager : MonoBehaviour, IEstaminable
{
    public PlayerController playerController;
    private float currentStamina, maxStamina = 100;
    public float Stamina { get => currentStamina; set => currentStamina = value; }
    public TextMeshProUGUI staminaTextDisplay;
    public void Awake()
    {
        currentStamina = maxStamina;
        playerController = GetComponent<PlayerController>();
    }
    public void LateUpdate()
    {
        switch (playerController.currentState)
        {
            case State.Running:
                UseStamina(10f * Time.deltaTime);
                break;
            case State.Moving:
                RecoverStamina(2 * Time.deltaTime);
                break;
            case State.Idle:
                RecoverStamina(4 * Time.deltaTime);
                break;
            default:
                break;
        }
    }
    public void UseStamina(float amount)
    {
        if (amount <= 0f) return;
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina); //It will never go below 0 or above maxStamina
        DisplayStamina();
    }
    public void RecoverStamina(float stamina, bool maxStaminaReached = false)
    {
        {
            if (!maxStaminaReached)
            {
                currentStamina += stamina * RecoverMultiplier(currentStamina);
                if (currentStamina > maxStamina)
                {
                    currentStamina = maxStamina;
                }
            }
            else
            {
                maxStamina += stamina;
                currentStamina = maxStamina;
            }
            DisplayStamina();
        }
    }
    private float RecoverMultiplier(float currentStamina)
    {
        if (currentStamina < 20)
        {
            return 1f;
        }
        else if (currentStamina >= 20 && currentStamina < 60)
        {
            return 1.5f;
        }
        else
        {
            return 2f;
        }
    }
    private void DisplayStamina()
    {
        staminaTextDisplay.text = "Stamina: " + currentStamina.ToString("F0") + "%";
    }
}
