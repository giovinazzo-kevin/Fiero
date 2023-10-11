using System;
using System.Linq;
using System.Threading.Tasks;

namespace Unconcern.Common
{
    public class EventBus
    {
        public static readonly EventBus Default = new();

        public readonly struct Message
        {
            public readonly DateTime Timestamp;
            public readonly string Sender;
            public readonly string[] Recipients;
            public readonly Type Type;
            public readonly object Content;

            internal Message(DateTime ts, Type t, object m, string h, string[] hubs)
            {
                Timestamp = ts;
                Type = t;
                Content = m;
                Sender = h;
                Recipients = hubs;
            }

            internal Message FromHub(string sender)
            {
                return new Message(Timestamp, Type, Content, sender, Recipients);
            }

            internal Message ToRecipients(params string[] recipients)
            {
                return new Message(Timestamp, Type, Content, Sender, recipients);
            }
        }

        public readonly struct Message<T>
        {
            public readonly DateTime Timestamp;
            public readonly string Sender;
            public readonly string[] Recipients;
            public readonly T Content;

            internal Message(DateTime ts, T m, string h, string[] hubs)
            {
                Timestamp = ts;
                Content = m;
                Sender = h;
                Recipients = hubs;
            }

            public Message<U> WithContent<U>(U newContent)
            {
                return new Message<U>(Timestamp, newContent, Sender, Recipients);
            }

            public Message<T> From(string newSender)
            {
                return new Message<T>(Timestamp, Content, newSender, Recipients);
            }

            public Message<T> To(params string[] recipients)
            {
                if (recipients.Length == 0)
                    throw new ArgumentOutOfRangeException(nameof(recipients));
                return new Message<T>(Timestamp, Content, Sender, recipients);
            }

            public override string ToString()
                => $"[{Timestamp:O}] {Sender} TO {(Recipients.Length == 0 ? "ALL" : String.Join(", ", Recipients))}: {Content}";
        }


        protected event Action<Message> OnMessageSent = _ => { };

        public EventBus()
        {

        }

        public Subscription Register(Action<Message> a)
        {
            OnMessageSent += a;
            return new Subscription(new Action[] { () => { OnMessageSent -= a; } }, throwOnDoubleDispose: false);
        }

        public void Send<T>(T msg, string fromHub, params string[] toHubs)
            => Send(new(DateTime.Now, typeof(T), msg, fromHub, toHubs));

        /// <summary>
        /// Sends a message and waits for all handlers to complete synchronously.
        /// </summary>
        public void Send(Message m)
        {
            var handlers = OnMessageSent
                .GetInvocationList()
                .OfType<Action<Message>>();
            foreach (var handle in handlers)
            {
                handle(m);
            }
        }

        /// <summary>
        /// Sends a message and waits for all handlers to complete asynchronously.
        /// </summary>
        public Task SendAsync(Message m)
        {
            var handlers = OnMessageSent
                .GetInvocationList()
                .OfType<Action<Message>>();
            return Task.WhenAll(handlers.Select(handle =>
            {
                handle(m);
                return Task.CompletedTask;
            }));
        }

        public Sieve<T> Filter<T>(Func<Message<T>, bool> filter = null)
        {
            return new Sieve<T>(this, filter ?? (msg => true));
        }
    }
}