using System.Threading;
using System.Threading.Tasks;

namespace osuLoader
{
    class TaskHandler
    {
        public static Task Delay(int milliseconds)
        {
            var tcs = new TaskCompletionSource<object>();
            new Timer(_ => tcs.SetResult(null)).Change(milliseconds, -1);
            return tcs.Task;
        }
    }
}
