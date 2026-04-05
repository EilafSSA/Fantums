using UnityEngine;
// btw im really sorry if the code isnt as clean, im trying to make it clean so anyone can really read it, trying to optimize it so like u get a general understanding on the whole thing and I guess i just like taking notes but feel free to remove them or add on them cause i check after like i push a github :D
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
        Destroy(gameObject);
    }
}