using System.Collections.Generic;
using LSCore;
using LSCore.Attributes;

public class SceneSelectorAttributeDrawer : BaseValueDropdownDrawer<SceneSelectorAttribute>
{
    protected override object GetValue()
    {
        return SceneSelectorAttribute.SceneNames;
    }
}