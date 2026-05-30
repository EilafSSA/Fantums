using UnityEngine;

public class FinalBossMainBody : MonoBehaviour //addedbyEilaf
{

    private Animator anim; 

    private void Awake()
    { 
        anim = GetComponent<Animator>(); 
    }

    public void bodyHurt()
    {
        anim.SetTrigger("Hurt");
    }

    public void bodyDeath()
    {
        anim.SetTrigger("Death");
    }
}
