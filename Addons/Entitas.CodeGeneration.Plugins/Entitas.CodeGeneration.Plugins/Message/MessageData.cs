using DesperateDevs.CodeGeneration;
using DesperateDevs.Utils;

namespace Entitas.CodeGeneration.Plugins
{
    public class MessageData : CodeGeneratorData {
    
        public MessageData() {
        }

        public MessageData(CodeGeneratorData data) : base(data) {
        }
        
    }
    
    public static class MessageDataExtension {

        public static string ToMessageName(this string fullTypeName, bool ignoreNamespaces) {
            return ignoreNamespaces
                ? fullTypeName.ShortTypeName().RemoveMessageSuffix()
                : fullTypeName.RemoveDots().RemoveMessageSuffix();
        }
    }
}