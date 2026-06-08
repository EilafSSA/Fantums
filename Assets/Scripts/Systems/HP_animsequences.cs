using UnityEngine;

public class HP_animsequences : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private PlayerHealth playerHealth;

    private Animator anim; //addedbyEilaf

    private void Awake()
    {
        anim = GetComponent<Animator>(); //addedbyEilaf
    }

    // Update is called once per frame
    void Update()
    {
        if (playerHealth != null)
        {
            anim.SetInteger("Health", playerHealth.GetCurrentHealth());
        }
    }
}
