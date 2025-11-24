using UnityEngine;

public class meleeEnemys : Enemy, IDamageable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Attack();
    }

    public override void Attack()
    {
        base.Attack();
        Debug.Log("Ataque cuerpo a cuerpo");
    }
    
        public void TakeDamage(float damage)
    {
        Debug.Log("enemigo recibiendo daño");
    }
}
