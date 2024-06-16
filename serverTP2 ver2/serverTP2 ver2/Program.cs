using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using taskmanager; // Ensure this namespace matches the generated gRPC code

class Servidor
{
    private static Dictionary<string, string> serviceDict = new Dictionary<string, string>();
    private static Dictionary<string, List<string>> taskDict = new Dictionary<string, List<string>>();
    private static Mutex mutex = new Mutex();
    private static IConnection rabbitConnection;
    private static IModel rabbitChannel;
    private static string rabbitQueueName = "task_notifications";

    static void Main(string[] args)
    {
        Task.Run(() => StartGrpcServer());
        InitRabbitMQ();
        PrintWorkingDirectory();
        LoadServiceAllocationsFromCSV();
        LoadDataFromCSVForAllServices();

        TcpListener servidor = null;
        try
        {
            servidor = new TcpListener(IPAddress.Any, 1234);
            servidor.Start();
            Console.WriteLine("Servidor iniciado. Aguardando conexões...");

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
            if (servidor != null)
            {
                servidor.Stop();
            }
        }
    }

    private static async Task StartGrpcServer()
    {
        const int Port = 50051;
        Server server = new Server
        {
            Services = { TaskManagerService.BindService(new TaskManagerServiceImpl()) }, // Replace with your generated service
            Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
        };

        server.Start();
        Console.WriteLine($"gRPC server started on port {Port}");

        await server.ShutdownAsync();
    }

    private static void InitRabbitMQ()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        rabbitConnection = factory.CreateConnection();
        rabbitChannel = rabbitConnection.CreateModel();

        rabbitChannel.QueueDeclare(queue: rabbitQueueName,
                                   durable: false,
                                   exclusive: false,
                                   autoDelete: false,
                                   arguments: null);

        var consumer = new EventingBasicConsumer(rabbitChannel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received notification: {message}");
        };
        rabbitChannel.BasicConsume(queue: rabbitQueueName,
                                   autoAck: true,
                                   consumer: consumer);
    }

    private static void HandleClient(object obj)
    {
        TcpClient cliente = (TcpClient)obj;
        try
        {
            using (NetworkStream stream = cliente.GetStream())
            using (StreamReader leitor = new StreamReader(stream))
            using (StreamWriter escritor = new StreamWriter(stream) { AutoFlush = true })
            {
                string mensagem;
                while ((mensagem = leitor.ReadLine()) != null)
                {
                    Console.WriteLine("Mensagem recebida: " + mensagem);
                    string resposta = ProcessMessage(mensagem);
                    escritor.WriteLine(resposta);
                }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine("Erro de E/S: " + ex.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro inesperado: " + ex.ToString());
        }
        finally
        {
            if (cliente != null)
            {
                cliente.Close();
            }
        }
    }

    private static string ProcessMessage(string message)
    {
        try
        {
            if (message.StartsWith("CONNECT", StringComparison.OrdinalIgnoreCase))
            {
                return "100 OK";
            }
            else if (message.StartsWith("CLIENT_ID:", StringComparison.OrdinalIgnoreCase))
            {
                string clientId = message.Substring("CLIENT_ID:".Length).Trim();
                Console.WriteLine($"Received CLIENT_ID: {clientId}");
                return $"ID_CONFIRMED:{clientId}";
            }
            else if (message.StartsWith("TASK_COMPLETED:", StringComparison.OrdinalIgnoreCase))
            {
                string taskDescription = message.Substring("TASK_COMPLETED:".Length).Trim();
                string clientId = GetClientIdFromMessage(message);
                return MarkTaskAsCompleted(clientId, taskDescription);
            }
            else if (message.StartsWith("REQUEST_SERVICE CLIENT_ID:", StringComparison.OrdinalIgnoreCase))
            {
                string clientId = message.Substring("REQUEST_SERVICE CLIENT_ID:".Length).Trim();
                return AllocateService(clientId);
            }
            else if (message.StartsWith("REQUEST_TASK CLIENT_ID:", StringComparison.OrdinalIgnoreCase))
            {
                string clientId = message.Substring("REQUEST_TASK CLIENT_ID:".Length).Trim();
                return AllocateTask(clientId);
            }
            else if (message.Equals("SAIR", StringComparison.OrdinalIgnoreCase))
            {
                return "400 BYE";
            }
            else
            {
                return "500 ERROR: Comando não reconhecido";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex}");
            return "500 ERROR: Internal server error";
        }
    }

    private static string GetClientIdFromMessage(string message)
    {
        string[] parts = message.Split(':');
        if (parts.Length >= 2)
        {
            return parts[1].Trim();
        }
        return string.Empty;
    }

    private static void PrintWorkingDirectory()
    {
        string workingDirectory = Environment.CurrentDirectory;
        Console.WriteLine("Current Working Directory: " + workingDirectory);
    }

    private static void LoadServiceAllocationsFromCSV()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string serviceAllocationFilePath = Path.Combine(baseDir, "Alocacao_Cliente_Servico.csv");

        try
        {
            if (File.Exists(serviceAllocationFilePath))
            {
                serviceDict.Clear();
                foreach (var line in File.ReadLines(serviceAllocationFilePath).Skip(1))
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 2)
                    {
                        var clientId = parts[0].Trim();
                        var serviceId = parts[1].Trim();
                        serviceDict[clientId] = serviceId;
                    }
                }
                Console.WriteLine("Serviços carregados com sucesso.");
            }
            else
            {
                Console.WriteLine($"Erro: Arquivo {serviceAllocationFilePath} não encontrado.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar dados dos arquivos CSV: {ex.Message}");
        }
    }

    private static void LoadDataFromCSVForAllServices()
    {
        foreach (var serviceId in serviceDict.Values)
        {
            string serviceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{serviceId}.csv");
            Console.WriteLine($"Loading tasks for service '{serviceId}' from {serviceFilePath}");
            LoadDataFromCSV(serviceFilePath);
        }
    }

    private static void LoadDataFromCSV(string serviceFilePath)
    {
        try
        {
            if (File.Exists(serviceFilePath))
            {
                taskDict.Clear();
                foreach (var line in File.ReadLines(serviceFilePath).Skip(1))
                {
                    Console.WriteLine($"Processing line: {line}");
                    var parts = line.Split(',');
                    if (parts.Length >= 3)
                    {
                        var taskId = parts[0].Trim();
                        var taskDescription = parts[1].Trim();
                        var taskStatus = parts[2].Trim();
                        var clientId = parts.Length > 3 ? parts[3].Trim() : null;

                        Console.WriteLine($"Task ID: {taskId}, Description: {taskDescription}, Status: {taskStatus}, Client ID: {clientId}");

                        if (taskStatus.Equals("Nao alocada", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!taskDict.ContainsKey(taskId))
                            {
                                taskDict[taskId] = new List<string>();
                                Console.WriteLine($"Created new task entry for ID: {taskId}");
                            }
                            taskDict[taskId].Add(taskDescription);
                            Console.WriteLine($"Added task '{taskDescription}' to taskDict under ID: {taskId}");
                        }
                        else
                        {
                            Console.WriteLine($"Task {taskId} is already allocated to client {clientId}. Skipping.");
                        }
                    }
                }
                Console.WriteLine($"Tarefas carregadas com sucesso de {serviceFilePath}.");
            }
            else
            {
                Console.WriteLine($"Erro: Arquivo {serviceFilePath} não encontrado.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar dados do arquivo CSV {serviceFilePath}: {ex.Message}");
        }
    }

    private static string AllocateTask(string clientId)
    {
        mutex.WaitOne();
        try
        {
            if (!serviceDict.ContainsKey(clientId))
            {
                return "NO_SERVICE_AVAILABLE";
            }

            string service = serviceDict[clientId];
            Console.WriteLine($"Client {clientId} belongs to service {service}");

            string serviceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{service}.csv");

            if (!File.Exists(serviceFilePath))
            {
                return "SERVICE_FILE_NOT_FOUND";
            }

            LoadDataFromCSV(serviceFilePath);
            string taskId = FindAvailableTask(service);
            if (taskId == null)
            {
                return "NO_TASK_AVAILABLE";
            }

            MarkTaskAsAllocated(service, taskId, clientId);
            NotifyTaskAllocation(taskId, clientId);

            return $"TASK_ALLOCATED:{taskId}";
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    private static string FindAvailableTask(string serviceId)
    {
        foreach (var task in taskDict)
        {
            if (task.Value.Count > 0)
            {
                return task.Key;
            }
        }
        return null;
    }

    private static void MarkTaskAsAllocated(string serviceId, string taskId, string clientId)
    {
        string serviceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{serviceId}.csv");

        try
        {
            List<string> lines = File.ReadAllLines(serviceFilePath).ToList();

            for (int i = 1; i < lines.Count; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length >= 4)
                {
                    var currentTaskId = parts[0].Trim();
                    var currentClientId = parts[3].Trim();

                    if (currentTaskId == taskId && currentClientId == "")
                    {
                        lines[i] = $"{taskId},{parts[1].Trim()},{parts[2].Trim()},{clientId}";
                        break;
                    }
                }
            }

            File.WriteAllLines(serviceFilePath, lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking task {taskId} as allocated: {ex.Message}");
        }
    }

    private static void NotifyTaskAllocation(string taskId, string clientId)
    {
        try
        {
            string notificationMessage = $"Task {taskId} allocated to client {clientId}";
            PublishNotification(notificationMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error notifying task allocation: {ex.Message}");
        }
    }

    private static void PublishNotification(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        rabbitChannel.BasicPublish(exchange: "",
                                   routingKey: rabbitQueueName,
                                   basicProperties: null,
                                   body: body);
        Console.WriteLine($"Published notification: {message}");
    }

    private static string MarkTaskAsCompleted(string clientId, string taskDescription)
    {
        try
        {
            string serviceId = serviceDict[clientId];
            string serviceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{serviceId}.csv");

            List<string> lines = File.ReadAllLines(serviceFilePath).ToList();

            for (int i = 1; i < lines.Count; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length >= 4)
                {
                    var currentTaskDescription = parts[1].Trim();
                    var currentClientId = parts[3].Trim();

                    if (currentTaskDescription == taskDescription && currentClientId == clientId)
                    {
                        lines[i] = $"{parts[0].Trim()},{taskDescription},Concluida,{clientId}";
                        break;
                    }
                }
            }

            File.WriteAllLines(serviceFilePath, lines);

            string notificationMessage = $"Task completed for client {clientId}: {taskDescription}";
            PublishNotification(notificationMessage);

            return "TASK_MARKED_COMPLETED";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking task completed: {ex.Message}");
            return "500 ERROR";
        }
    }

    private static string AllocateService(string clientId)
    {
        // Stub for AllocateService implementation
        return "SERVICE_ALLOCATED";
    }
}
