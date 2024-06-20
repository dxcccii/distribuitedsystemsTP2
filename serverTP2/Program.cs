using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Servidor
{
    public static Dictionary<string, (string ServiceId, string Password)> serviceDict = new Dictionary<string, (string ServiceId, string Password)>();
    public static Dictionary<string, List<string>> taskDict = new Dictionary<string, List<string>>();
    private static Mutex mutex = new Mutex();

    private static IConnection rabbitConnection;
    private static IModel rabbitChannel;
    private static string rpcQueueName = "rpc_queue";

    static void Main(string[] args)
    {
        InitRabbitMQ();
        PrintWorkingDirectory();
        LoadServiceAllocationsFromCSV();
        LoadDataFromCSVForAllServices();

        var consumer = new EventingBasicConsumer(rabbitChannel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var props = ea.BasicProperties;
            var replyProps = rabbitChannel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            string response = null;
            try
            {
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received message: {message}");
                response = HandleRequest(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling request: {ex.Message}");
                response = "500 INTERNAL SERVER ERROR";
            }
            finally
            {
                var responseBytes = Encoding.UTF8.GetBytes(response);
                rabbitChannel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
                rabbitChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
        };

        rabbitChannel.BasicConsume(queue: rpcQueueName, autoAck: false, consumer: consumer);
        Console.WriteLine("RPC Server is running. Waiting for requests...");
        Console.ReadLine();
    }

    private static void InitRabbitMQ()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        rabbitConnection = factory.CreateConnection();
        rabbitChannel = rabbitConnection.CreateModel();
        rabbitChannel.QueueDeclare(queue: rpcQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        rabbitChannel.ExchangeDeclare(exchange: "service_notifications", type: ExchangeType.Topic); // Ensure exchange declaration
        Console.WriteLine("RabbitMQ Initialized.");
    }

    private static string HandleRequest(string message)
    {
        string response = null;
        if (message.StartsWith("CONNECT"))
        {
            response = "100 OK";
        }
        else if (message.StartsWith("CLIENT_ID:"))
        {
            string clientId = message.Substring("CLIENT_ID:".Length).Trim();
            response = $"ID_CONFIRMED:{clientId}";
        }
        else if (message.StartsWith("PASSWORD:"))
        {
            string[] parts = message.Substring("PASSWORD:".Length).Trim().Split(',');
            string clientId = parts[0].Trim();
            string password = parts[1].Trim();

            if (serviceDict.ContainsKey(clientId) && serviceDict[clientId].Password == password)
            {
                response = "PASSWORD_CONFIRMED";
            }
            else
            {
                response = "403 FORBIDDEN";
            }
        }
        else if (message.StartsWith("ADMIN_SERVICE_ID:"))
        {
            string serviceId = message.Substring("ADMIN_SERVICE_ID:".Length).Trim();

            if (!serviceId.StartsWith("Servico_"))
            {
                response = "500 BAD REQUEST";
            }
            else
            {
                string serviceFilePath = Path.Combine(serviceId + ".csv");
                response = File.Exists(serviceFilePath) ? "SERVICE_CONFIRMED" : "SERVICE_NOT_FOUND";
            }
        }
        else
        {
            string[] parts = message.Split('|');
            string command = parts[0];
            string clientId = parts[1];
            string data = parts.Length > 2 ? parts[2] : null;

            switch (command)
            {
                case "ADD_TASK":
                    response = AddTask(clientId, data);
                    break;
                case "CONSULT_TASKS":
                    response = ConsultTasks(clientId);
                    break;
                case "CHANGE_TASK_STATUS":
                    var statusParts = data.Split(',');
                    response = ChangeTaskStatus(clientId, statusParts[0], statusParts[1], statusParts[2]);
                    break;
                case "REQUEST_TASK":
                    response = AllocateTask(clientId);
                    break;
                case "TASK_COMPLETED":
                    response = MarkTaskAsCompleted(clientId, data);
                    break;
                case "SUBSCRIBE":
                    SubscribeToService(clientId, data);
                    response = "SUBSCRIBED";
                    break;
                case "UNSUBSCRIBE":
                    UnsubscribeFromService(clientId, data);
                    response = "UNSUBSCRIBED";
                    break;
                default:
                    response = "500 BAD REQUEST";
                    break;
            }
        }
        return response;
    }

    private static void LoadServiceAllocationsFromCSV()
    {
        string csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "service_allocations.csv");

        try
        {
            var lines = File.ReadAllLines(csvFilePath);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');

                if (parts.Length >= 3)
                {
                    string clientId = parts[0].Trim();
                    string serviceId = parts[1].Trim();
                    string password = parts[2].Trim();

                    serviceDict[clientId] = (serviceId, password);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading data from CSV file {csvFilePath}: {ex.Message}");
        }
    }

    private static string AddTask(string serviceId, string taskDescription)
    {
        string serviceFilePath = serviceId + ".csv";
        try
        {
            string newTask = $"{taskDescription},nao alocada,";
            File.AppendAllLines(serviceFilePath, new string[] { newTask });
            PublishNotification($"\nTASK_ADDED:{serviceId}: {taskDescription}", isAdminChange: true);
            return "201 CREATED";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding task: {ex.Message}");
            return "500 INTERNAL SERVER ERROR";
        }
    }

    private static string ConsultTasks(string serviceFilePath)
    {
        serviceFilePath = serviceFilePath + ".csv";
        try
        {
            string[] tasks = File.ReadAllLines(serviceFilePath);
            StringBuilder response = new StringBuilder();
            foreach (string task in tasks)
            {
                response.AppendLine(task);
            }
            response.AppendLine("END");
            return response.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error consulting tasks: {ex.Message}");
            return "500 Internal Server Error";
        }
    }

    private static string ChangeTaskStatus(string serviceId, string taskDescription, string newStatus, string additionalField)
    {
        string serviceFilePath = serviceId + ".csv";
        try
        {
            string[] lines = File.ReadAllLines(serviceFilePath);
            bool taskFound = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] parts = line.Split(',');

                if (parts.Length >= 3 && parts[1].Trim() == taskDescription)
                {
                    if (!IsValidStatus(newStatus))
                    {
                        return "500 BAD REQUEST - Invalid newStatus";
                    }

                    if (newStatus.ToLower() == "nao alocada")
                    {
                        additionalField = "";
                    }
                    else if (!string.IsNullOrEmpty(additionalField) && !additionalField.StartsWith("Cl_"))
                    {
                        return "500 BAD REQUEST - Additional field must start with 'Cl_'";
                    }

                    parts[2] = newStatus;
                    if (parts.Length == 3)
                    {
                        line = string.Join(",", parts[0], parts[1], parts[2], additionalField);
                    }
                    else
                    {
                        parts[3] = additionalField;
                        line = string.Join(",", parts);
                    }

                    lines[i] = line;
                    taskFound = true;
                    break;
                }
            }

            if (taskFound)
            {
                File.WriteAllLines(serviceFilePath, lines);
                string notificationMessage = $"\nTASK_STATUS_CHANGED:{serviceId}:{taskDescription}:{newStatus}";
                PublishNotification(notificationMessage, isAdminChange: true);
                Console.WriteLine($"Published notification: {notificationMessage}");
                return "200 OK";
            }
            else
            {
                return "404 NOT FOUND - Task not found";
            }
        }
        catch (IOException)
        {
            return "500 INTERNAL SERVER ERROR - IOException";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error changing task status: {ex.Message}");
            return "500 INTERNAL SERVER ERROR";
        }
    }

    private static bool IsValidStatus(string status)
    {
        string[] validStatuses = { "Nao alocada", "Concluido", "Em curso" };
        return validStatuses.Contains(status);
    }

    private static void PrintWorkingDirectory()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        Console.WriteLine("Current working directory: " + currentDirectory);
    }

    private static void LoadDataFromCSVForAllServices()
    {
        string servicesFilePath = Directory.GetCurrentDirectory();
        try
        {
            foreach (var serviceFile in Directory.GetFiles(servicesFilePath, "*.csv"))
            {
                var serviceLines = File.ReadAllLines(serviceFile);
                string serviceId = Path.GetFileNameWithoutExtension(serviceFile);

                if (!taskDict.ContainsKey(serviceId))
                {
                    taskDict[serviceId] = new List<string>();
                }

                for (int i = 1; i < serviceLines.Length; i++)
                {
                    var line = serviceLines[i];
                    var parts = line.Split(',');

                    if (parts.Length == 3)
                    {
                        line = $"{parts[0].Trim()},{parts[1].Trim()},{parts[2].Trim()},";
                    }
                    else if (parts.Length < 4)
                    {
                        line = $"{line.Trim()},";
                    }

                    taskDict[serviceId].Add(line.Trim());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading data from CSV file {servicesFilePath}: {ex.Message}");
        }
    }

    public static string AllocateTask(string clientId)
    {
        mutex.WaitOne();
        try
        {
            if (!serviceDict.ContainsKey(clientId))
            {
                return "ERROR: Service not found for client";
            }

            string serviceId = serviceDict[clientId].ServiceId;

            if (taskDict.ContainsKey(serviceId))
            {
                var unallocatedTask = taskDict[serviceId].FirstOrDefault(task => task.Split(',')[2].Trim().ToLower() == "nao alocada");

                if (unallocatedTask != null)
                {
                    var taskParts = unallocatedTask.Split(',');

                    if (taskParts.Length != 4)
                    {
                        return "500 INTERNAL SERVER ERROR";
                    }

                    taskParts[2] = "Em curso";
                    taskParts[3] = clientId;

                    string updatedTask = string.Join(",", taskParts);
                    int taskIndex = taskDict[serviceId].IndexOf(unallocatedTask);
                    taskDict[serviceId][taskIndex] = updatedTask;

                    string serviceFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{serviceId}.csv");
                    File.WriteAllLines(serviceFilePath, taskDict[serviceId]);

                    string message = $"TASK_ALLOCATED:{taskParts[1]}";

                    return message;
                }
                else
                {
                    return "NO_TASK_AVAILABLE";
                }
            }
            else
            {
                return "NO_TASK_AVAILABLE";
            }
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    public static string MarkTaskAsCompleted(string clientId, string taskDescription)
    {
        mutex.WaitOne();
        try
        {
            if (!serviceDict.ContainsKey(clientId))
            {
                return "ERROR: Service not found for client";
            }

            string serviceId = serviceDict[clientId].ServiceId;

            if (taskDict.ContainsKey(serviceId))
            {
                var taskIndex = taskDict[serviceId].FindIndex(task => task.Split(',')[1].Trim() == taskDescription);

                if (taskIndex != -1)
                {
                    var taskParts = taskDict[serviceId][taskIndex].Split(',');

                    if (taskParts.Length != 4)
                    {
                        return $"ERROR: Incorrect task format for task: {taskDict[serviceId][taskIndex]}";
                    }

                    taskParts[2] = "Concluido";
                    taskParts[3] = clientId;

                    string updatedTask = string.Join(",", taskParts);
                    taskDict[serviceId][taskIndex] = updatedTask;

                    string serviceFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{serviceId}.csv");
                    File.WriteAllLines(serviceFilePath, taskDict[serviceId]);

                    string notificationMessage = $"TASK_COMPLETED:{clientId}: {taskDescription}";
                    PublishNotification(notificationMessage, isAdminChange: false);
                    return $"TASK_MARKED_AS_COMPLETED:{taskDescription}";
                }
                else
                {
                    return $"ERROR_TASK_NOT_FOUND:{taskDescription}";
                }
            }
            else
            {
                return $"ERROR_SERVICE_NOT_FOUND:{serviceId}";
            }
        }
        catch (Exception)
        {
            return "500 INTERNAL SERVER ERROR";
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    private static void PublishNotification(string message, bool isAdminChange)
    {
        var body = Encoding.UTF8.GetBytes($"{message}");
        string exchange = "service_notifications";
        string routingKey = $"NOTIFICATION.{message.Split(':')[1]}"; // Use serviceId as part of the routing key
        rabbitChannel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: null, body: body);
        Console.WriteLine($"Sent notification: {message} with routing key {routingKey}");
    }

    private static void PublishUnsubscribeNotification(string clientId, string serviceId)
    {
        var body = Encoding.UTF8.GetBytes($"\nUNSUBSCRIBE:{clientId}:{serviceId}");
        string exchange = "service_notifications";
        string routingKey = $"NOTIFICATION.{serviceId}";
        rabbitChannel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: null, body: body);
        Console.WriteLine($"Sent unsubscription notification for client {clientId} from service {serviceId}");
    }


    public static Dictionary<string, List<string>> subscriptions = new Dictionary<string, List<string>>();

    private static void SubscribeToService(string clientId, string serviceId)
    {
        if (!subscriptions.ContainsKey(serviceId))
        {
            subscriptions[serviceId] = new List<string>();
        }
        if (!subscriptions[serviceId].Contains(clientId))
        {
            subscriptions[serviceId].Add(clientId);
        }
        Console.WriteLine($"Client {clientId} subscribed to {serviceId}");
    }

    private static void UnsubscribeFromService(string clientId, string serviceId)
    {
        if (subscriptions.ContainsKey(serviceId))
        {
            subscriptions[serviceId].Remove(clientId);
        }
        Console.WriteLine($"Client {clientId} unsubscribed from {serviceId}");
    }

}
