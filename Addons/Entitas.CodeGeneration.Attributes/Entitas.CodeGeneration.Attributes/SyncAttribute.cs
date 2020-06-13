using System;

namespace Entitas.CodeGeneration.Attributes
{
    public class SyncAttribute : Attribute
    {
    }

    public class AuthorityAttribute : Attribute
    {
    }

    public class ClientAttribute : Attribute
    {
    }
    
    public enum SyncType
    {
        None,
        Sync,
        AuthorityAndClient,
        Authority,
        Client
    }
}