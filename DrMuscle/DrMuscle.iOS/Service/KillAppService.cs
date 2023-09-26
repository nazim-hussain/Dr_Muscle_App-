using System;
using System.Threading;
using DrMuscle.Dependencies;

namespace DrMuscle.iOS.Service
{
    public class KillAppService : IKillAppService
    {
        public KillAppService()
        {
        }

        public void ExitApp()
        {
            Thread.CurrentThread.Abort();
        }
    }
}
