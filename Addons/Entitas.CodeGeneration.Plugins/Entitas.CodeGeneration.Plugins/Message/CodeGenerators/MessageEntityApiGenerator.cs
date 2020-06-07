using System.IO;
using System.Linq;
using DesperateDevs.CodeGeneration;
using DesperateDevs.Utils;

namespace Entitas.CodeGeneration.Plugins {

    public class MessageEntityApiGenerator : AbstractGenerator {

        public override string name { get { return "Message (Entity API)"; } }

        const string STANDARD_TEMPLATE =
            @"public partial class ${EntityType} {

    public void Send${MessageName}(${newMethodParameters}) {
        var index = ${Index};
        var message = new ${MessageType}();
${memberAssignmentList}
        SendMessage(index, message);
    }
}
";

        public override CodeGenFile[] Generate(CodeGeneratorData[] data) {
            return data
                .OfType<MessageData>()
                //.Where(d => d.ShouldGenerateMethods())
                .SelectMany(generate)
                .ToArray();
        }

        CodeGenFile[] generate(MessageData data) {
            return data.GetContextNames()
                .Select(contextName => generate(contextName, data))
                .ToArray();
        }

        CodeGenFile generate(string contextName, MessageData data) {
            var template = STANDARD_TEMPLATE;
            
            var fileContent = template
                .Replace("${memberAssignmentList}", getMemberAssignmentList(data.GetMemberData()))
                .Replace(data, contextName);

            return new CodeGenFile(
                contextName + Path.DirectorySeparatorChar +
                "Messages" + Path.DirectorySeparatorChar +
                data.MessageNameWithContext(contextName).AddMessageSuffix() + ".cs",
                fileContent,
                GetType().FullName
            );
        }

        string getMemberAssignmentList(MemberData[] memberData) {
            return string.Join("\n", memberData
                .Select(info => "        message." + info.name + " = new" + info.name.UppercaseFirst() + ";")
                .ToArray()
            );
        }
    }
}
