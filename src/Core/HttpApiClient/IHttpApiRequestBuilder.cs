﻿using System;

namespace BreadTh.StronglyApied
{
    public interface IHttpApiRequestBuilder<OUTCOME>
    {
        IHttpApiRequestBuilder<OUTCOME> AddJsonBody(object body);
        IHttpApiRequestBuilder<OUTCOME> AddHeader(string name, string value);
        ICallResultParser<OUTCOME> PerformCall(Func<FailedHttpCallContext, OUTCOME> onTransitError, TimeSpan[] retrySpacing = null);
        ICallResultParser<OUTCOME> PerformCall(Func<string, OUTCOME> onTransitError, TimeSpan[] retrySpacing = null);
    }
}
