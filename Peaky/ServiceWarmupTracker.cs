// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using System.Timers;

namespace Peaky
{
    internal class ServiceWarmupTracker : IDisposable
    {
        private readonly IPerformServiceWarmup warmup;

        private readonly Timer timer;

        private bool warmedUp;

        public ServiceWarmupTracker(IPerformServiceWarmup warmup)
        {
            this.warmup = warmup ?? throw new ArgumentNullException(nameof(warmup));

            timer = new Timer(15000);

            timer.Elapsed += (sender, args) => warmedUp = false;
        }

        public async Task WarmUp()
        {
            if (!warmedUp)
            {
                await warmup.WarmUp();
                warmedUp = true;
            }
        }

        public void Dispose() => timer?.Dispose();
    }
}
