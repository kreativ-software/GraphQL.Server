using Sample.Service.Interface;

namespace Sample.Service.GraphQL
{
    public class SampleSchema : ISampleSchema
    {
        public ISampleMutations Mutations { get; private set; }
        public ISampleQueries Queries { get; private set; }

        public SampleSchema()
        {
            Mutations = new SampleMutations();
            Queries = new SampleQueries();
        }
    }
}
