# Quartz_Hangfire_PoC
This repo shows how one can use [Quartz.NET](https://www.quartz-scheduler.net/) or [Hangfire](https://www.hangfire.io/) as job scheduling mechanisms for recurring jobs, while ensuring that only one instance of that job can run at a specific time. Also supports cancellation of a job.

## Prerequisites
You need to have on your machine installed:
1. [.NET 6 SDK.](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
2. [Docker](https://www.docker.com/)

## Getting started
### Hangfire.API
All required dependencies are specified in 'docker-compose' file. Simply run 

    docker-compose up

command from the path where 'docker-compose' file resides and Docker will download & run all needed dependencies for you. 

### Quartz.API
Run the project, no additional steps are needed.

## Features and key points
### Recurring jobs
Recurring jobs registration for both projects are defined in Program.cs.
* Hangfire uses [RecurringJob.AddOrUpdate](https://docs.hangfire.io/en/latest/background-methods/performing-recurrent-tasks.html) method
* Quartz.NET uses [IServiceCollectionQuartzConfigurator.AddJob](https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/jobs-and-triggers.html) method

Schedules according to which jobs should re-execute are defined in appsettings.json as CRON schedules (with suffix 'CronSchedule').

### Manual triggering of a job
* Hangfire's job can be triggered manually via its UI (Hangfire Dashboard)
* Quartz.NET job can be triggered manually by calling dedicated HTTP POST 'simpleJob/execute' endpoint.

### Cancelling ongoing job
* Hangfire's job can be cancelled manually via its UI (Hangfire Dashboard), by deleting job (deleting also calls cancellation logic)
* Quartz.NET job can be cancelled manually by calling dedicated HTTP POST 'simpleJob/cancel' endpoint.

### Job storage
* Hangfire uses [SQL Server](https://www.microsoft.com/pl-pl/sql-server/sql-server-2019) as a [job storage](https://docs.hangfire.io/en/latest/configuration/using-sql-server.html) in this example.
* Quartz.NET uses RAM memory as a [job storage](https://www.quartz-scheduler.net/documentation/quartz-2.x/tutorial/job-stores.html#ramjobstore) in this example.