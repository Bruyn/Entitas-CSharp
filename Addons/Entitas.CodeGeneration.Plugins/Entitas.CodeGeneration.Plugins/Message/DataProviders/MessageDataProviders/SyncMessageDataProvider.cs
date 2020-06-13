using System;
using System.Collections.Generic;
using System.Linq;
using DesperateDevs.Serialization;
using Entitas.CodeGeneration.Attributes;

namespace Entitas.CodeGeneration.Plugins
{
    public class SyncMessageDataProvider : IMessageDataProvider, IConfigurable
    {
        public Dictionary<string, string> defaultProperties
        {
            get { return _contextNamesConfig.defaultProperties; }
        }

        readonly ContextNamesConfig _contextNamesConfig = new ContextNamesConfig();

        public void Configure(Preferences preferences)
        {
            _contextNamesConfig.Configure(preferences);
        }

        public void Provide(Type type, MessageData data)
        {
            data.SetSyncType(GetSyncTypeName(type));
        }

        private string GetSyncTypeName(Type type)
        {
            bool isSync = type.GetCustomAttributes(typeof(SyncAttribute), false).Length > 0;
            bool isAuthority = type.GetCustomAttributes(typeof(AuthorityAttribute), false).Length > 0;
            bool isClient = type.GetCustomAttributes(typeof(ClientAttribute), false).Length > 0;

            if (isSync)
            {
                return "SyncType.Sync";
            }
            else if (isAuthority && isClient)
            {
                return "SyncType.AuthorityAndClient";
            }
            else if (isAuthority)
            {
                return "SyncType.Authority";
            }
            else if (isClient)
            {
                return "SyncType.Client";
            }

            return "SyncType.None";
        }
    }

    public static class SyncMessageDataExtension {
    
        public const string MESSAGE_SYNC_TYPE = "Message.SyncTypeName";
    
        public static string GetSyncTypeName(this MessageData data) {
            return (string)data[MESSAGE_SYNC_TYPE];
        }
    
        public static void SetSyncType(this MessageData data, string syncTypeName) {
            data[MESSAGE_SYNC_TYPE] = syncTypeName;
        }
    }
}