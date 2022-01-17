using Microsoft.AspNetCore.Mvc;
using Quartz.API.BackgroundJobs;

namespace Quartz.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JobsManagerController : ControllerBase
    {
        private readonly ILogger<JobsManagerController> _logger;
        private readonly ISchedulerFactory _backgroundJobsSchedulerFactory;

        public JobsManagerController(
            ILogger<JobsManagerController> logger,
            ISchedulerFactory backgroundJobsSchedulerFactory)
        {
            _logger = logger;
            _backgroundJobsSchedulerFactory = backgroundJobsSchedulerFactory;
        }

        [HttpPost("simpleJob/execute")]
        public async Task<IActionResult> ForceSimpleJobExecution()
        {
            var scheduler = await _backgroundJobsSchedulerFactory.GetScheduler();

            if (SimpleJob.IsRunning) // this property SHOULD BE cluster aware - should work in cloud ecosystem (check state of all VMs)
            {
                _logger.LogInformation($"Job {nameof(SimpleJob)} is currently running. Manual execution has been prevented.");
                return BadRequest("Job is already running");
            }

            await scheduler.TriggerJob(new JobKey(nameof(SimpleJob)));

            _logger.LogInformation($"Job {nameof(SimpleJob)} has been manually triggered.");
            return Ok();
        }

        [HttpPost("simpleJob/cancel")]
        public async Task<IActionResult> CancelSimpleJobExecution()
        {
            var scheduler = await _backgroundJobsSchedulerFactory.GetScheduler();
            var currentlyExecutingJobs = await scheduler.GetCurrentlyExecutingJobs(); // this method is NOT cluster aware - won't work in cloud ecosystem (won't check state of all VMs, only the single one which processes this request)

            var simpleJob = currentlyExecutingJobs.FirstOrDefault(x => x.JobDetail.Key.Name.Equals(nameof(SimpleJob)));
            if (simpleJob != null)
            {
                _logger.LogInformation($"Job {simpleJob.JobDetail.Key} is being cancelled...");

                await scheduler.Interrupt(simpleJob.JobDetail.Key);

                return Ok();
            }

            return NotFound();
        }
    }
}