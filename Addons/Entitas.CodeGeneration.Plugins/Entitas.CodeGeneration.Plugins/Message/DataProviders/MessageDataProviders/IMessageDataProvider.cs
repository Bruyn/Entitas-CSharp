using System;

namespace Entitas.CodeGeneration.Plugins {

    public interface IMessageDataProvider {

        void Provide(Type type, MessageData data);
    }
}