using System.Collections.Generic;

namespace LSCore.Extensions
{
    public static class SelectEx
    {
        public static bool TryParse(string expression)
        {
            using var enumerator = GetIndexes(expression, int.MaxValue).GetEnumerator();
            return enumerator.MoveNext();
        }
        
        public static bool TryParseEveryPart(string expression, List<string> incorrectParts)
        {
            incorrectParts.Clear();
            
            if (expression.Contains(','))
            {
                var split = expression.Split(',');

                for (int i = 0; i < split.Length; i++)
                {
                    using var enumerator = GetIndexes(split[i], int.MaxValue).GetEnumerator();
                    if (!enumerator.MoveNext())
                    {
                        incorrectParts.Add($"\"{split[i]}\" at {i}");
                    }
                }
            }
            else
            {
                using var enumerator = GetIndexes(expression, int.MaxValue).GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    incorrectParts.Add($"\"{expression}\"");
                }
            }
            
            return incorrectParts.Count == 0;
        }
        
        public static IEnumerable<int> GetIndexes(string expression, int count)
        { 
            if (expression.Contains(','))
            {
                var split = expression.Split(',');

                for (int i = 0; i < split.Length; i++)
                {
                    foreach (var index in Internal_GetIndexes(split[i], count))
                    {
                        yield return index;
                    }
                }
            }
            else
            {
                foreach (var index in Internal_GetIndexes(expression, count))
                {
                    yield return index;
                }
            }
        }

        private static IEnumerable<int> Internal_GetIndexes(string expression, int count)
        {
            if(IndexEx.TryParse(expression, out var index))
            {
                yield return index.GetOffset(count);
            }
            else if(RangeEx.TryParse(expression, out var range))
            {
                var a = RangeEx.Range(count, range);

                for (int i = a.start; i < a.end; i++)
                {
                    yield return i;
                }
            }
        }
    }
}