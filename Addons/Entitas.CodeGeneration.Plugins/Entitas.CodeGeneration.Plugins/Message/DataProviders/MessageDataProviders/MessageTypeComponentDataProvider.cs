using System;
using DesperateDevs.Utils;

namespace Entitas.CodeGeneration.Plugins {

    public class MessageTypeMessageDataProvider : IMessageDataProvider {

        public void Provide(Type type, MessageData data) {
            data.SetTypeName(type.ToCompilableString());
        }
    }

    public static class MessageTypeMessageDataExtension {

        public const string Message_TYPE = "Message.TypeName";

        public static string GetTypeName(this MessageData data) {
            return (string)data[Message_TYPE];
        }

        public static void SetTypeName(this MessageData data, string fullTypeName) {
            data[Message_TYPE] = fullTypeName;
        }
    }
}
