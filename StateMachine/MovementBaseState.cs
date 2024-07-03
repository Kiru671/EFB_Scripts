using UnityEngine;

public abstract class MovementBaseState
{
    public abstract void EnterState(PlayerStateMachine context);

    public abstract void UpdateState(PlayerStateMachine context, Vector2 move, Vector2 MouseMove);

    public abstract void FixedUpdateState(PlayerStateMachine context, Vector2 move, Vector2 MouseMove);

    public abstract void HandleMovement(PlayerStateMachine context, Vector2 move, Vector2 MouseMove);

    protected virtual bool GroundCheck(PlayerStateMachine context)
    {
        return false;
    }
}
