namespace CouchGraphQl.GraphQl.Shared;

using System.Reflection;

using HotChocolate.Types.Descriptors;

using Serilog;

public class LoqQueryAttribute : ObjectFieldDescriptorAttribute
{
    private const string MessageTemplate = "Query: {Query}";

    protected override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
    {
        descriptor.Use(next => async middlewareContext =>
        {
            await next(middlewareContext).ConfigureAwait(false);

            if (middlewareContext.Result is IQueryable queryable)
            {
                Log.Debug(LoqQueryAttribute.MessageTemplate, queryable.Expression.ToString());
            }
        });
    }
}