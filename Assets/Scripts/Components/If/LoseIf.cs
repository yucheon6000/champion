using UnityEngine;

public class LoseIf : ConditionalActionIf
{
    protected override string GetComponentType() => "LoseIf";

    protected override void ExecuteAction()
    {
        WinLoseManager.Instance.Lose();
    }
}