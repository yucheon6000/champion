using UnityEngine;

public class WinIf : ConditionalActionIf
{
    protected override string GetComponentType() => "WinIf";

    protected override void ExecuteAction()
    {
        WinLoseManager.Instance.Win();
    }
}