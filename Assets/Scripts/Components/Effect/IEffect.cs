using Newtonsoft.Json.Linq;

public interface IEffect
{
    string Type { get; }

    JObject ToJson();
}