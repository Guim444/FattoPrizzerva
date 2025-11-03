using UnityEngine;

public abstract class EnemyController : MonoBehaviour, IDamageable
{
    public int enduranceDistance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public abstract void TakeDamage();
}
