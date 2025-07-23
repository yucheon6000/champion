using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class BTValue<T>
{
    private enum EValueType { Literal, BlackboardKey }

    private readonly EValueType valueType;
    private readonly T literalValue;
    private readonly string blackboardKey;
    private readonly T defaultValue;

    public BTValue(T literalValue)
    {
        valueType = EValueType.Literal;
        this.literalValue = literalValue;
        blackboardKey = null;
        defaultValue = default;
    }

    public BTValue(string blackboardKey)
    {
        valueType = EValueType.BlackboardKey;
        literalValue = default;
        this.blackboardKey = blackboardKey;
        defaultValue = default;
    }

    public BTValue(string blackboardKey, T defaultValue)
    {
        valueType = EValueType.BlackboardKey;
        literalValue = default;
        this.blackboardKey = blackboardKey;
        this.defaultValue = defaultValue;
    }

    public T GetValue(Blackboard blackboard)
    {
        if (valueType == EValueType.Literal)
            return literalValue;
        else
            return blackboard.GetValue<T>(blackboardKey, defaultValue);
    }

    public void SetValue(Blackboard blackboard, T value)
    {
        if (valueType == EValueType.Literal)
            Debug.LogWarning("Cannot set value for a fixed (Literal) BTValue.");
        else
            blackboard.SetValue<T>(blackboardKey, value);
    }

    public JToken ToJToken()
    {
        if (valueType == EValueType.Literal)
            return JToken.FromObject(literalValue);
        else
            return JToken.FromObject(blackboardKey);
    }

    public static BTValue<T> FromJToken(JToken token, T defaultValue = default)
    {
        // If token is null, set default value.
        if (token == null || token.Type == JTokenType.Null)
        {
            return new BTValue<T>(defaultValue);
        }

        // If token is a string and variable with { }
        if (token.Type == JTokenType.String)
        {
            string stringValue = (string)token;
            if (stringValue.StartsWith("{") && stringValue.EndsWith("}"))
            {
                // { } 제거하고 변수명만 저장
                string variableName = stringValue.Substring(1, stringValue.Length - 2);
                return new BTValue<T>(variableName, defaultValue);
            }
        }

        // Set value as literal value.
        try
        {
            return new BTValue<T>(token.ToObject<T>());
        }
        catch (System.Exception)
        {
            Debug.LogWarning($"Value of JToken ('{token}') can't be converted to {typeof(T).Name}. Using default value.");
            return new BTValue<T>(defaultValue);
        }
    }
}
