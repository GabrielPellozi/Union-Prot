public abstract class BaseState<S, C>
{
    public C Controller { get; set; }

    public abstract void OnStateEnter();

    public abstract void OnStateExit();

    public abstract void OnStateUpdate();

    public abstract void OnStateFixedUpdate();
}