using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace API
{
    public class CircuitBreaker
    {
         public static AsyncCircuitBreakerPolicy CreatePolicy()
    {
        return Policy
            .Handle<Exception>()
            .AdvancedCircuitBreakerAsync(0.25, TimeSpan.FromSeconds(60), 7, TimeSpan.FromSeconds(30),
                onBreak: (_, _) =>
                {
                    // Implementação do onBreak
                },
                onReset: () =>
                {
                    // Implementação do onReset
                },
                onHalfOpen: () =>
                {
                    // Implementação do onHalfOpen
                });
    }


        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            //return HttpPolicyExtensions
            //.HandleTransientHttpError()
            //.CircuitBreakerAsync(3, TimeSpan.FromMinutes(1));

            /*
             * the circuit will be cut if 25% of requests fail in a 60 second window, with a minimum of 7 requests in the 60 second window, then the circuit should be cut for 30 seconds:
             * */

            return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .AdvancedCircuitBreakerAsync(0.25, TimeSpan.FromSeconds(60), 7, TimeSpan.FromSeconds(30));
        }


        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(10));
        }
    }
}
