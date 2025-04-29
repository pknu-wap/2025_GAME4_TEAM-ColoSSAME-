using UnityEngine;

public class CharacterValue : MonoBehaviour
{
    public float maxHp = 100;
    public float currentHp = 100;

    public HealthBar healthBar;

    void Start()
    {
        currentHp = maxHp;
        healthBar.SetHealth(currentHp, maxHp);
    }

    public void TakeDamage(float dmg)
    {
        currentHp -= dmg;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        healthBar.SetHealth(currentHp, maxHp);
    }
}