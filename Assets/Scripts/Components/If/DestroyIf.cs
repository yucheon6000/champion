public class DestroyIf : ConditionalActionIf
{
    protected override string GetComponentType() => "DestroyIf";

    protected override void ExecuteAction()
    {
        Destroy(gameObject);
    }
}