
using UnityEngine;

public class HeartCollectible : MonoBehaviour
{
    [SerializeField] private int healAmount = 1;
    [SerializeField] private float pickupRadius = 1.5f;
    private Animator anim; //addedbyEilaf
    
    private void Awake()
    {
        anim = GetComponent<Animator>(); //addedbyEilaf
    }

    private void Update()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance <= pickupRadius)
            {
                ProcessHeal(player);
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        ProcessHeal(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessHeal(collision.gameObject);
    }

    private void ProcessHeal(GameObject otherObj)
    {
        PlayerHealth health = otherObj.GetComponent<PlayerHealth>();

        if (health == null)
        {
            health = otherObj.GetComponentInParent<PlayerHealth>();
        }

        if (health == null)
        {
            health = otherObj.GetComponentInChildren<PlayerHealth>();
        }

        if (health == null)
        {
            PlayerController pc = otherObj.GetComponent<PlayerController>();
            if (pc == null) pc = otherObj.GetComponentInParent<PlayerController>();
            if (pc == null) pc = otherObj.GetComponentInChildren<PlayerController>();
            if (pc != null)
            {
                health = pc.GetComponent<PlayerHealth>();
            }
        }
        

        if (health == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null && (otherObj == player || otherObj.transform.IsChildOf(player.transform)))
            {
                health = player.GetComponent<PlayerHealth>();
            }
        }

        if (health != null && health.GetCurrentHealth() < health.GetMaxHealth())
        {
            health.Heal(healAmount);

            anim.SetTrigger("Sway");

            Destroy(gameObject, 1f);
        }
    }
}