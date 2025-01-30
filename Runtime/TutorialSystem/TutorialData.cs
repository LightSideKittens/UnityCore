using System;
using System.Collections.Generic;
using LSCore.ConfigModule;
using Newtonsoft.Json;

public class TutorialData : GameSingleConfig<TutorialData>
{
    [JsonProperty] private readonly HashSet<Type> completedSteps = new();
    
    public static void OnStepComplete(Type stepType)
    {
        Config.completedSteps.Add(stepType);
    }

    public static bool CheckStepComplete(Type stepType)
    {
        return Config.completedSteps.Contains(stepType);
    }
}
