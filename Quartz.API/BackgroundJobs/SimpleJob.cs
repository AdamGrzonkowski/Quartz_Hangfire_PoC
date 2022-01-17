namespace Quartz.API.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class SimpleJob : IJob
    {
        private readonly ILogger<SimpleJob> _logger;

        /// <summary>
        /// We need semaphore to control requests for manual execution of this job via <see cref="Controllers.JobsManagerController.ForceSimpleJobExecution"/>.
        /// Thanks to it, we can ensure that only one request for manual execution at a time is being processed (reduces risk of DoS on server).
        ///
        /// It should be replcaed with value stored in persitant storage of choice, in distributed environment.
        /// </summary>
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private static long _isRunning = 0;

        /// <summary>
        /// Cross-cluster flag returning info whether Job is currently running.
        /// </summary>
        /// <remarks>
        /// This should be changed to some database entry in cloud ecosystem, to have shared value across all VMs.
        /// </remarks>
        public static bool IsRunning
        {
            get
            {
                /* Interlocked.Read() is only available for int64,
                 * so we have to represent the bool as a long with 0's and 1's
                 */
                return Interlocked.Read(ref _isRunning) == 1;
            }
            set
            {
                Interlocked.Exchange(ref _isRunning, Convert.ToInt64(value));
            }
        }

        public SimpleJob(ILogger<SimpleJob> logger)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"{context.JobDetail.Key} with id {context.FireInstanceId} : started.");
            try
            {
                await _semaphore.WaitAsync();
                IsRunning = true;

                // START - put Job's body here
                await DoSomeBusinessLogicOperation(context.CancellationToken);

                _logger.LogInformation($"{context.JobDetail.Key} with id {context.FireInstanceId} : finished in {context.JobRunTime} [hh:mm:ss.us]");

                await Task.CompletedTask;

                // END
            }
            finally
            {
                _semaphore.Release();
                IsRunning = false;
            }
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