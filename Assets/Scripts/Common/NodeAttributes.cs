using System;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Class)]
public class NodeTypeAttribute : Attribute
{
    public string Type { get; }
    public NodeTypeAttribute(string type) { this.Type = type; }
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeNameAttribute : Attribute
{
    public string Name { get; }
    public NodeNameAttribute(string name) { this.Name = name; }
}

public enum NodeParamType
{
    Bool, BoolOrVariable, BoolVariable,
    Int, IntOrVariable, IntVariable,
    Float, FloatOrVariable, FloatVariable,
    String, StringOrVariable, StringVariable,
    EntityVariable,
    OneNode, Nodes,
    None
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NodeParamAttribute : Attribute
{
    public string Name { get; }
    public NodeParamType Type { get; }
    public bool IsRequired { get; }

    static Dictionary<NodeParamType, string> nodeParamTypeToString = new Dictionary<NodeParamType, string>() {
        { NodeParamType.Bool, "only bool" },
        { NodeParamType.BoolOrVariable, "bool_or_{b_var}" },
        { NodeParamType.BoolVariable, "only {b_var}" },
        { NodeParamType.Int, "only int" },
        { NodeParamType.IntOrVariable, "int_or_{i_var}" },
        { NodeParamType.IntVariable, "only {i_var}" },
        { NodeParamType.Float, "only float" },
        { NodeParamType.FloatOrVariable, "float_or_{f_var}" },
        { NodeParamType.FloatVariable, "only {f_var}" },
        { NodeParamType.String, "only string" },
        { NodeParamType.StringOrVariable, "string_or_{s_var}" },
        { NodeParamType.StringVariable, "only {s_var}" },
        { NodeParamType.EntityVariable, "only {e_var}" },
        { NodeParamType.OneNode, "only one node object" },
        { NodeParamType.Nodes, "[node list]" }
    };

    public NodeParamAttribute(string name, NodeParamType type, bool isRequired = false)
    {
        this.Name = name;
        this.Type = type;
        this.IsRequired = isRequired;
    }

    public virtual string TypeString()
    {
        return nodeParamTypeToString[Type];
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CustomNodeParamAttribute : NodeParamAttribute
{
    public new string Type { get; }

    public CustomNodeParamAttribute(string name, string type, bool isRequired = false) : base(name, NodeParamType.None, isRequired)
    {
        this.Type = type;
    }

    public override string TypeString()
    {
        return Type;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class NodeDescriptionAttribute : Attribute
{
    public string Description { get; }

    public NodeDescriptionAttribute(string description)
    {
        this.Description = description;
    }
}