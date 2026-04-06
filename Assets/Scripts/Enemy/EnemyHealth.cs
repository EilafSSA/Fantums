using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    private Animator anim; //addedbyEilaf

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update() //addedbyEilaf
    {
        anim = GetComponent<Animator>(); //addedbyEilaf
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Enemy HP: {currentHealth}/{maxHealth}");
        anim.SetTrigger("IsHurt"); //addedbyEilaf

        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashDamage()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color original = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        sr.color = original;
    }

    private void Die()
    {
        Debug.Log("Enemy Died!");

        // award score on kill
        if (GameManager.Instance != null)
            GameManager.Instance.AddEnemyKillScore();

        Destroy(gameObject);
    }
}