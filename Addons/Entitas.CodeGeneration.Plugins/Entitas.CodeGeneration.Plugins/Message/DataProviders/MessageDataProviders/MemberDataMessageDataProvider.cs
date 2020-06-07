using System;
using System.Linq;
using DesperateDevs.Utils;

namespace Entitas.CodeGeneration.Plugins {

    public class MemberDataMessageDataProvider : IMessageDataProvider {

        public void Provide(Type type, MessageData data) {
            var memberData = type.GetPublicMemberInfos()
                .Select(info => new MemberData(
                    info.type.ToCompilableString(),
                    info.name))
                .ToArray();

            data.SetMemberData(memberData);
        }
    }

    public static class MemberInfosMessageDataExtension {

        public const string COMPONENT_MEMBER_DATA = "Message.MemberData";

        public static MemberData[] GetMemberData(this MessageData data) {
            return (MemberData[])data[COMPONENT_MEMBER_DATA];
        }

        public static void SetMemberData(this MessageData data, MemberData[] memberInfos) {
            data[COMPONENT_MEMBER_DATA] = memberInfos;
        }
    }
}
