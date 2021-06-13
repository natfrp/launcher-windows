using System;

namespace SakuraLibrary.Helper
{
    public class DispatcherWrapper
    {
        public Action<Action> Invoke, BeginInvoke;
        public Func<bool> CheckAccess;

        public DispatcherWrapper(Action<Action> invoke, Action<Action> beginInvoke, Func<bool> checkAccess)
        {
            Invoke = invoke;
            BeginInvoke = beginInvoke;
            CheckAccess = checkAccess;
        }
    }
}
