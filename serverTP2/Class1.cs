using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using TaskManager;

namespace TaskManager
{
    public class TaskServiceImpl : TaskService.TaskServiceBase
    {
        public override Task<TaskResponse> CreateTask(TaskRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Received task creation request from admin {request.AdminId}");
            Servidor.PublishNotification($"New task created: {request.TaskDescription} for service {request.ServiceId}");
            return Task.FromResult(new TaskResponse { Status = "TASK_CREATED" });
        }

        public override Task<TaskResponse> UpdateTask(TaskRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Received task update request from admin {request.AdminId}");
            Servidor.PublishNotification($"Task updated: {request.TaskDescription} for service {request.ServiceId}");
            return Task.FromResult(new TaskResponse { Status = "TASK_UPDATED" });
        }

        public override Task<ServiceList> ListServices(Empty request, ServerCallContext context)
        {
            var services = Servidor.serviceDict.Values.Distinct().ToList();
            return Task.FromResult(new ServiceList { Services = { services } });
        }

        public override Task<TaskResponse> AllocateTask(ClientRequest request, ServerCallContext context)
        {
            var response = Servidor.AllocateTask(request.ClientId);
            return Task.FromResult(new TaskResponse { Status = response });
        }

        public override Task<TaskResponse> CompleteTask(ClientRequest request, ServerCallContext context)
        {
            var response = Servidor.MarkTaskAsCompleted(request.ClientId, request.TaskDescription);
            return Task.FromResult(new TaskResponse { Status = response });
        }

        public override Task<ServiceResponse> SubscribeService(ClientRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Client {request.ClientId} subscribed to service {request.ServiceId}");
            return Task.FromResult(new ServiceResponse { Status = "SUBSCRIBED" });
        }

        public override Task<ServiceResponse> UnsubscribeService(ClientRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Client {request.ClientId} unsubscribed from service {request.ServiceId}");
            return Task.FromResult(new ServiceResponse { Status = "UNSUBSCRIBED" });
        }
    }
}