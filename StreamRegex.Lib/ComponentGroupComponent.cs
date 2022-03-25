namespace StreamRegex.Lib;

public class ComponentGroupComponent : RegexComponent
{
    private readonly IEnumerable<RegexComponent> _components;
    public ComponentGroupComponent(IEnumerable<RegexComponent> components)
    {
        _components = components;
    }

    public override (bool doesMatch, bool canAdvance, bool mustAdvance) IsMatch(char character)
    {
        bool result = _components.Any(x => x.IsMatch(character).doesMatch);
        return (result, result, result);
    }
}