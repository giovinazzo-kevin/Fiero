using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public partial class AiSensor<T> : IAISensor
    {
        protected readonly Func<GameSystems, Actor, IEnumerable<T>> Calculate;
        protected Func<GameSystems, Actor, T, bool> ShouldRaiseAlert { get; private set; }

        public IList<T> Values { get; private set; }
        public IList<T> AlertingValues { get; private set; }
        public bool RaisingAlert { get; private set; }

        public AiSensor(Func<GameSystems, Actor, IEnumerable<T>> calculate)
        {
            Calculate = calculate;
            ShouldRaiseAlert = (_, __, ___) => false;
        }

        public void Update(GameSystems sys, Actor a)
        {
            Values = Calculate(sys, a)
                .ToArray();
            AlertingValues = Values
                .Where(x => ShouldRaiseAlert(sys, a, x))
                .ToArray();
            RaisingAlert = AlertingValues.Count > 0;
        }

        public void ConfigureAlert(Func<GameSystems, Actor, T, bool> match, bool appendLogic = false)
        {
            if (!appendLogic)
            {
                ShouldRaiseAlert = match;
            }
            else
            {
                var shouldRaiseAlert = ShouldRaiseAlert;
                ShouldRaiseAlert = (s, a, x) => shouldRaiseAlert(s, a, x) && match(s, a, x);
            }
        }
    }
}
