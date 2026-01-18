using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;
    public RedHealthBarScript healthBar;
    public Transform respawnPoint;
    public Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BatScript enemy = collision.GetComponent<BatScript>();
        if (enemy)
        {
            TakeDamage(enemy.damage);
        }
    }
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            animator.SetBool("isDead", true);
            Die();
        }
    }

    void Die()
    {
        Respawn();
    }

    void Respawn()
    {
        transform.position = respawnPoint.position;
        currentHealth = maxHealth;
        healthBar.SetHealth(currentHealth);
        animator.SetBool("isDead", false);
    }
}
