using Pomelo.AspNetCore.TimedJob;

namespace OMS.WebCore
{
    public class BaseTimeJob : Job
    {
        [Invoke(Begin = "2018-1-15 08:30", Interval = 1000 * 3600 * 24, SkipWhileExecuting = true, IsEnabled = true)]
        public void Run()
        {
            //执行逻辑
        }
    }
}
