namespace Sample.Service.Interface
{
    public interface ISampleSchema
    {
        ISampleMutations Mutations { get; }
        ISampleQueries Queries { get; }
    }
}
