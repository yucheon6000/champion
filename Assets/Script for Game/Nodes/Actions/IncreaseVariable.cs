using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeName(nameof(IncreaseVariable))]
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
        string variableKey = json["variable"].ToObject<string>();
        string amountKey = json["amount"].ToObject<string>();

        // Int
        if (variableKey.Contains("i_"))
        {
            variableType = VariableType.Int;

            variable_int = new BTValue<int>(variableKey);

            if (amountKey.Contains("i_"))
                amount_int = new BTValue<int>(amountKey);
            else
                amount_int = new BTValue<int>(json["amount"].ToObject<int>());
        }

        // Float
        else if (variableKey.Contains("f_"))
        {
            variableType = VariableType.Float;

            variable_float = new BTValue<float>(variableKey);

            if (amountKey.Contains("f_"))
                amount_float = new BTValue<float>(amountKey);
            else
                amount_float = new BTValue<float>(json["amount"].ToObject<float>());
        }
    }
}
