using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace GraphQL.Client
{
    public class GraphOutputStats
    {
        public TimeSpan HttpDuration { get; set; }
        public int RequestBodySize { get; set; }
        public int RequestHeaderSize { get; set; }
        public int RequestSize => RequestHeaderSize + RequestBodySize;
        public int ResponseBodySize { get; set; }
        public int ResponseHeaderSize { get; set; }
        public int ResponseSize => ResponseHeaderSize + ResponseBodySize;
        public HttpStatusCode StatusCode { get; set; }
        public DateTime TimeFinished { get; set; }
        public DateTime TimeStarted { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public Uri Url { get; set; }
        private Stopwatch HttpStopwatch { get; set; }
        private Stopwatch Stopwatch { get; set; }

        internal GraphOutputStats Start()
        {
            if (Stopwatch == null)
            {
                Stopwatch = new Stopwatch();
            }
            Stopwatch.Start();
            return this;
        }

        internal GraphOutputStats StartHttp(int requestHeaderSize, int requestBodySize)
        {
            if (HttpStopwatch == null)
            {
                HttpStopwatch = new Stopwatch();
            }
            HttpStopwatch.Start();

            RequestHeaderSize = requestHeaderSize;
            RequestBodySize = requestBodySize;

            return this;
        }

        internal GraphOutputStats Stop()
        {
            if (Stopwatch == null)
            {
                return this;
            }
            Stopwatch.Stop();

            TotalDuration = Stopwatch.Elapsed;
            TimeFinished = DateTime.Now;
            TimeStarted = TimeFinished.Subtract(TotalDuration);

            return this;
        }

        internal GraphOutputStats StopHttp(HttpStatusCode statusCode, int responseHeaderSize, int responseBodySize)
        {
            if (Stopwatch == null)
            {
                return this;
            }
            HttpStopwatch.Stop();

            HttpDuration = HttpStopwatch.Elapsed;
            StatusCode = statusCode;
            ResponseHeaderSize = responseHeaderSize;
            ResponseBodySize = ResponseBodySize;

            return this;
        }
    }
}