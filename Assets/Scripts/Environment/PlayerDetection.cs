using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    private Animator anim; //addedbyEilaf

    void Update()
    {
        anim = GetComponent<Animator>(); //addedbyEilaf
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"DoorTrigger hit by: {other.gameObject.name} | Tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
           anim.SetTrigger("Sway");
        }
    }
}
