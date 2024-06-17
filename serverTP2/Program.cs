using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskManager;

class Servidor
{
    public static Dictionary<string, string> serviceDict = new Dictionary<string, string>();
    public static Dictionary<string, List<string>> taskDict = new Dictionary<string, List<string>>();
    private static Mutex mutex = new Mutex();
    private static IConnection rabbitConnection;
    private static IModel rabbitChannel;
    private static string rabbitQueueName = "task_notifications";

    static void Main(string[] args)
    {
        InitRabbitMQ();
        PrintWorkingDirectory();
        LoadServiceAllocationsFromCSV();
        LoadDataFromCSVForAllServices();

        TcpListener servidor = null;
        try
        {
            servidor = new TcpListener(IPAddress.Any, 1234);
            servidor.Start();
            Console.WriteLine("Servidor iniciado. Aguardando conexÃµes...");

            while (true)
            {
                TcpClient cliente = servidor.AcceptTcpClient();
                Console.WriteLine("Cliente conectado!");
                ThreadPool.QueueUserWorkItem(HandleClient, cliente);
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Erro de Socket: " + ex.ToString());
        }
        finally
        {
            servidor?.Stop();
        }
    }

    private static void InitRabbitMQ()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        rabbitConnection = factory.CreateConnection();
        rabbitChannel = rabbitConnection.CreateModel();

        rabbitChannel.QueueDeclare(queue: rabbitQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(rabbitChannel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received notification: {message}");
        };
        rabbitChannel.BasicConsume(queue: rabbitQueueName, autoAck: true, consumer: consumer);
    }

    public static void PublishNotification(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        rabbitChannel.BasicPublish(exchange: "", routingKey: rabbitQueueName, basicProperties: null, body: body);
        Console.WriteLine($"Sent notification: {message}");
    }

    private static void LoadServiceAllocationsFromCSV()
    {
        string csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "service_allocations.csv");
        try
        {
            var lines = File.ReadAllLines(csvFilePath);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    string clientId = parts[0].Trim();
                    string serviceId = parts[1].Trim();
                    serviceDict[clientId] = serviceId;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading data from CSV file {csvFilePath}: {ex.Message}");
        }
    }


    private static void HandleClient(object obj)
    {
        TcpClient cliente = (TcpClient)obj;
        NetworkStream stream = cliente.GetStream();
        StreamReader leitor = new StreamReader(stream);
        StreamWriter escritor = new StreamWriter(stream) { AutoFlush = true };

        try
        {
            string clientId = null;
            string serviceId = null;

            while (true)
            {
                string message = leitor.ReadLine();
                if (message == null)
                    break;

                if (message.StartsWith("CONNECT"))
                {
                    escritor.WriteLine("100 OK");
                }
                else if (message.StartsWith("CLIENT_ID:"))
                {
                    clientId = message.Substring("CLIENT_ID:".Length).Trim();
                    escritor.WriteLine($"ID_CONFIRMED:{clientId}");
                }
                else if (message.StartsWith("ADMIN_SERVICE_ID:"))
                {
                    serviceId = message.Substring("ADMIN_SERVICE_ID:".Length).Trim();
                    if (!serviceId.StartsWith("Servico_"))
                    {
                        escritor.WriteLine("400 BAD REQUEST");
                        Console.WriteLine($"Invalid service ID format: {serviceId}");
                        continue;
                    }

                    string serviceFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{serviceId}.csv");
                    Console.WriteLine($"Attempting to load service file: {serviceFilePath}");

                    if (File.Exists(serviceFilePath))
                    {
                        escritor.WriteLine("SERVICE_CONFIRMED");
                    }
                    else
                    {
                        escritor.WriteLine("SERVICE_NOT_FOUND");
                        Console.WriteLine($"Service not found: {serviceId}");
                    }
                }
                else if (clientId != null && clientId.StartsWith("Adm"))
                {
                    if (serviceId != null)
                    {
                        string serviceFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{serviceId}.csv");
                        string response = ProcessAdminCommand(message, clientId, serviceFilePath);
                        escritor.WriteLine(response);
                    }
                    else
                    {
                        escritor.WriteLine("403 SERVICE_ID_NOT_SPECIFIED");
                    }
                }
                else if (clientId != null)
                {
                    string response = ProcessClientCommand(message, clientId);
                    escritor.WriteLine(response);
                }
                else
                {
                    escritor.WriteLine("403 FORBIDDEN");
                }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine("Erro de E/S: " + ex.ToString());
        }
        finally
        {
            cliente.Close();
        }
    }

    private static string ProcessAdminCommand(string message, string clientId, string serviceFilePath)
    {
        if (message.StartsWith("ADD_TASK:"))
        {
            string taskDescription = message.Substring("ADD_TASK:".Length).Trim();

            return AddTask(serviceFilePath, taskDescription);
        }
        else if (message.StartsWith("CONSULT_TASKS"))
        {
            return ConsultTasks(serviceFilePath);
        }
        else if (message.StartsWith("CHANGE_TASK_STATUS:"))
        {
            string taskStatusInfo = message.Substring("CHANGE_TASK_STATUS:".Length).Trim();
            string[] statusParts = taskStatusInfo.Split(',');
            if (statusParts.Length < 3)
            {
                return "400 BAD REQUEST";
            }

            string taskDescription = statusParts[0].Trim();
            string newStatus = statusParts[1].Trim();
            string additionalField = statusParts[2].Trim();
            return ChangeTaskStatus(serviceFilePath, taskDescription, newStatus, additionalField);
        }
        else
        {
            return "400 BAD REQUEST";
        }
    }

    private static string ProcessClientCommand(string message, string clientId)
    {
        if (message.StartsWith("REQUEST_TASK"))
        {
            return AllocateTask(clientId);
        }
        else if (message.StartsWith("TASK_COMPLETED:"))
        {
            string taskDescription = message.Substring("TASK_COMPLETED:".Length).Trim();
            return MarkTaskAsCompleted(clientId, taskDescription);
        }
        else if (message.StartsWith("SUBSCRIBE:"))
        {
            string serviceId = message.Substring("SUBSCRIBE:".Length).Trim();
            SubscribeToService(clientId, serviceId);
            return "SUBSCRIBED";
        }
        else if (message.StartsWith("UNSUBSCRIBE:"))
        {
            string serviceId = message.Substring("UNSUBSCRIBE:".Length).Trim();
            UnsubscribeFromService(clientId, serviceId);
            return "UNSUBSCRIBED";
        }
        else
        {
            return "400 BAD REQUEST";
        }
    }

    private static string AddTask(string serviceFilePath, string taskDescription)
    {
        try
        {
            string newTask = $"{taskDescription},nao alocada,";
            File.AppendAllLines(serviceFilePath, new string[] { newTask });
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
        try
        {
            string[] tasks = File.ReadAllLines(serviceFilePath);
            StringBuilder response = new StringBuilder();
            foreach (string task in tasks)
            {
                response.AppendLine(task);
            }
            response.AppendLine("<END_OF_RESPONSE>");
            Console.WriteLine("ConsultTasks response: " + response.ToString());
            return response.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error consulting tasks: {ex.Message}");
            return "500 Internal Server Error";
        }
    }
    private static string ChangeTaskStatus(string serviceFilePath, string taskDescription, string newStatus, string additionalField)
    {
        try
        {
            // Validate taskDescription
            if (string.IsNullOrEmpty(taskDescription))
            {
                return "400 BAD REQUEST - Task description cannot be empty";
            }

            string[] lines = File.ReadAllLines(serviceFilePath);
            bool taskFound = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] parts = line.Split(',');

                if (parts.Length >= 4 && parts[1].Trim() == taskDescription)
                {
                    // Validate newStatus
                    if (!IsValidStatus(newStatus))
                    {
                        return "400 BAD REQUEST - Invalid newStatus";
                    }

                    // Validate additionalField if needed
                    if (newStatus.ToLower() == "nao alocada")
                    {
                        additionalField = ""; // Clear additionalField if status is "nao alocada"
                    }
                    else if (!string.IsNullOrEmpty(additionalField))
                    {
                        // Validate additionalField starts with "Cl"
                        if (!additionalField.StartsWith("Cl"))
                        {
                            return "400 BAD REQUEST - Additional field must start with 'Cl'";
                        }
                    }

                    // Update the line
                    parts[2] = newStatus;
                    parts[3] = additionalField;

                    string updatedLine = string.Join(",", parts);
                    lines[i] = updatedLine;
                    taskFound = true;
                    break;
                }
            }

            if (taskFound)
            {
                File.WriteAllLines(serviceFilePath, lines);
                return "200 OK";
            }
            else
            {
                return "404 NOT FOUND - Task not found";
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error accessing file {serviceFilePath}: {ex.Message}");
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
        // Define your validation rules here
        // For example, if status can only be one of a predefined set:
        string[] validStatuses = { "nao alocada", "concluido", "em progresso" };
        return validStatuses.Contains(status.ToLower());
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

                for (int i = 1; i < serviceLines.Length; i++) // Skip the header line
                {
                    var line = serviceLines[i];
                    var parts = line.Split(',');
                    // Ensure there are exactly 4 parts
                    if (parts.Length == 3)
                    {
                        line = $"{parts[0].Trim()},{parts[1].Trim()},{parts[2].Trim()},";
                    }
                    else if (parts.Length < 4)
                    {
                        // Add missing fields if necessary
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

            string serviceId = serviceDict[clientId];
            if (taskDict.ContainsKey(serviceId))
            {
                var unallocatedTask = taskDict[serviceId].FirstOrDefault(task => task.Split(',')[2].Trim().ToLower() == "nao alocada");

                if (unallocatedTask != null)
                {
                    var taskParts = unallocatedTask.Split(',');

                    // Log task parts for debugging
                    Console.WriteLine($"Task parts: {string.Join(" | ", taskParts)}");

                    // Ensure there are exactly 4 parts in taskParts
                    if (taskParts.Length != 4)
                    {
                        Console.WriteLine($"Error: Task format is incorrect for task: {unallocatedTask}");
                        return "500 INTERNAL SERVER ERROR";
                    }

                    // Mark the task as allocated to the client
                    taskParts[2] = "Em curso"; // Change status to "Em curso"
                    taskParts[3] = clientId; // Assign client ID

                    string updatedTask = string.Join(",", taskParts);
                    int taskIndex = taskDict[serviceId].IndexOf(unallocatedTask);
                    taskDict[serviceId][taskIndex] = updatedTask;

                    // Update the CSV file
                    string serviceFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{serviceId}.csv");
                    File.WriteAllLines(serviceFilePath, taskDict[serviceId]);

                    string message = $"TASK_ALLOCATED:{taskParts[1]}";
                    PublishNotification(message);
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

            string serviceId = serviceDict[clientId];
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

                    taskParts[2] = "Concluido"; // Change status to "Concluido"
                    taskParts[3] = clientId; // Update client ID

                    string updatedTask = string.Join(",", taskParts);
                    taskDict[serviceId][taskIndex] = updatedTask;

                    // Update the CSV file
                    string serviceFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{serviceId}.csv");
                    File.WriteAllLines(serviceFilePath, taskDict[serviceId]);

                    string notificationMessage = $"TASK_COMPLETED:{clientId}:{taskDescription}";
                    PublishNotification(notificationMessage);
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking task as completed: {ex.Message}");
            return "500 INTERNAL SERVER ERROR";
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    private static void SubscribeToService(string clientId, string serviceId)
    {
        PublishNotification($"SUBSCRIBED:{clientId}:{serviceId}");
    }

    private static void UnsubscribeFromService(string clientId, string serviceId)
    {
        PublishNotification($"UNSUBSCRIBED:{clientId}:{serviceId}");
    }
}