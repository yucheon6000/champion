using UnityEngine;

public class LoseIf : ConditionalActionIf
{
    protected override string GetComponentType() => "LoseIf";

    protected override void ExecuteAction()
    {
        Debug.Log("Game Over!");
        // GameManager.Instance.LoseGame(); 와 같은 실제 처리도 여기에
    }
}