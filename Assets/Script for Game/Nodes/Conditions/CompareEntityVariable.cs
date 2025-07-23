using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeName(nameof(CompareEntityVariable))]
[CustomNodeParam("variable", "only {e_var}", isRequired: true)]
[CustomNodeParam("operator", "is_equal_to|is_not_equal_to|is_null|is_not_null", isRequired: true)]
[NodeParam("value", NodeParamType.EntityVariable, isRequired: false)]
[NodeParam("resultTarget", NodeParamType.BoolVariable, isRequired: false)]
[NodeDescription("Compares an entity variable with another entity or null. value is optional. resultTarget is for saving the result of the comparison.")]
public class CompareEntityVariable : ConditionNode, IUsableNode
{
    private BTValue<Entity> variable_entity;
    private string operatorString;
    private BTValue<Entity> valueToCompare_entity;
    private BTValue<bool> resultTarget;

    public override NodeState Evaluate()
    {
        Entity variableValue = variable_entity.GetValue(entity.Blackboard);
        Entity valueToCompareValue = valueToCompare_entity != null ? valueToCompare_entity.GetValue(entity.Blackboard) : null;

        bool result = false;
        switch (operatorString)
        {
            case "is_equal_to": result = Equals(variableValue, valueToCompareValue); break;
            case "is_not_equal_to": result = !Equals(variableValue, valueToCompareValue); break;
            case "is_null": result = variableValue == null; break;
            case "is_not_null": result = variableValue != null; break;
        }

        if (resultTarget != null)
            resultTarget.SetValue(entity.Blackboard, result);

        return result ? ReturnSuccess() : ReturnFailure();
    }

    public override void FromJson(JObject json)
    {
        // Variable
        JToken variableJToken = json["variable"];
        variable_entity = BTValue<Entity>.FromJToken(variableJToken);

        // Operator
        operatorString = json["operator"].ToObject<string>();

        // Value to Compare
        if (json.ContainsKey("value"))
        {
            JToken valueToCompareJToken = json["value"];
            valueToCompare_entity = BTValue<Entity>.FromJToken(valueToCompareJToken);
        }

        // Output Target
        if (json.ContainsKey("resultTarget"))
            resultTarget = BTValue<bool>.FromJToken(json["resultTarget"]);
    }
}