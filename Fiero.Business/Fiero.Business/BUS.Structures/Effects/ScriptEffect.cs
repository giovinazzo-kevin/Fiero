using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Fiero.Core;
using System;
using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{

    /// <summary>
    /// Script effects can be applied to:
    /// - Entities:
    ///     - The effect is applied to the entity when the effect starts, and is removed when the effect ends.
    /// </summary>
    public class ScriptEffect : Effect
    {
        public readonly Script Script;
        public readonly string Description;

        protected static readonly Dictionary<Signature, Func<ScriptEffect, GameSystems, Subscription>> CachedRoutes = GetRoutes();

        public ScriptEffect(Script script, string description = null)
        {
            Script = script;
            Description = description ?? string.Empty;
        }

        public override EffectName Name => EffectName.Script;
        public override string DisplayName => Script.Info.Name;
        public override string DisplayDescription => Description;

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            /* Ergo scripts can subscribe to Fiero events via the subscribe/2 directive.
               All the directive does is prepare a list for this method, which is
               called whenever an effect that is tied to a script resolves. The list
               contains the signatures of the events that the script is handling.

               By wiring each event to a call to the script's solver, we can interpret
               the result of that call as the EventResult to pass to the owning system.
            */

            foreach (var sig in Script.ScriptProperties.SubscribedEvents)
            {
                if (CachedRoutes.TryGetValue(sig, out var sub))
                {
                    yield return sub(this, systems);
                }
                else
                {
                    // TODO: Warn script?
                }
            }
        }

        /// <summary>
        /// Maps every SystemRequest and SystemEvent defined in all systems to a wrapper that
        /// calls an Ergo script automatically and parses its result as an EventResult for Fiero,
        /// returning a subscription that will be disposed when this effect ends.
        /// </summary>
        /// <returns>All routes indexed by signature.</returns>
        public static Dictionary<Signature, Func<ScriptEffect, GameSystems, Subscription>> GetRoutes()
        {
            var finalDict = new Dictionary<Signature, Func<ScriptEffect, GameSystems, Subscription>>();
            foreach (var (sys, field, isReq) in MetaSystem.GetSystemEventFields())
            {
                var sysName = new Atom(sys.Name.Replace("System", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .ToErgoCase());
                if (isReq)
                {
                    var reqName = new Atom(field.Name.Replace("Request", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .ToErgoCase());
                    var reqType = field.FieldType.GetGenericArguments()[1];
                    finalDict.Add(new(reqName, 1, sysName, default), (self, systems) =>
                    {
                        return ((ISystemRequest)field.GetValue(sys.GetValue(systems)))
                            .SubscribeResponse(evt => Respond(self, evt, reqType, reqName, sysName));
                    });
                }
                else
                {
                    var evtName = new Atom(field.Name.Replace("Event", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .ToErgoCase());
                    var evtType = field.FieldType.GetGenericArguments()[1];
                    finalDict.Add(new(evtName, 1, sysName, default), (self, systems) =>
                    {
                        return ((ISystemEvent)field.GetValue(sys.GetValue(systems)))
                            .SubscribeHandler(evt => Respond(self, evt, evtType, evtName, sysName));
                    });
                }
            }
            return finalDict;


            static EventResult Respond(ScriptEffect self, object evt, Type type, Atom evtName, Atom sysName)
            {
                var term = TermMarshall.ToTerm(evt, type, mode: TermMarshalling.Named);
                // Qualify term with module so that the declaration needs to match, e.g. action:actor_turn_started/1
                var query = new Query(((ITerm)new Complex(evtName, term)).Qualified(sysName));
                try
                {
                    // TODO: Figure out a way for scripts to return complex EventResults?
                    foreach (var _ in self.Script.Solve(query))
                    {
                        if (self.Script.ScriptProperties.LastError != null)
                            return false;
                    }
                    return true;
                }
                catch (ErgoException ex)
                {
                    // TODO: Log to the in-game console
                    Console.WriteLine(ex);
                    return false;
                }
            }
        }
    }
}
