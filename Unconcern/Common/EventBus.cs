using System;

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

            public Message<T> To(params string[] recipients)
            {
                if (recipients.Length == 0)
                    throw new ArgumentOutOfRangeException(nameof(recipients));
                return new Message<T>(Timestamp, Content, Sender, recipients);
            }
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
        {
            OnMessageSent.Invoke(new Message(DateTime.Now, typeof(T), msg, fromHub, toHubs));
        }

        public void Send(Message m)
        {
            OnMessageSent.Invoke(m);
        }

        public Sieve<T> Filter<T>(Func<Message<T>, bool> filter = null)
        {
            return new Sieve<T>(this, filter ?? (msg => true));
        }
    }
}