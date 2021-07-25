using System;
using System.Numerics;

namespace Fiero.Business
{
    public readonly struct Relationship
    {
        public readonly StandingName Standing;

        public Relationship With(StandingName s) => new(s);

        public Relationship(StandingName s)
        {
            Standing = s;
        }
    }
}
