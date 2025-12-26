using System;

[Serializable]
public class EmptyModifier : BaseModifier
{
    protected override void CreateBuffers() {}
    protected override void Subscribe() { }
    protected override void Unsubscribe() { }
    protected override void ReleaseBuffers() { }
    protected override void ClearBuffers() { }
    protected override void OnApply(int start, int end, string parameter) { }
}