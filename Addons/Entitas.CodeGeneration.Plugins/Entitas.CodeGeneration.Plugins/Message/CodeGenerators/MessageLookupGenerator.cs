using System.Collections.Generic;
using System.Linq;
using System.IO;
using DesperateDevs.CodeGeneration;

namespace Entitas.CodeGeneration.Plugins {

    public class MessageLookupGenerator : AbstractGenerator {

        public override string name { get { return "Message (Lookup)"; } }

        const string TEMPLATE =
            @"public static class ${Lookup} {

${MessageConstantsList}

${totalMessagesConstant}

    public static readonly string[] MessageNames = {
${MessageNamesList}
    };

    public static readonly System.Type[] MessageTypes = {
${MessageTypesList}
    };
}
";

        const string MESSAGE_CONSTANT_TEMPLATE = @"    public const int ${MessageName} = ${Index};";
        const string TOTAL_MESSAGES_CONSTANT_TEMPLATE = @"    public const int TotalMessages = ${totalMessages};";
        const string MESSAGE_NAME_TEMPLATE = @"        ""${MessageName}""";
        const string MESSAGE_TYPE_TEMPLATE = @"        typeof(${MessageType})";

        public override CodeGenFile[] Generate(CodeGeneratorData[] data) {
            var lookups = generateLookups(data
                .OfType<MessageData>()
                .ToArray());

            var existingFileNames = new HashSet<string>(lookups.Select(file => file.fileName));

            var emptyLookups = generateEmptyLookups(data
                    .OfType<ContextData>()
                    .ToArray())
                .Where(file => !existingFileNames.Contains(file.fileName))
                .ToArray();

            return lookups.Concat(emptyLookups).ToArray();
        }

        CodeGenFile[] generateEmptyLookups(ContextData[] data) {
            var emptyData = new MessageData[0];
            return data
                .Select(d => generateMessagesLookupClass(d.GetContextName(), emptyData))
                .ToArray();
        }

        CodeGenFile[] generateLookups(MessageData[] data) {
            var contextNameToMessageData = data
                .Aggregate(new Dictionary<string, List<MessageData>>(), (dict, d) => {
                    var contextNames = d.GetContextNames();
                    foreach (var contextName in contextNames) {
                        if (!dict.ContainsKey(contextName)) {
                            dict.Add(contextName, new List<MessageData>());
                        }

                        dict[contextName].Add(d);
                    }

                    return dict;
                });

            foreach (var key in contextNameToMessageData.Keys.ToArray()) {
                contextNameToMessageData[key] = contextNameToMessageData[key]
                    .OrderBy(d => d.GetTypeName())
                    .ToList();
            }

            return contextNameToMessageData
                .Select(kv => generateMessagesLookupClass(kv.Key, kv.Value.ToArray()))
                .ToArray();
        }

        CodeGenFile generateMessagesLookupClass(string contextName, MessageData[] data) {
            var MessageConstantsList = string.Join("\n", data
                .Select((d, index) => MESSAGE_CONSTANT_TEMPLATE
                    .Replace("${MessageName}", d.MessageName())
                    .Replace("${Index}", index.ToString())).ToArray());

            var totalMessagesConstant = TOTAL_MESSAGES_CONSTANT_TEMPLATE
                .Replace("${totalMessages}", data.Length.ToString());

            var MessageNamesList = string.Join(",\n", data
                .Select(d => MESSAGE_NAME_TEMPLATE
                    .Replace("${MessageName}", d.MessageName())
                ).ToArray());

            var MessageTypesList = string.Join(",\n", data
                .Select(d => MESSAGE_TYPE_TEMPLATE
                    .Replace("${MessageType}", d.GetTypeName())
                ).ToArray());

            var fileContent = TEMPLATE
                .Replace("${Lookup}", contextName + CodeGeneratorExtentions.LOOKUP_MESSAGE)
                .Replace("${MessageConstantsList}", MessageConstantsList)
                .Replace("${totalMessagesConstant}", totalMessagesConstant)
                .Replace("${MessageNamesList}", MessageNamesList)
                .Replace("${MessageTypesList}", MessageTypesList);

            return new CodeGenFile(
                contextName + Path.DirectorySeparatorChar +
                contextName + "MessagesLookup.cs",
                fileContent,
                GetType().FullName
            );
        }
    }
}
