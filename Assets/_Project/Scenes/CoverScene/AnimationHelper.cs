using UnityEngine;

public class AnimationHelper : MonoBehaviour
{
    public Animator animator;
    public string trigger;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger(trigger);
        }
    }
}
