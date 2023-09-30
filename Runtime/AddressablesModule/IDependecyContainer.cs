using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace LightGamesCore.Runtime.AddressablesModule
{
    public interface IDependecyContainer
    {
        IEnumerable<AssetReference> Dependencies { get; }
    }
}