global using Fiero.Core.Extensions;
global using Fiero.Core.Structures;
global using LightInject;
global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.Linq;
global using System.Text;
global using System.Threading;
global using System.Threading.Tasks;
global using ScriptDataRoutes = System.Collections.Generic.Dictionary<Fiero.Core.Script.DataHook, System.Func<Fiero.Core.Script, Unconcern.Common.Subscription>>;
global using ScriptEventRoutes = System.Collections.Generic.Dictionary<Fiero.Core.Script.EventHook, System.Func<Fiero.Core.Script, Unconcern.Common.Subscription>>;

namespace Fiero.Core;