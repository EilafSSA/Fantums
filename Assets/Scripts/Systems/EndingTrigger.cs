using UnityEngine;

public class EndingTrigger : MonoBehaviour
{
    public PlayerController playeranim;


    private void OnTriggerEnter2D(Collider2D other)
    {
        playeranim.Ending();
    }
}
