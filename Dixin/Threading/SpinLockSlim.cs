namespace Dixin.Threading
{
    using System;
    using System.Threading;

    public class SpinlockSlim
    {
        private const int Locked = 1;

        private const int Unlocked = 0;

        private int isLocking = Unlocked;

        public void Enter()
        {
            while(Interlocked.Exchange(ref this.isLocking, Locked) == Locked)
            {
            }
        }

        public void Exit() => Interlocked.Exchange(ref this.isLocking, Unlocked);

        public void Lock(Action action)
        {
            this.Enter();

            try
            {
                action();
            }
            finally
            {
                this.Exit();
            }
        }
    }
}
