using System;
using System.Collections.Generic;
using System.Linq;
using DesperateDevs.CodeGeneration;
using DesperateDevs.CodeGeneration.CodeGenerator;
using DesperateDevs.Serialization;
using DesperateDevs.Utils;
using Entitas.CodeGeneration.Attributes;

namespace Entitas.CodeGeneration.Plugins
{
    public class MessageDataProvider : IDataProvider, IConfigurable, ICachable, IDoctor {
        
        public string name { get { return "Message"; } }
        public int priority { get { return 0; } }
        public bool runInDryMode { get { return true; } }

        public Dictionary<string, string> defaultProperties {
            get {
                var dataProviderProperties = _dataProviders
                    .OfType<IConfigurable>()
                    .Select(i => i.defaultProperties)
                    .ToArray();

                return _assembliesConfig.defaultProperties
                    .Merge(_contextsMessageDataProvider.defaultProperties)
                    .Merge(_ignoreNamespacesConfig.defaultProperties)
                    .Merge(dataProviderProperties);
            }
        }

        public Dictionary<string, object> objectCache { get; set; }

        readonly CodeGeneratorConfig _codeGeneratorConfig = new CodeGeneratorConfig();
        readonly AssembliesConfig _assembliesConfig = new AssembliesConfig();
        readonly ContextsMessageDataProvider _contextsMessageDataProvider = new ContextsMessageDataProvider();
        readonly IgnoreNamespacesConfig _ignoreNamespacesConfig = new IgnoreNamespacesConfig();

        static IMessageDataProvider[] getMessageDataProviders() {
            return new IMessageDataProvider[] {
                new ContextsMessageDataProvider(),
                new MessageTypeMessageDataProvider(),
                new MemberDataMessageDataProvider(),
            };
        }

        readonly Type[] _types;
        readonly IMessageDataProvider[] _dataProviders;

        public MessageDataProvider() : this(null) {
        }

        public MessageDataProvider(Type[] types) : this(types, getMessageDataProviders()) {
        }

        protected MessageDataProvider(Type[] types, IMessageDataProvider[] dataProviders) {
            _types = types;
            _dataProviders = dataProviders;
        }

        public void Configure(Preferences preferences) {
            _codeGeneratorConfig.Configure(preferences);
            _assembliesConfig.Configure(preferences);
            foreach (var dataProvider in _dataProviders.OfType<IConfigurable>()) {
                dataProvider.Configure(preferences);
            }

            _contextsMessageDataProvider.Configure(preferences);
            _ignoreNamespacesConfig.Configure(preferences);
        }

        public CodeGeneratorData[] GetData() {
            var types = _types ?? PluginUtil
                            .GetCachedAssemblyResolver(objectCache, _assembliesConfig.assemblies, _codeGeneratorConfig.searchPaths)
                            .GetTypes();

            var dataFromMessages = types
                .Where(type => type.ImplementsInterface<IMessage>())
                .Where(type => !type.IsAbstract)
                .Select(createDataForMessage)
                .ToArray();

            // var dataFromNonMessages = types
            //     .Where(type => !type.ImplementsInterface<IMessage>())
            //     .Where(type => !type.IsGenericType)
            //     .Where(hasContexts)
            //     .SelectMany(createDataForNonMessage)
            //     .ToArray();
            //
            // var mergedData = merge(dataFromNonMessages, dataFromMessages);
            //
            // var dataFromEvents = mergedData
            //     .Where(data => data.IsEvent())
            //     .SelectMany(createDataForEvents)
            //     .ToArray();

            //MessageData[] result = merge(dataFromEvents, mergedData);
            return dataFromMessages;
        }

        // MessageData[] merge(MessageData[] prioData, MessageData[] redundantData) {
        //     var lookup = prioData.ToLookup(data => data.GetTypeName());
        //     return redundantData
        //         .Where(data => !lookup.Contains(data.GetTypeName()))
        //         .Concat(prioData)
        //         .ToArray();
        // }

        MessageData createDataForMessage(Type type) {
            var data = new MessageData();
            foreach (var provider in _dataProviders) {
                provider.Provide(type, data);
            }

            return data;
        }

        // MessageData[] createDataForNonMessage(Type type) {
        //     return getMessageNames(type)
        //         .Select(MessageName => {
        //             var data = createDataForMessage(type);
        //             data.SetTypeName(MessageName.AddMessageSuffix());
        //             data.SetMemberData(new[] {
        //                 new MemberData(type.ToCompilableString(), "value")
        //             });
        //
        //             return data;
        //         }).ToArray();
        // }

        // MessageData[] createDataForEvents(MessageData data) {
        //     return data.GetContextNames()
        //         .SelectMany(contextName =>
        //             data.GetEventData().Select(eventData => {
        //                 var dataForEvent = new MessageData(data);
        //                 dataForEvent.IsEvent(false);
        //                 dataForEvent.IsUnique(false);
        //                 dataForEvent.ShouldGenerateMessage(false);
        //                 var eventMessageName = data.EventMessageName(eventData);
        //                 var eventTypeSuffix = eventData.GetEventTypeSuffix();
        //                 var optionalContextName = dataForEvent.GetContextNames().Length > 1 ? contextName : string.Empty;
        //                 var listenerMessageName = optionalContextName + eventMessageName + eventTypeSuffix.AddListenerSuffix();
        //                 dataForEvent.SetTypeName(listenerMessageName.AddMessageSuffix());
        //                 dataForEvent.SetMemberData(new[] {
        //                     new MemberData("System.Collections.Generic.List<I" + listenerMessageName + ">", "value")
        //                 });
        //                 dataForEvent.SetContextNames(new[] { contextName });
        //                 return dataForEvent;
        //             }).ToArray()
        //         ).ToArray();
        // }

        // bool hasContexts(Type type) {
        //     return _contextsMessageDataProvider.GetContextNames(type).Length != 0;
        // }
        //
        // string[] getMessageNames(Type type) {
        //     var attr = Attribute
        //         .GetCustomAttributes(type)
        //         .OfType<MessageNameAttribute>()
        //         .SingleOrDefault();
        //
        //     if (attr == null) {
        //         return new[] { type.ToCompilableString().ShortTypeName().AddMessageSuffix() };
        //     }
        //
        //     return attr.MessageNames;
        // }

        public Diagnosis Diagnose() {
            var isStandalone = AppDomain.CurrentDomain
                .GetAllTypes()
                .Any(type => type.FullName == "DesperateDevs.CodeGeneration.CodeGenerator.CLI.Program");

            if (isStandalone) {
                var typeName = typeof(MessageDataProvider).FullName;
                if (_codeGeneratorConfig.dataProviders.Contains(typeName)) {
                    return new Diagnosis(
                        typeName + " loads and reflects " + string.Join(", ", _assembliesConfig.assemblies) + " and therefore doesn't support server mode!",
                        "Don't use the code generator in server mode with " + typeName,
                        DiagnosisSeverity.Hint
                    );
                }
            }

            return Diagnosis.Healthy;
        }

        public bool Fix() {
            return false;
        }
    }
}