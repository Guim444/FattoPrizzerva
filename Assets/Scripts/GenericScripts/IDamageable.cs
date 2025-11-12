using UnityEngine;

public interface IDamageable
{
    float HP { get; set; }
    public void TakeDamage(int dmg);
    public void Die();
}
