public interface IDestructible
{
    ToolType RequiredTool { get; }
    void TakeDamage(float amount);
}