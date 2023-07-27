using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SokuLauncher.Utils
{
    public class Debouncer
    {
        private List<CancellationTokenSource> StepperCancelTokens = new List<CancellationTokenSource>();
        private readonly int MillisecondsToWait;
        private readonly object _lockThis = new object();

        public Debouncer(int millisecondsToWait = 300)
        {
            MillisecondsToWait = millisecondsToWait;
        }

        public void Debouce(Action func)
        {
            CancelAllStepperTokens(); // Cancel all api requests;
            var newTokenSrc = new CancellationTokenSource();
            lock (_lockThis)
            {
                StepperCancelTokens.Add(newTokenSrc);
            }
            Task.Delay(MillisecondsToWait, newTokenSrc.Token).ContinueWith(task =>
            {
                if (!newTokenSrc.IsCancellationRequested)
                {
                    CancelAllStepperTokens();
                    StepperCancelTokens = new List<CancellationTokenSource>();
                    lock (_lockThis)
                    {
                        func();
                    }
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void CancelAllStepperTokens()
        {
            foreach (var token in StepperCancelTokens)
            {
                if (!token.IsCancellationRequested)
                {
                    token.Cancel();
                }
            }
        }
    }
}
