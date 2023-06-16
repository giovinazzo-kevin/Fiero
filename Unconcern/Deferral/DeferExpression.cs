using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Unconcern.Deferral
{
    public class DeferExpression : IDeferExpression
    {
        public TimeSpan Delay { get; private set; }
        public TimeSpan Duration { get; private set; }
        public bool WaitDuration { get; private set; }
        public bool PreciseTimer { get; private set; }

        protected List<IDeferExpression> SiblingsList { get; set; }
        protected List<IDeferExpression> ChildrenList { get; set; }
        protected List<IDeferExpression> FallbacksList { get; set; }
        protected List<Func<CancellationToken, Task>> TasksList { get; set; }

        public IEnumerable<IDeferExpression> Siblings => SiblingsList;
        public IEnumerable<IDeferExpression> Children => ChildrenList;
        public IEnumerable<IDeferExpression> Fallbacks => FallbacksList;
        public IEnumerable<Func<CancellationToken, Task>> Tasks => TasksList;

        internal DeferExpression(
            IEnumerable<Func<CancellationToken, Task>> tasks,
            TimeSpan start,
            TimeSpan duration,
            IEnumerable<IDeferExpression> siblings,
            IEnumerable<IDeferExpression> children,
            IEnumerable<IDeferExpression> fallbacks,
            bool waitDuration,
            bool tightLoop)
        {
            Delay = start;
            Duration = duration;
            WaitDuration = waitDuration;
            PreciseTimer = tightLoop;
            SiblingsList = new List<IDeferExpression>(siblings ?? Enumerable.Empty<IDeferExpression>());
            ChildrenList = new List<IDeferExpression>(children ?? Enumerable.Empty<IDeferExpression>());
            FallbacksList = new List<IDeferExpression>(fallbacks ?? Enumerable.Empty<IDeferExpression>());
            TasksList = new List<Func<CancellationToken, Task>>(tasks);
        }
    }
}
