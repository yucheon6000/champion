using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RequiresBTComponentAttribute : Attribute
{
    public Type RequiredComponentType { get; }

    public RequiresBTComponentAttribute(Type componentType)
    {
        this.RequiredComponentType = componentType;
    }
}