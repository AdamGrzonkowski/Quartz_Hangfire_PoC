namespace Hangfire.API.BackgroundJobs
{
    [DisableConcurrentExecution(timeoutInSeconds: 1)] // timeoutInSeconds = timeout after which each instance which is waiting for the Lock's release will throw SqlServerDistributedLockException
    [AutomaticRetry(Attempts = 3)] // retry pattern applied to entire job
    public class SimpleJob
    {
        private readonly ILogger<SimpleJob> _logger;

        public SimpleJob(ILogger<SimpleJob> logger)
        {
            _logger = logger;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(SimpleJob)} execution started on threadId: '{Thread.CurrentThread.ManagedThreadId}' !");

            // START - put Job's body here
            await DoSomeBusinessLogicOperation(cancellationToken);

            _logger.LogInformation($"{nameof(SimpleJob)} has been executed!");

            await Task.CompletedTask;
            // END
        }

        private async Task DoSomeBusinessLogicOperation(CancellationToken cancellationToken)
        {
            for (int i = 0; i < 10; i++)
            {
                if (cancellationToken.IsCancellationRequested) // check periodically whether cancellation of Job was requested - ideally should be placed in some loop or at least after some heavy time-consuming operations
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken); // database calls, calculations or whatever goes here
            }
        }
    }
}