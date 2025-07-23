using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeName(nameof(CompareBoolVariable))]
[CustomNodeParam("variable", "only {b_var}", isRequired: true)]
[CustomNodeParam("operator", "is_equal_to|is_not_equal_to", isRequired: true)]
[CustomNodeParam("value", "bool_or_{b_var}", isRequired: true)]
[NodeParam("resultTarget", NodeParamType.BoolVariable, isRequired: false)]
[NodeDescription("Compares a bool variable with a value and returns Success if they match, otherwise returns Failure. resultTarget is for saving the result of the comparison.")]
public class CompareBoolVariable : ConditionNode, IUsableNode
{
    private BTValue<bool> variable_bool;
    private string operatorString;
    private BTValue<bool> valueToCompare_bool;
    private BTValue<bool> resultTarget;

    public override NodeState Evaluate()
    {
        bool variableValue = variable_bool.GetValue(entity.Blackboard);
        bool valueToCompareValue = valueToCompare_bool.GetValue(entity.Blackboard);

        bool result = false;
        switch (operatorString)
        {
            case "is_equal_to": result = variableValue == valueToCompareValue; break;
            case "is_not_equal_to": result = variableValue != valueToCompareValue; break;
        }

        if (resultTarget != null)
            resultTarget.SetValue(entity.Blackboard, result);

        return result ? ReturnSuccess() : ReturnFailure();
    }

    public override void FromJson(JObject json)
    {
        // Variable
        JToken variableJToken = json["variable"];
        variable_bool = BTValue<bool>.FromJToken(variableJToken);

        // Operator
        operatorString = json["operator"].ToObject<string>();

        // Value to Compare
        JToken valueToCompareJToken = json["value"];
        valueToCompare_bool = BTValue<bool>.FromJToken(valueToCompareJToken);

        // Output Target
        if (json.ContainsKey("resultTarget"))
            resultTarget = BTValue<bool>.FromJToken(json["resultTarget"]);
    }
}