using System;

public class TutorialManager
{
    public static void WaitStepFinish<T>(Action continuation) where T : BaseTutorialStep, new()
    {
        var type = typeof(T);

        if (TutorialData.CheckStepComplete(type))
        {
            continuation();
        }
        else
        {
            var step = new T();
            continuation += () => TutorialData.OnStepComplete(type);
            step.StartTutorial(continuation);
        }
    }
}
