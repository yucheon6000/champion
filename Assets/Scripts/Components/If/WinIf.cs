using UnityEngine;

public class WinIf : ConditionalActionIf
{
    protected override string GetComponentType() => "WinIf";

    protected override void ExecuteAction()
    {
        Debug.Log("Game Win!");
        // GameManager.Instance.LoseGame(); 와 같은 실제 처리도 여기에
    }
}