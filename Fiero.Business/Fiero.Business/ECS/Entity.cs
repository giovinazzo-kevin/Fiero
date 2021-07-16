using Fiero.Core;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fiero.Business
{
    public class Entity : EcsEntity
    {
        [RequiredComponent]
        public InfoComponent Info { get; private set; }
    }
}
