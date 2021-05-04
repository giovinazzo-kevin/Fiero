using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Fiero.Core
{
    public abstract class Entity
    {
        internal Func<Entity, int, bool> _refresh;

        public int Id { get; internal set; }

        public bool TryRefresh(int newId)
        {
            return _refresh(this, newId);
        }
    }
}
