using System.Threading.Tasks;

namespace GraphQL.Client.Transport
{
    public interface ITransport
    {
        Task<ITransportOutput> SendAsync(string operation, string variables);
    }
}