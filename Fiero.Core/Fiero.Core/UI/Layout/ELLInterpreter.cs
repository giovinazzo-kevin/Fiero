using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Fiero.Core.Ergo;
using System.Runtime.CompilerServices;

namespace Fiero.Core;

public sealed class UnificationDictionary<T>(Func<T, SubstitutionMap, T> substitute)
{
    public readonly record struct UnifiedEntry(ITerm OriginalKey, T OriginalValue, T SubstitutedValue, SubstitutionMap Substitutions);
    
    private readonly List<KeyValuePair<ITerm, T>> _kvps = [];
    public IEnumerable<KeyValuePair<ITerm, T>> KeyValuePairs => _kvps;
    public IEnumerable<UnifiedEntry> this[ITerm term] => Get(term);
    public IEnumerable<UnifiedEntry> Get(ITerm term)
    {
        foreach(var kvp in _kvps)
        {
            if(kvp.Key.Unify(term).TryGetValue(out var subs))
            {
                var substitutedValue = substitute(kvp.Value, subs);
                yield return new(kvp.Key, kvp.Value, substitutedValue, subs);
            }
        }
    }


    public bool TryGetSubstitutedValue(ITerm term, out T ret)
    {
        foreach (var v in Get(term))
        {
            ret = v.SubstitutedValue;
            return true;
        }
        ret = default;
        return false;
    }

    public void Add(ITerm term, T value)
    {
        _kvps.Add(new(term, value));
    }
}

public static class ELLInterpreter
{

    [Term(Functor = "</>", Marshalling = TermMarshalling.Positional)]
    private readonly record struct Tag(ITerm Head, ITerm Body);

    [Term(Marshalling = TermMarshalling.Named)]
    private readonly record struct TagProperties(string Id, string Class, float Size, bool Px);
    private readonly record struct Instruction(Op Op, ITerm Functor, Dict Properties);
    private enum Op
    {
        PushTag,
        PopTag
    }
    private const string Row = "row";
    private const string Col = "col";
    private const string GlobalStyle = "_";

    private static readonly Atom Functor = new("</>");
    private static Hook Hook_GetComponentDefinitions
        => new(new(Functor, 2, default, default));
    private static Hook Hook_GetStyle
        => new(new(new("style"), 1, default, default));

    public static Dictionary<string, Dict> GetStyles(ErgoVM vm)
    {
        var ret = new Dictionary<string, Dict>();
        var var_x = new Variable("X");
        foreach (var sol in Hook_GetStyle.CallInteractive(vm, var_x))
        {
            if (sol[var_x] is not Dict dict)
                continue;
            var key = GlobalStyle;
            if (dict.Functor.TryGetA(out var atom))
                key = atom.Explain(false).ToCSharpCase();
            if (ret.TryGetValue(key, out var existingDict))
                ret[key] = dict.Merge(existingDict);
            else ret[key] = dict;
        }
        return ret;
    }

    public static Dictionary<string, Func<LayoutGrid>> GetComponentDefinitions(ErgoVM vm, Dictionary<string, Func<UIControl>> resolveDict)
    {
        var instructionCache = new UnificationDictionary<Instruction[]>((instructions, subs) => instructions
            .Select(i => i with { Properties = (Dict)i.Properties.Substitute(subs) })
            .ToArray());
        var styles = GetStyles(vm);
        var var_componentHead = new Variable("Head");
        var var_componentBody = new Variable("Body");
        foreach (var sol in Hook_GetComponentDefinitions.CallInteractive(vm, var_componentHead, var_componentBody))
        {
            var instructions = GetTags(sol[var_componentBody])
                .SelectMany(ProcessTag)
                .ToArray();
            var key = sol[var_componentHead];
            instructionCache.Add(key, instructions);
        }
        var componentCache = new Dictionary<string, Func<LayoutGrid>>();
        foreach (var (key, instructions) in instructionCache.KeyValuePairs)
        {
            if (key is not Atom atom)
                continue;
            componentCache.Add(atom.Explain(false), () =>
            {
                var stack = new Stack<LayoutGrid>();
                stack.Push(new LayoutGrid(LayoutPoint.RelativeOne, new()));
                ProcessInstructions(instructions);
                Debug.Assert(stack.Count == 1);
                return stack.Pop();
                void ProcessInstructions(Instruction[] instructions)
                {
                    foreach (var instr in instructions)
                    {
                        instr.Properties.Match(out TagProperties props);
                        props = props with
                        {
                            Id = props.Id ?? string.Empty,
                            Class = props.Class ?? string.Empty,
                            Size = props.Size == 0 && !props.Px ? 1 : props.Size
                        };
                        switch (instr.Op)
                        {
                            case Op.PushTag when instr.Functor is Atom { Value: Row }:
                                stack.Push(stack.Peek().Row(h: props.Size, px: props.Px, @class: props.Class, id: props.Id));
                                break;
                            case Op.PushTag when instr.Functor is Atom { Value: Col }:
                                stack.Push(stack.Peek().Col(w: props.Size, px: props.Px, @class: props.Class, id: props.Id));
                                break;
                            case Op.PushTag when instructionCache.TryGetSubstitutedValue(instr.Functor, out var componentDef):
                                ProcessInstructions(componentDef);
                                stack.Push(null); // will be popped by the next instruction
                                break;
                            case Op.PushTag when resolveDict.TryGetValue(instr.Functor.Explain(false).ToCSharpCase(), out var resolve):
                                var control = resolve();
                                var customProps = instr.Properties;
                                foreach (var styleName in (string[])[GlobalStyle, instr.Functor.Explain(false)])
                                {
                                    if (!styles.TryGetValue(styleName, out var style))
                                        continue;
                                    customProps = customProps.Merge(style);
                                }
                                var hooks = new List<DisposableHook>();
                                foreach (var prop in control.Properties)
                                {
                                    var key = new Atom(prop.Name.ToErgoCase());
                                    if (customProps.Dictionary.TryGetValue(key, out var value))
                                    {
                                        prop.Value = TermMarshall.FromTerm(value, prop.PropertyType);
                                    }
                                    // Create event handlers for property value change events
                                    //var valueChanged = prop.GetType().GetEvent(nameof(UIControlProperty<Atom>.ValueChanged), BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
                                    ////valueChanged.EventHandlerType
                                    //var hook_valueChanged = Hook.Marshall(valueChanged, prop, functor: new("value_changed"), eventModule)(vm);
                                    //hooks.Add(hook_valueChanged);
                                }
                                foreach (var evt in control.Events)
                                {
                                    var functor = new Atom(evt.Name.ToErgoCase());
                                    if (!instr.Properties.Dictionary.TryGetValue(functor, out var handler))
                                        continue;
                                    if (handler is not Atom handlerFunctor)
                                        continue;
                                    var hook_evt = Hook.MarshallEvent(evt, control, handlerFunctor, vm.KB.Scope.Entry)(vm);
                                    hooks.Add(hook_evt);
                                }
                                stack.Push(stack.Peek().Cell(control));
                                break;
                            case Op.PushTag:
                                stack.Push(stack.Peek());
                                break;
                            case Op.PopTag:
                                stack.Pop();
                                break;
                        }
                    }
                }
            });
        }
        return componentCache;

        IEnumerable<Instruction> ProcessTag(Tag tag)
        {
            var pushHead = ParseNode(Op.PushTag, tag.Head);
            yield return pushHead;
            foreach (var instr in GetTags(tag.Body).SelectMany(ProcessTag))
                yield return instr;
            yield return pushHead with { Op = Op.PopTag };

            Instruction ParseNode(Op op, ITerm node)
            {
                var (functor, properties) = node switch
                {
                    Atom a => (a, new Dict(a, WellKnown.Literals.Discard)),
                    Complex c => ((ITerm)c, new Dict(c.Functor, WellKnown.Literals.Discard)),
                    Dict d when d.Functor.TryGetA(out var f) => (f, d),
                    _ => throw new NotSupportedException(node.Explain(false))
                };
                return new(op, functor, properties);
            }

        }
        IEnumerable<Tag> GetTags(ITerm term)
        {
            if (term.Equals(WellKnown.Literals.EmptyCommaList))
                return [];
            if (term is NTuple tup)
                return tup.Contents.SelectMany(GetTags);
            if (term.Match(out Tag tag, matchFunctor: true))
                return [tag];
            return [new Tag(term, WellKnown.Literals.EmptyCommaList)];
        }
    }
}
