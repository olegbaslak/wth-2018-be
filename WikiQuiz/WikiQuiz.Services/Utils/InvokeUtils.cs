using System;
using System.Collections.Generic;
using System.Text;

namespace WikiQuiz.Services.Utils
{
    public class InvokeUtils
    {
        public static TReturn InvokeSafely<TReturn>(Func<TReturn> actionToRepeat, int triesCount = 3)
        {
            var returnedValue = default(TReturn);

            for (var i = 1; i <= triesCount; i++)
            {
                try
                {
                    return actionToRepeat();
                }
                catch (Exception e)
                {
                    if (i == triesCount)
                    {
                        throw;
                    }
                }
            }

            return returnedValue;
        }
    }
}
