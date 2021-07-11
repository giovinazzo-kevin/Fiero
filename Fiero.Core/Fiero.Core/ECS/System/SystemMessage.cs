namespace Fiero.Core
{
    public readonly struct SystemMessage<TSystem, TArgs>
        where TSystem : EcsSystem
    {
        public readonly string Sender;
        public readonly TSystem System;
        public readonly TArgs Message;

        public SystemMessage(string sender, TSystem sys, TArgs msg)
        {
            Sender = sender;
            System = sys;
            Message = msg;
        }
    }
}
