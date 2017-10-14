// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Pocket;

namespace Peaky
{
    public class TestTargetRegistry : IEnumerable<TestTarget>
    {
        private readonly IDictionary<string, Lazy<TestTarget>> targets = new Dictionary<string, Lazy<TestTarget>>();

        public TestTargetRegistry Add(
            string environment,
            string application,
            Uri baseAddress,
            Action<TestDependencyRegistry> testDependencies = null)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }
            if (baseAddress == null)
            {
                throw new ArgumentNullException(nameof(baseAddress));
            }
            if (!baseAddress.IsAbsoluteUri)
            {
                throw new ArgumentException("Base address must be an absolute URI.");
            }

            var container = new PocketContainer()
                .Register(c => new HttpClient())
                .RegisterSingle(c =>  new TestTarget(c.Resolve)
            {
                Application = application,
                Environment = environment,
                BaseAddress = baseAddress
            });

            testDependencies?.Invoke(new TestDependencyRegistry((t, func) => container.Register(t, c => func())));

            container.AfterCreating<HttpClient>(client =>
            {
                if (client.BaseAddress == null)
                {
                    client.BaseAddress = baseAddress;
                }
                return client;
            });

            targets.Add($"{environment}:{application}",
                        new Lazy<TestTarget>(() => container.Resolve<TestTarget>()));

            return this;
        }

        internal TestTarget Get(string environment, string application)
        {
            var testTarget = TryGet(environment, application);
            if (testTarget != null)
            {
                return testTarget;
            }

            if (!targets.Any(t => t.Value.Value.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase)))
            {

                throw new TestNotDefinedException($"Environment '{environment}' has not been defined.");
            }

            throw new TestNotDefinedException($"Environment '{environment}' has no application named '{application}' defined.");
        }

        internal TestTarget TryGet(string environment, string application)
        {
            Lazy<TestTarget> target;
            if (targets.TryGetValue(environment + ":" + application, out target))
            {
                return target.Value;
            }
            return null;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<TestTarget> GetEnumerator() =>
            targets.Select(c => c.Value.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}