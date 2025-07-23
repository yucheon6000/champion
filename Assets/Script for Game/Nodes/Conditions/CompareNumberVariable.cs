using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeName(nameof(CompareNumberVariable))]
[CustomNodeParam("variable", "only {i_var}|only {f_var}", isRequired: true)]
[CustomNodeParam("operator", "is_equal_to|is_not_equal_to|is_greater_than|is_greater_than_or_equal_to|is_less_than|is_less_than_or_equal_to", isRequired: true)]
[CustomNodeParam("value", "int_or_{i_var}|float_or_{f_var}", isRequired: true)]
[NodeParam("resultTarget", NodeParamType.BoolVariable, isRequired: false)]
[NodeDescription("Compares a number variable (int/float) with a value and returns Success if they match, otherwise returns Failure. resultTarget is for saving the result of the comparison.")]
public class CompareNumberVariable : ConditionNode, IUsableNode
{
    private enum NumberType { Int, Float }
    private NumberType numberType;
    private BTValue<int> variable_int;
    private BTValue<float> variable_float;
    private string operatorString;
    private BTValue<int> valueToCompare_int;
    private BTValue<float> valueToCompare_float;
    private BTValue<bool> resultTarget;

    public override NodeState Evaluate()
    {
        bool result = false;

        switch (numberType)
        {
            case NumberType.Int:
                result = EvaluateInt();
                break;
            case NumberType.Float:
                result = EvaluateFloat();
                break;
        }

        if (resultTarget != null)
            resultTarget.SetValue(entity.Blackboard, result);

        return result ? ReturnSuccess() : ReturnFailure();
    }

    private bool EvaluateInt()
    {
        int variableValue = variable_int.GetValue(entity.Blackboard);
        int valueToCompareValue = valueToCompare_int.GetValue(entity.Blackboard);
        switch (operatorString)
        {
            case "is_equal_to": return variableValue == valueToCompareValue;
            case "is_not_equal_to": return variableValue != valueToCompareValue;
            case "is_greater_than": return variableValue > valueToCompareValue;
            case "is_greater_than_or_equal_to": return variableValue >= valueToCompareValue;
            case "is_less_than": return variableValue < valueToCompareValue;
            case "is_less_than_or_equal_to": return variableValue <= valueToCompareValue;
            default: return false;
        }
    }

    private bool EvaluateFloat()
    {
        float variableValue = variable_float.GetValue(entity.Blackboard);
        float valueToCompareValue = valueToCompare_float.GetValue(entity.Blackboard);
        switch (operatorString)
        {
            case "is_equal_to": return variableValue == valueToCompareValue;
            case "is_not_equal_to": return variableValue != valueToCompareValue;
            case "is_greater_than": return variableValue > valueToCompareValue;
            case "is_greater_than_or_equal_to": return variableValue >= valueToCompareValue;
            case "is_less_than": return variableValue < valueToCompareValue;
            case "is_less_than_or_equal_to": return variableValue <= valueToCompareValue;
            default: return false;
        }
    }

    public override void FromJson(JObject json)
    {
        // Variable
        JToken variableJToken = json["variable"];
        if (variableJToken.ToObject<string>().Contains("i_"))
        {
            variable_int = BTValue<int>.FromJToken(variableJToken);
            numberType = NumberType.Int;
        }
        else if (variableJToken.ToObject<string>().Contains("f_"))
        {
            variable_float = BTValue<float>.FromJToken(variableJToken);
            numberType = NumberType.Float;
        }

        // Operator
        operatorString = json["operator"].ToObject<string>();

        // Value to Compare
        JToken valueToCompareJToken = json["value"];
        if (numberType == NumberType.Int)
            valueToCompare_int = BTValue<int>.FromJToken(valueToCompareJToken);
        else if (numberType == NumberType.Float)
            valueToCompare_float = BTValue<float>.FromJToken(valueToCompareJToken);

        // Output Target
        if (json.ContainsKey("resultTarget"))
            resultTarget = BTValue<bool>.FromJToken(json["resultTarget"]);
    }
}