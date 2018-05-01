using GraphQL.Server.Security;
using System;

namespace GraphQL.Server.Sample.Autorization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public CustomAuthorizeAttribute(params string[] claims) : base(claims)
        {
            
        }
    }
}