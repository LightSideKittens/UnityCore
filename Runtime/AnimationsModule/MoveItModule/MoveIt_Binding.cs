public partial class MoveIt
{
    public interface IPropertyHandler
    {
        void HandleAnimatedProperty(Handler handler, HandlerEvaluator evaluator);
    }
    
    private bool isBound = false;
    
    private void BindCurrent()
    {
        isBound = true;
        
        for (int i = 0; i < currentHandlers.Count; i++)
        {
            var handler = currentHandlers[i];

            if (handler.TryGetPropBindingData(out var obj, out var go))
            {

                var type = obj.GetType();
                var evaluators = handler.evaluators;
                for (var j = 0; j < evaluators.Count; j++)
                {
                    var evaluator = evaluators[j];
                    evaluator.InitAccessor(type);
                    evaluator.InitDelegates();
                }
            }
        }
    }

    private void UnBindCurrent()
    {
        isBound = false;
    }
}