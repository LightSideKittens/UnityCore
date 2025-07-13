using LSCore.DataStructs;

namespace LSCore
{
    internal interface IInputProvider
    {
        ArraySlice<LSTouch> GetTouches();
    }
}