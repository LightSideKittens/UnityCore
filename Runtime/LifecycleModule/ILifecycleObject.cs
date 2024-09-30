using LSCore.Extensions;

namespace LSCore.LifecycleSystem
{
    public interface ILifecycleObject<out T>
    {
        string Id { get; }
        T Create(string placementId, string id);
        void BuildTargetData(RJToken token);
    }
}