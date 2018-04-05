using GraphQL.Server.Operation;

namespace GraphQL.Server.OperationFilters
{
    public interface IOperationFilter
    {
        OperationFilterType Type { get; }

        OperationValues Run(OperationValues operationValues);
    }
}
