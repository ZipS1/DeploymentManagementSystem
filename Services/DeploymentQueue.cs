using System.Threading.Channels;

namespace DeploymentManagementSystem.Services
{
    public record DeploymentRequest(int TaskId, string UserPAT);

    public interface IDeploymentQueue
    {
        ValueTask QueueDeploymentAsync(DeploymentRequest request);
        ValueTask<DeploymentRequest> DequeueAsync(CancellationToken cancellationToken);
    }

    public class DeploymentQueue : IDeploymentQueue
    {
        private readonly Channel<DeploymentRequest> _queue;
        private const int MAX_QUEUE_CAPACITY = 100;

        public DeploymentQueue(int capacity = MAX_QUEUE_CAPACITY)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<DeploymentRequest>(options);
        }

        public async ValueTask QueueDeploymentAsync(DeploymentRequest request)
        {
            await _queue.Writer.WriteAsync(request);
        }

        public async ValueTask<DeploymentRequest> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}

