namespace Dixin.Threading
{
    using System;
    using System.Threading;

    public static class CpuUsage
    {
        public static void Draw()
        {
            Thread thread0 = new Thread(() =>
            {
                ThreadHelper.AssignCurrentThreadToCpu(0);
                Draw((index, count) => Math.Sin((2 * Math.PI) * ((double)index / count)) / 2 + 0.5);
            });
            Thread thread1 = new Thread(() =>
            {
                ThreadHelper.AssignCurrentThreadToCpu(1);
                Draw((index, count) => 0.5);
            });
            Thread thread2 = new Thread(() =>
            {
                ThreadHelper.AssignCurrentThreadToCpu(2);
                Draw((index, count) => (double)index / (count - 1));
            });
            Thread thread3 = new Thread(() =>
            {
                ThreadHelper.AssignCurrentThreadToCpu(3);
                Draw((index, count) => index < count / 2 ? 0 : 1);
            });

            thread0.Start();
            thread1.Start();
            thread2.Start();
            thread3.Start();

            thread0.Join();
            thread1.Join();
            thread2.Join();
            thread3.Join();
        }

        private static void Draw(
            Func<int, int, double> getCpuUsage, TimeSpan? period = null, TimeSpan? frame = null)
        {
            period = period ?? TimeSpan.FromSeconds(20);
            frame = frame ?? TimeSpan.FromSeconds(0.5);

            int millisecondsPerFrame = (int)frame.Value.TotalMilliseconds;
            int frameCountPerPeriod = (int)(period.Value.TotalMilliseconds / millisecondsPerFrame);
            
            if (frameCountPerPeriod < 1)
            {
                throw new InvalidOperationException();
            }

            while (true)
            {
                for (int frameIndex = 0; frameIndex < frameCountPerPeriod; frameIndex++)
                {
                    // If the target CPU usage is 70%,
                    double cpuUsage = getCpuUsage(frameIndex, frameCountPerPeriod);
                    if (cpuUsage < 0 || cpuUsage > 1)
                    {
                        throw new InvalidOperationException();
                    }

                    // the thread spins for 70% of the time,
                    double busyTimePerFrame = millisecondsPerFrame * cpuUsage;
                    double busyStartTime = Environment.TickCount;
                    while (Environment.TickCount - busyStartTime <= busyTimePerFrame)
                    {
                    }
                    
                    // and sleeps for the rest 30% of time.
                    int idleTimePerFrame = (int)(millisecondsPerFrame - busyTimePerFrame);
                    Thread.Sleep(idleTimePerFrame);
                }
            }
        }
    }
}
