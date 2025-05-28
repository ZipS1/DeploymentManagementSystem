using DeploymentManagementSystem.Data;
using DeploymentManagementSystem.Data.DomainStringConstants;
using DeploymentManagementSystem.Services.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DeploymentManagementSystem.Services
{
    public class DeploymentService : BackgroundService
    {
        private readonly IDeploymentQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DeploymentService> _logger;

        public DeploymentService(
            IDeploymentQueue queue,
            IServiceProvider serviceProvider,
            ILogger<DeploymentService> logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Deployment service started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var request = await _queue.DequeueAsync(stoppingToken);
                    _logger.LogInformation($"Started to deploy task.Id={request.TaskId}");
                    await ProcessDeployment(request.TaskId, request.UserPAT);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing deployment queue");
                }
            }
        }

        private async Task ProcessDeployment(int taskId, string userToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            var gitlabService = scope.ServiceProvider.GetRequiredService<GitlabService>();

            using var context = await dbFactory.CreateDbContextAsync();
            var task = await context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found", taskId);
                return;
            }

            task.TaskStatusId = (await context.TaskStatuses
                .SingleOrDefaultAsync(t => t.Name == TaskStatusConstants.Deploying))!.Id;
            await context.SaveChangesAsync();
            _logger.LogInformation($"Update status of task.ID = {task.Id} to Deploying... ");

            try
            {
                var projectDTO = new GitlabProjectDTO
                {
                    Url = task.Project!.GitlabUrl,
                    ProjectId = task.Project!.GitlabProjectId!.Value,
                    DefaultBranch = task.Project!.GitlabDefaultBranch,
                };

                var mergeSucceed = await gitlabService.MergeMRForTask(
                    new GitlabTaskDTO
                    {
                        MergeRequestID = task.MergeRequestId,
                        BranchNameEncoded = task.RefName,
                    },
                    projectDTO,
                    userToken
                );

                if (mergeSucceed)
                {
                    var isPipelineSucceed = await gitlabService.QueryAndHandleDeploymentPipeline(
                        projectDTO, task.Id, userToken);
                    if (isPipelineSucceed)
                    {
                        task.TaskStatusId = (await context.TaskStatuses
                            .SingleOrDefaultAsync(t => t.Name == TaskStatusConstants.SuccessfullyDeployed))!.Id;
                    } else
                    {
                        task.TaskStatusId = (await context.TaskStatuses
                            .SingleOrDefaultAsync(t => t.Name == TaskStatusConstants.DeploymentError))!.Id;
                    }
                }
                else
                {
                    task.TaskStatusId = (await context.TaskStatuses
                        .SingleOrDefaultAsync(t => t.Name == TaskStatusConstants.DeploymentError))!.Id;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deploy task {TaskId}", taskId);
                task.TaskStatusId = (await context.TaskStatuses
                    .SingleOrDefaultAsync(t => t.Name == TaskStatusConstants.DeploymentError))!.Id;
            }
            finally
            {
                _logger.LogInformation($"Saving deployment status in database for task.Id = {task.Id}");
                await context.SaveChangesAsync();
            }
        }
    }
}
