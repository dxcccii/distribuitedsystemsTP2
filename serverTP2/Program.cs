using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class Servidor
{
    private static Dictionary<string, string> serviceDict = new Dictionary<string, string>();
    private static Dictionary<string, List<string>> taskDict = new Dictionary<string, List<string>>();
    private static Dictionary<string, TcpClient> clientConnections = new Dictionary<string, TcpClient>();
    private static Dictionary<string, List<string>> clientSubscriptions = new Dictionary<string, List<string>>();
    private static Mutex mutex = new Mutex();

    static void Main(string[] args)
    {
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

    private static void HandleClient(object obj)
    {
        TcpClient cliente = (TcpClient)obj;
        try
        {
            using (NetworkStream stream = cliente.GetStream())
            using (StreamReader leitor = new StreamReader(stream))
            using (StreamWriter escritor = new StreamWriter(stream) { AutoFlush = true })
            {
                string clientId = null;
                string mensagem;
                while ((mensagem = leitor.ReadLine()) != null)
                {
                    Console.WriteLine("Mensagem recebida: " + mensagem);
                    string resposta = ProcessMessage(mensagem, cliente, out clientId);
                    escritor.WriteLine(resposta);
                }
                if (clientId != null)
                {
                    clientConnections.Remove(clientId);
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

    private static string ProcessMessage(string message, TcpClient client, out string clientId)
    {
        clientId = null;
        try
        {
            if (message.StartsWith("CONNECT", StringComparison.OrdinalIgnoreCase))
            {
                return "100 OK";
            }
            else if (message.StartsWith("CLIENT_ID:", StringComparison.OrdinalIgnoreCase))
            {
                clientId = message.Substring("CLIENT_ID:".Length).Trim();
                Console.WriteLine($"Received CLIENT_ID: {clientId}");
                clientConnections[clientId] = client;
                return $"ID_CONFIRMED:{clientId}";
            }
            else if (message.StartsWith("TASK_COMPLETED:", StringComparison.OrdinalIgnoreCase))
            {
                string taskDescription = message.Substring("TASK_COMPLETED:".Length).Trim();
                clientId = GetClientIdFromMessage(message);
                return MarkTaskAsCompleted(clientId, taskDescription);
            }
            else if (message.StartsWith("REQUEST_SERVICE CLIENT_ID:", StringComparison.OrdinalIgnoreCase))
            {
                clientId = message.Substring("REQUEST_SERVICE CLIENT_ID:".Length).Trim();
                return AllocateService(clientId);
            }
            else if (message.StartsWith("REQUEST_TASK CLIENT_ID:", StringComparison.OrdinalIgnoreCase))
            {
                clientId = message.Substring("REQUEST_TASK CLIENT_ID:".Length).Trim();
                return AllocateTask(clientId);
            }
            else if (message.StartsWith("SUBSCRIBE:", StringComparison.OrdinalIgnoreCase))
            {
                clientId = GetClientIdFromMessage(message);
                string serviceId = message.Substring("SUBSCRIBE:".Length).Trim();
                return SubscribeClientToService(clientId, serviceId);
            }
            else if (message.StartsWith("DISASSOCIATE:", StringComparison.OrdinalIgnoreCase))
            {
                clientId = GetClientIdFromMessage(message);
                return DisassociateService(clientId);
            }
            else if (message.StartsWith("ASSOCIATE:", StringComparison.OrdinalIgnoreCase))
            {
                clientId = GetClientIdFromMessage(message);
                string serviceId = message.Substring("ASSOCIATE:".Length).Trim();
                return AssociateService(clientId, serviceId);
            }
            else if (message.Equals("SAIR", StringComparison.OrdinalIgnoreCase))
            {
                return "400 BYE";
            }
            else if (message.StartsWith("ADMIN:"))
            {
                return HandleAdminMessage(message);
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

    private static string AllocateService(string clientId)
    {
        if (serviceDict.ContainsKey(clientId))
        {
            string service = serviceDict[clientId];
            Console.WriteLine($"Alocando serviço '{service}' para o cliente {clientId}");
            return "SERVICE_ALLOCATED:" + service;
        }
        else
        {
            return "NO_SERVICE_AVAILABLE";
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
                    var parts = line.Split(',');
                    if (parts.Length >= 3)
                    {
                        var taskId = parts[0].Trim();
                        var taskDescription = parts[1].Trim();
                        var taskStatus = parts[2].Trim();
                        var clientId = parts.Length > 3 ? parts[3].Trim() : null;

                        if (taskStatus.Equals("Nao alocada", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!taskDict.ContainsKey(taskId))
                            {
                                taskDict[taskId] = new List<string>();
                            }
                            taskDict[taskId].Add(taskDescription);
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
            string serviceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{service}.csv");
            LoadDataFromCSV(serviceFilePath);

            foreach (var kvp in taskDict)
            {
                foreach (var taskDescription in kvp.Value)
                {
                    if (!IsTaskAllocated(serviceFilePath, kvp.Key, taskDescription))
                    {
                        UpdateTaskCSV(serviceFilePath, kvp.Key, "Em curso", clientId);
                        NotifyClientsOfNewTask(service, taskDescription);
                        return $"TASK_ALLOCATED:{taskDescription}";
                    }
                }
            }
            return "NO_TASK_AVAILABLE";
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    private static bool IsTaskAllocated(string serviceFilePath, string taskId, string taskDescription)
    {
        try
        {
            if (File.Exists(serviceFilePath))
            {
                foreach (var line in File.ReadLines(serviceFilePath).Skip(1))
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 3)
                    {
                        var loadedTaskId = parts[0].Trim();
                        var loadedTaskDescription = parts[1].Trim();
                        var loadedTaskStatus = parts[2].Trim();

                        if (loadedTaskId == taskId && loadedTaskDescription == taskDescription)
                        {
                            return !loadedTaskStatus.Equals("Nao alocada", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"Erro: Arquivo {serviceFilePath} não encontrado.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao verificar se a tarefa está alocada: {ex.Message}");
        }
        return false;
    }

    private static void UpdateTaskCSV(string serviceFilePath, string taskId, string newStatus, string clientId)
    {
        try
        {
            if (File.Exists(serviceFilePath))
            {
                List<string> lines = File.ReadAllLines(serviceFilePath).ToList();

                for (int i = 1; i < lines.Count; i++)
                {
                    string[] parts = lines[i].Split(',');
                    if (parts.Length >= 3 && parts[0].Trim() == taskId)
                    {
                        lines[i] = $"{taskId},{parts[1]},{newStatus},{clientId}";
                        File.WriteAllLines(serviceFilePath, lines);
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Erro: Arquivo {serviceFilePath} não encontrado.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao atualizar o arquivo CSV: {ex.Message}");
        }
    }

    private static string MarkTaskAsCompleted(string clientId, string taskDescription)
    {
        foreach (var serviceId in serviceDict.Values)
        {
            string serviceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{serviceId}.csv");

            try
            {
                if (File.Exists(serviceFilePath))
                {
                    List<string> lines = File.ReadAllLines(serviceFilePath).ToList();

                    for (int i = 1; i < lines.Count; i++)
                    {
                        string[] parts = lines[i].Split(',');
                        if (parts.Length >= 4 && parts[1].Trim() == taskDescription)
                        {
                            parts[2] = "Concluido";
                            lines[i] = string.Join(",", parts);
                            File.WriteAllLines(serviceFilePath, lines);
                            return "TASK_MARKED_COMPLETED";
                        }
                    }
                }
                else
                {
                    return $"ERROR_FILE_NOT_FOUND:{serviceFilePath}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking task as completed: {ex.Message}");
                return "ERROR_MARKING_TASK_COMPLETED";
            }
        }
        return "ERROR_TASK_NOT_FOUND";
    }

    private static string SubscribeClientToService(string clientId, string serviceId)
    {
        mutex.WaitOne();
        try
        {
            if (!clientSubscriptions.ContainsKey(serviceId))
            {
                clientSubscriptions[serviceId] = new List<string>();
            }
            if (!clientSubscriptions[serviceId].Contains(clientId))
            {
                clientSubscriptions[serviceId].Add(clientId);
                Console.WriteLine($"Client {clientId} subscribed to service {serviceId}");
                return $"SUBSCRIBED_TO_SERVICE:{serviceId}";
            }
            else
            {
                return $"ALREADY_SUBSCRIBED:{serviceId}";
            }
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    private static void NotifyClientsOfNewTask(string serviceId, string taskDescription)
    {
        if (clientSubscriptions.ContainsKey(serviceId))
        {
            foreach (var clientId in clientSubscriptions[serviceId])
            {
                if (clientConnections.ContainsKey(clientId))
                {
                    TcpClient client = clientConnections[clientId];
                    if (client.Connected)
                    {
                        try
                        {
                            using (StreamWriter writer = new StreamWriter(client.GetStream()) { AutoFlush = true })
                            {
                                writer.WriteLine($"NEW_TASK:{taskDescription}");
                                Console.WriteLine($"Notified client {clientId} of new task: {taskDescription}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to notify client {clientId}: {ex.Message}");
                        }
                    }
                }
            }
        }
    }

    private static string HandleAdminMessage(string message)
    {
        try
        {
            if (message.StartsWith("ADMIN:CREATE_TASK", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = message.Split(':');
                if (parts.Length >= 4)
                {
                    string adminId = parts[1].Trim();
                    string serviceId = parts[2].Trim();
                    string taskDescription = parts[3].Trim();

                    if (serviceDict.ContainsKey(adminId) && serviceDict[adminId] == serviceId)
                    {
                        string taskId = Guid.NewGuid().ToString();
                        string taskFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{serviceId}.csv");
                        string newTaskLine = $"{taskId},{taskDescription},Nao alocada";

                        File.AppendAllText(taskFilePath, Environment.NewLine + newTaskLine);
                        NotifyClientsOfNewTask(serviceId, taskDescription);

                        return $"TASK_CREATED:{taskId}";
                    }
                    else
                    {
                        return "ERROR:ADMIN_NOT_ASSOCIATED_WITH_SERVICE";
                    }
                }
                else
                {
                    return "ERROR:INVALID_ADMIN_CREATE_TASK_COMMAND";
                }
            }
            else if (message.StartsWith("ADMIN:ALLOCATE_BIKE", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = message.Split(':');
                if (parts.Length >= 4)
                {
                    string adminId = parts[1].Trim();
                    string bikeId = parts[2].Trim();
                    string serviceId = parts[3].Trim();

                    if (serviceDict.ContainsKey(adminId) && serviceDict[adminId] == serviceId)
                    {
                        // Allocate the bike to the service (Assuming you have a method to do this)
                        // For now, just add a placeholder logic
                        Console.WriteLine($"Allocating bike {bikeId} to service {serviceId} by admin {adminId}");
                        return $"BIKE_ALLOCATED:{bikeId}";
                    }
                    else
                    {
                        return "ERROR:ADMIN_NOT_ASSOCIATED_WITH_SERVICE";
                    }
                }
                else
                {
                    return "ERROR:INVALID_ADMIN_ALLOCATE_BIKE_COMMAND";
                }
            }
            else if (message.StartsWith("ADMIN:QUERY_INFO", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = message.Split(':');
                if (parts.Length >= 3)
                {
                    string adminId = parts[1].Trim();
                    string queryType = parts[2].Trim();

                    if (serviceDict.ContainsKey(adminId))
                    {
                        string serviceId = serviceDict[adminId];
                        string serviceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{serviceId}.csv");

                        if (queryType.Equals("TASKS", StringComparison.OrdinalIgnoreCase))
                        {
                            if (File.Exists(serviceFilePath))
                            {
                                string[] lines = File.ReadAllLines(serviceFilePath);
                                return $"TASKS_INFO:{string.Join(";", lines.Skip(1))}";
                            }
                            else
                            {
                                return $"ERROR:FILE_NOT_FOUND:{serviceFilePath}";
                            }
                        }
                        else
                        {
                            return "ERROR:UNKNOWN_QUERY_TYPE";
                        }
                    }
                    else
                    {
                        return "ERROR:ADMIN_NOT_ASSOCIATED_WITH_SERVICE";
                    }
                }
                else
                {
                    return "ERROR:INVALID_ADMIN_QUERY_INFO_COMMAND";
                }
            }
            else
            {
                return "ERROR:UNKNOWN_ADMIN_COMMAND";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing admin message: {ex}");
            return "500 ERROR: Internal server error";
        }
    }

    private static string DisassociateService(string clientId)
    {
        if (serviceDict.ContainsKey(clientId))
        {
            serviceDict.Remove(clientId);
            Console.WriteLine($"Cliente {clientId} desassociado do serviço.");
            return "DISASSOCIATED_FROM_SERVICE";
        }
        else
        {
            return "ERROR:CLIENT_NOT_ASSOCIATED_WITH_SERVICE";
        }
    }


    private static string AssociateService(string clientId, string serviceId)
    {
        serviceDict[clientId] = serviceId;
        Console.WriteLine($"Cliente {clientId} associado ao serviço {serviceId}.");
        return "ASSOCIATED_WITH_SERVICE";
    }
}