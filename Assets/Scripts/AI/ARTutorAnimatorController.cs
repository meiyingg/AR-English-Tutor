using UnityEngine;

public class ARTutorAnimatorController : MonoBehaviour
{
    public static ARTutorAnimatorController Instance { get; private set; }
    public Animator animator;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Plays an animation by setting a trigger on the Animator.
    /// Resets other triggers to ensure clean transitions.
    /// </summary>
    /// <param name="triggerName">The name of the trigger in the Animator Controller.</param>
    public void PlayAnimation(string triggerName)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator not assigned on ARTutorAnimatorController.");
            return;
        }

        // Reset all other triggers first
        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(param.name);
            }
        }
        
        // Set the new trigger
        animator.SetTrigger(triggerName);
    }
    
    /// <summary>
    /// Sets the thinking state of the avatar.
    /// </summary>
    /// <param name="isThinking">True to start thinking, false to stop.</param>
    public void SetThinking(bool isThinking)
    {
        if (animator != null)
        {
            animator.SetBool("IsThinking", isThinking);
        }
    }

    /// <summary>
    /// Sets the talking state of the avatar.
    /// </summary>
    /// <param name="isTalking">True to start talking, false to stop.</param>
    public void SetTalking(bool isTalking)
    {
        if (animator != null)
        {
            animator.SetBool("IsTalking", isTalking);
        }
    }

    // Arm Stretching
    public void PlayArmStretching() => PlayAnimation("ArmStretching");

    // Bboy Hip Hop Move
    public void PlayBboyHipHopMove() => PlayAnimation("BboyHipHopMove");

    // Pointing Forward
    public void PlayPointingForward() => PlayAnimation("PointingForward");

    // Waving
    public void PlayWaving() => PlayAnimation("Waving");
}
