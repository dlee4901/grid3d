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

    private int _actionHash;
    private int _actionFrame;

    public bool IsActing
    {
        get
        {
            if (_actionHash == 0 || !isActiveAndEnabled) return false;
            if (Time.frameCount <= _actionFrame + 1) return true;
            if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash == _actionHash) return true;
            if (_animator.GetNextAnimatorStateInfo(0).shortNameHash == _actionHash) return true;
            _actionHash = 0;
            return false;
        }
    }

    public void Play(CasterAction action)
    {
        if (action == CasterAction.None || !isActiveAndEnabled) return;
        var state = action.ToString();
        _actionHash = Animator.StringToHash(state);
        _actionFrame = Time.frameCount;
        _animator.CrossFadeInFixedTime(state, ActionBlendSeconds);
    }
}
