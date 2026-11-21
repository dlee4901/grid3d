using UnityEngine;

public class EntityModel : LoggableBehaviour
{
    public const string MovingParameter = "Moving";

    private const float ActionBlendSeconds = 0.05f;
    private static readonly int MovingHash = Animator.StringToHash(MovingParameter);

    [SerializeField] private Animator _animator;

    public void SetMoving(bool moving)
    {
        if (isActiveAndEnabled) _animator.SetBool(MovingHash, moving);
    }

    public void Play(CasterAction action)
    {
        if (action != CasterAction.None && isActiveAndEnabled) _animator.CrossFadeInFixedTime(action.ToString(), ActionBlendSeconds);
    }
}
