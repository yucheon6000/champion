using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[CustomNodeParam("variable", "only {i_var}|only {f_var}", isRequired: true)]
[CustomNodeParam("amount", "int_or_{i_var}|float_or_{f_var}", isRequired: true)]
[NodeDescription("Increases a variable by a value and returns Success. Minus amount is also possible.")]

public class IncreaseVariable : ActionNode, IUsableNode
{
    enum VariableType
    {
        Int,
        Float
    }

    private VariableType variableType;

    private BTValue<int> variable_int;
    private BTValue<float> variable_float;

    private BTValue<int> amount_int;
    private BTValue<float> amount_float;

    public override NodeState Evaluate()
    {
        if (variableType == VariableType.Int)
        {
            int variableValue = variable_int.GetValue(entity.Blackboard);
            int amountValue = amount_int.GetValue(entity.Blackboard);

            variableValue += amountValue;
            variable_int.SetValue(entity.Blackboard, variableValue);
        }

        else if (variableType == VariableType.Float)
        {
            float variableValue = variable_float.GetValue(entity.Blackboard);
            float amountValue = amount_float.GetValue(entity.Blackboard);

            variableValue += amountValue;
            variable_float.SetValue(entity.Blackboard, variableValue);
        }

        return ReturnSuccess();
    }

    public override void FromJson(JObject json)
    {
        // Int
        if (json["variable"].ToObject<string>().Contains("i_"))
        {
            variableType = VariableType.Int;

            variable_int = BTValue<int>.FromJToken(json["variable"], 0);
            amount_int = BTValue<int>.FromJToken(json["amount"], 0);
        }

        // Float
        else if (json["variable"].ToObject<string>().Contains("f_"))
        {
            variableType = VariableType.Float;

            variable_float = BTValue<float>.FromJToken(json["variable"], 0);
            amount_float = BTValue<float>.FromJToken(json["amount"], 0);
        }
    }
}
