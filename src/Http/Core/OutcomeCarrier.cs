using System;

namespace BreadTh.StronglyApied.Http.Core
{
    public readonly struct OutcomeCarrier<OUTCOME>
    {
        public enum Status { Undefined, NotYetFound, AlreadyFound }

        public static OutcomeCarrier<OUTCOME> AlreadyFound(OUTCOME outcome) =>
            new OutcomeCarrier<OUTCOME>(Status.AlreadyFound, outcome);

        public static OutcomeCarrier<OUTCOME> NotYetFound() =>
            new OutcomeCarrier<OUTCOME>(Status.NotYetFound, default);


        public readonly Status status;
        public readonly OUTCOME outcome;

        private OutcomeCarrier(Status status, OUTCOME outcome)
        {
            this.status = status;
            this.outcome = outcome;
        }
    }
}
