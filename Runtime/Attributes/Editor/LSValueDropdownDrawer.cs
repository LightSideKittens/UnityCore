using LSCore;
using LSCore.Attributes;
using Sirenix.OdinInspector.Editor.ValueResolvers;

public class LSValueDropdownDrawer : BaseValueDropdownDrawer<LSValueDropdownAttribute>
{
    private ValueResolver<object> rawGetter;

    protected override void Initialize()
    {
        var path = Attribute.ValuesGetter;
        var segments = path.Split('/');
        var targetName = segments[^1];
        path = segments.Length > 1 ? path.Replace($"/{targetName}", string.Empty) : string.Empty;

        rawGetter = ValueResolver.Get<object>(Property.GetPropertyByPath(path), targetName);
        base.Initialize();
    }

    protected override object GetValue()
    {
        return rawGetter.GetValue();
    }
}