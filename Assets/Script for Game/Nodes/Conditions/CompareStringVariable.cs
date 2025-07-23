using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeName(nameof(CompareStringVariable))]
[CustomNodeParam("variable", "only {s_var}", isRequired: true)]
[CustomNodeParam("operator", "is_equal_to|is_not_equal_to", isRequired: true)]
[CustomNodeParam("value", "string_or_{s_var}", isRequired: true)]
[NodeParam("resultTarget", NodeParamType.BoolVariable, isRequired: false)]
[NodeDescription("Compares a string variable with a value and returns Success if they match, otherwise returns Failure. resultTarget is for saving the result of the comparison.")]
public class CompareStringVariable : ConditionNode, IUsableNode
{
    private BTValue<string> variable_string;
    private string operatorString;
    private BTValue<string> valueToCompare_string;
    private BTValue<bool> resultTarget;

    public override NodeState Evaluate()
    {
        string variableValue = variable_string.GetValue(entity.Blackboard);
        string valueToCompareValue = valueToCompare_string.GetValue(entity.Blackboard);

        bool result = false;
        switch (operatorString)
        {
            case "is_equal_to": result = variableValue.Equals(valueToCompareValue); break;
            case "is_not_equal_to": result = !variableValue.Equals(valueToCompareValue); break;
        }

        if (resultTarget != null)
            resultTarget.SetValue(entity.Blackboard, result);

        return result ? ReturnSuccess() : ReturnFailure();
    }

    public override void FromJson(JObject json)
    {
        // Variable
        JToken variableJToken = json["variable"];
        variable_string = BTValue<string>.FromJToken(variableJToken);

        // Operator
        operatorString = json["operator"].ToObject<string>();

        // Value to Compare
        JToken valueToCompareJToken = json["value"];
        valueToCompare_string = BTValue<string>.FromJToken(valueToCompareJToken);

        // Output Target
        if (json.ContainsKey("resultTarget"))
            resultTarget = BTValue<bool>.FromJToken(json["resultTarget"]);
    }
}