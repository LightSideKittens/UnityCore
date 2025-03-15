using System.Collections.Generic;
using UnityEngine;

public static class LSConsts
{
    public static class Env
    {
        public const string Dev = nameof(Dev);
        public const string Prod = nameof(Prod);
        
        public static IEnumerable<string> Environments
        {
            get
            {
                yield return Dev;
                yield return Prod;
            }
        }
    }
}

