using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;

namespace TerminalGateway.ApiService
{
    public sealed class GetMongoResourceLogsLifecycleHook(
     ResourceNotificationService resourceNotificationService,
     ResourceLoggerService resourceLoggerService,
     ILogger<GetMongoResourceLogsLifecycleHook> logger) : IDistributedApplicationLifecycleHook, IAsyncDisposable
    {
        private readonly CancellationTokenSource _shutdownCts = new();
        private Task? _getMongoResourceLogsTask;

        public Task AfterResourcesCreatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
        {
            this._getMongoResourceLogsTask = this.GetMongoResourceLogsAsync(this._shutdownCts.Token);
            return Task.CompletedTask;
        }

        private async Task GetMongoResourceLogsAsync(CancellationToken cancellationToken)
        {
            try
            {
                string? mongoResourceId = null;
                await foreach (var notification in resourceNotificationService.WatchAsync(cancellationToken))
                {
                    if (notification.Resource.Name == "mongo")
                    {
                        mongoResourceId = notification.ResourceId;
                        break;
                    }
                }

                if (mongoResourceId is not null)
                {
                    await foreach (var batch in resourceLoggerService.WatchAsync(mongoResourceId).WithCancellation(cancellationToken))
                    {
                        foreach (var logLine in batch)
                        {
                            logger.LogInformation("{MongoOutput}", logLine);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when the application is shutting down.
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred while getting the logs for the mongo resource");
            }
        }

        public async ValueTask DisposeAsync()
        {
            await this._shutdownCts.CancelAsync();

            if (this._getMongoResourceLogsTask is not null)
            {
                try
                {
                    await this._getMongoResourceLogsTask;
                }
                catch (OperationCanceledException)
                {
                }
            }

            this._shutdownCts.Dispose();
        }
    }
}
