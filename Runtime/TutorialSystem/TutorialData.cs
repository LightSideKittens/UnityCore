using System;
using System.Collections.Generic;
using LSCore.ConfigModule;
using Newtonsoft.Json;

public class TutorialData : BaseConfig<TutorialData>
{
    protected override string FileName => "tutorialData";
    [JsonProperty] private readonly HashSet<Type> completedSteps = new HashSet<Type>();

    public static void OnStepComplete(Type stepType)
    {
        Config.completedSteps.Add(stepType);
    }

    public static bool CheckStepComplete(Type stepType)
    {
        return Config.completedSteps.Contains(stepType);
    }
}
