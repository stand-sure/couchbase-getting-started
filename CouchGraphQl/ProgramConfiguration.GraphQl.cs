namespace CouchGraphQl;

using System.Reflection;

using CouchGraphQl.GraphQl.Airline;
using CouchGraphQl.GraphQl.Bucket;
using CouchGraphQl.GraphQl.Collection;
using CouchGraphQl.GraphQl.Scope;
using CouchGraphQl.GraphQl.Shared;

using HotChocolate.Execution.Configuration;
using HotChocolate.Execution.Options;
using HotChocolate.Execution.Pipeline.Complexity;
using HotChocolate.Types.Pagination;

using Serilog;

internal static partial class ProgramConfiguration
{
    private static void AddGraphQl(this IServiceCollection services)
    {
        services.AddGraphQLServer()
            .ModifyOptions(ConfigureSchemaOptions)
            .AddQueries()
            .AddTypes()
            .AddMutations()
            .AddGlobalObjectIdentification()
            .AddQueryFieldToMutationPayloads()
            .ModifyRequestOptions(ConfigureRequestExecutorOptions)
            .SetPagingOptions(GetPagingOptions())
            .AddSorting()
            .AddFiltering()
            .AddErrorFilter<DetailRemovingErrorFilter>()
            .AllowIntrospection(true);
    }

    private static IRequestExecutorBuilder AddMutations(this IRequestExecutorBuilder builder)
    {
        builder.AddMutationType(descriptor => descriptor.Name(OperationTypeNames.Mutation));

        // poc to add all mutations
        IEnumerable<Type> mutations = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.GetCustomAttribute<ExtendObjectTypeAttribute>()?.Name == OperationTypeNames.Mutation);

        foreach (Type mutation in mutations)
        {
            builder.AddTypeExtension(mutation);
        }
        
        return builder;
    }

    private static IRequestExecutorBuilder AddQueries(this IRequestExecutorBuilder builder)
    {
        return builder.AddQueryType(descriptor => descriptor.Name(OperationTypeNames.Query))
            .AddTypeExtension<AirlineQueries>()
            .AddTypeExtension<BucketQueries>()
            .AddTypeExtension<ScopeQueries>()
            .AddTypeExtension<CollectionQueries>();
    }

    private static IRequestExecutorBuilder AddTypes(this IRequestExecutorBuilder builder)
    {
        return builder.AddType<AirlineType>();
    }

    private static int ComplexityCalculator(ComplexityContext context)
    {
        int cost = context.Complexity + context.ChildComplexity;

        Log.Debug(ProgramConfiguration.MessageTemplate, $"Cost: {context.Selection.Name} {cost}");

        return cost;
    }

    private static void ConfigureRequestExecutorOptions(IServiceProvider provider, RequestExecutorOptions options)
    {
        options.IncludeExceptionDetails = true;
        options.Complexity.Enable = true;
        options.Complexity.ApplyDefaults = true;

        options.Complexity.Calculation = ComplexityCalculator;
    }

    private static void ConfigureSchemaOptions(SchemaOptions options)
    {
        options.SortFieldsByName = true;
        options.EnableFlagEnums = true;
        options.RemoveUnreachableTypes = true;
    }

    private static PagingOptions GetPagingOptions()
    {
        return new PagingOptions
        {
            MaxPageSize = 100,
            //IncludeTotalCount = true, // this blows up Couchbase
        };
    }
}