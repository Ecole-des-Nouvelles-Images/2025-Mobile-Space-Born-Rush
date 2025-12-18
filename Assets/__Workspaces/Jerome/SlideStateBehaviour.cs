// This script attaches TO A SPECIFIC STATE in your visual state machine

using UnityEngine;

public class SlideStateBehaviour : StateMachineBehaviour
{
    // Called when animation STARTS playing
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Started Slide Animation");
    }
    
    // Called when animation STOPS playing (EVEN IF INTERRUPTED!)
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Slide Animation Ended (even if interrupted!)");
        
        // This gets called whether slide finishes OR gets interrupted by hit/death/jump
        PlayerController player = animator.GetComponent<PlayerController>();
        if (player != null)
        {
            player.OnSlideAnimationComplete();
        }
    }
}
