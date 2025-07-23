using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

public static class NodeDocumentationGenerator
{
    public static string GenerateNodeDocumentation()
    {
        var stringBuilder = new StringBuilder();

        var nodeTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t =>
            typeof(IUsableNode).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract
        );

        foreach (var type in nodeTypes)
        {
            var nameAttr = type.GetCustomAttribute<NodeNameAttribute>();
            if (nameAttr == null) continue; // 이름이 없는 노드는 건너뜀

            var typeAttr = type.GetCustomAttribute<NodeTypeAttribute>();
            var descAttr = type.GetCustomAttribute<NodeDescriptionAttribute>();
            var paramAttrs = type.GetCustomAttributes<NodeParamAttribute>().ToList();

            var parts = new List<string>();

            // 1. type과 name 추가
            if (typeAttr != null) parts.Add($"\"type\": \"{typeAttr.Type}\"");
            parts.Add($"\"name\": \"{nameAttr.Name}\"");

            // 2. 파라미터들 추가
            foreach (var param in paramAttrs)
            {
                parts.Add($"\"{param.Name}\": \"{param.TypeString()}\"");
            }

            // 3. 필수 파라미터 목록 추가
            var requiredParams = paramAttrs.Where(p => p.IsRequired).Select(p => $"\"{p.Name}\"").ToList();
            if (requiredParams.Any())
            {
                parts.Add($"\"required\": [{string.Join(", ", requiredParams)}]");
            }

            // 4. 모든 조각을 합쳐서 한 줄의 주석 생성
            stringBuilder.Append("{ " + string.Join(", ", parts) + " }");
            if (descAttr != null)
            {
                stringBuilder.Append($" // {descAttr.Description}");
            }
            stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }
}