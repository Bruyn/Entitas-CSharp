using System.Collections.Generic;

namespace Entitas
{
    public abstract class MessageSystem<TEntity, TMessage> : IMessageSystem
        where TEntity : class, IEntity
        where TMessage : class, IMessage
    {
        private Dictionary<TEntity, List<TMessage>> _messagesCollector;
        private Dictionary<TEntity, List<TMessage>> _buffer;
        
        private readonly IGroup<TEntity> _group;

        protected MessageSystem(IContext<TEntity> context)
        {
            _messagesCollector = new Dictionary<TEntity, List<TMessage>>();
            _buffer = new Dictionary<TEntity, List<TMessage>>();
            
            context.OnMessageSend += updateMessages;
            _group = context.GetGroup(GetMatcher());
        }
        
        protected abstract IMatcher<TEntity> GetMatcher();
        
        protected abstract void Execute(TMessage message, TEntity entity);

        public void Execute()
        {
            var temp = _buffer;
            _buffer = _messagesCollector;
            _messagesCollector = temp;
            
            foreach (var pair in _buffer)
            {
                var entity = pair.Key;
                if (!_group.ContainsEntity(entity))
                {
                    continue;
                }
                
                foreach (var message in pair.Value)
                {
                    Execute(message, entity);
                }
            }
            
            _buffer.Clear();
        }
        
        void updateMessages(IEntity e, int index, IMessage m)
        {
            TMessage message = (m as TMessage);
            if (message == null) return;
            var entity = (TEntity) e;
            
            List<TMessage> messages;
            if (!_messagesCollector.TryGetValue(entity, out messages))
            {
                messages = new List<TMessage>();
                _messagesCollector.Add(entity, messages);
            }
            messages.Add(message);
        }
    }
}