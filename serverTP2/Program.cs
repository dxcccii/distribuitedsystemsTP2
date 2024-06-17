using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices.Expando;
using System.Text;
using System.Threading;
using System;

Федя
dxcccii
Invisible

Федя — 06/14/2024 6:49 PM
tou com a gripe de verao
Cari0 — 06/14/2024 6:58 PM
Summer love 💟 fever
Федя — 06/14/2024 7:00 PM
voutomar outro brufen para me doer menos o corpo
Cari0 — 06/14/2024 7:02 PM
Toma babus
Федя — 06/14/2024 7:07 PM
?
Федя — 06/14/2024 7:41 PM
9, 9 e meia estou aqui
Федя — 06/14/2024 8:10 PM
did yu go back to bed?
Cari0 — 06/14/2024 8:10 PM
Yes
Федя — 06/14/2024 8:10 PM
oke
9 e 30
oke? 
Cari0 — 06/14/2024 8:11 PM
I will try
Федя — 06/14/2024 8:35 PM
I cant fall asleep
If you're not here when i wake up
ill start setting this up, o repositorio no git, o read me, e o admin e as passwords pelomenos isso.... no idea how we'll do forms
guess we'll both be a little late
its ok
Федя — 06/14/2024 11:50 PM
Up
Cari0 — 06/14/2024 11:50 PM
alou
Федя — 06/14/2024 11:55 PM
heyo
Федя — 06/15/2024 12:11 AM
here
Федя
 started a call that lasted 5 hours.
 — 06/15/2024 12:12 AM
Федя — 06/15/2024 1:53 AM
Attachment file type: acrobat
protocolo_tp2_2324.pdf
214.86 KB
Федя — 06/15/2024 2:03 AM
https://github.com/dxcccii/distributedsystemsTP1/blob/master/TP1server/Program.cs
GitHub
distributedsystemsTP1/TP1server/Program.cs at master · dxcccii/dist...
1st project for distributed systems class (C#). Contribute to dxcccii/distributedsystemsTP1 development by creating an account on GitHub.
distributedsystemsTP1/TP1server/Program.cs at master · dxcccii/dist...
Cari0 — 06/15/2024 2:15 AM
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
Expand
message.txt
16 KB
private static string HandleAdminMessage(string message)
{
    try
    {
        if (message.StartsWith("ADMIN:CREATE_TASK", StringComparison.OrdinalIgnoreCase))
        {
            Expand
message.txt
5 KB
Cari0 — 06 / 15 / 2024 2:39 AM
using System;                  // Provides basic functionalities like console input and output
using System.Diagnostics;      // Provides classes for interacting with system processes
using System.IO;               // Provides classes for reading and writing to files
using System.Net.Sockets;      // Provides classes for creating TCP/IP client and server applications
using System.Threading;        // Provides classes for threading, including Thread.Sleep
Expand
message.txt
11 KB
Cari0 — 06/15/2024 3:19 AM
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

class Cliente
Expand
message.txt
8 KB
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
Expand
message.txt
23 KB
Cari0 — 06/15/2024 4:21 AM
Image
Федя — 06/15/2024 9:46 PM
tas?
liga me qd acordares
Федя — 06/15/2024 11:27 PM
Image
Cari0 — Yesterday at 1:03 AM
im here
Федя — Yesterday at 1:08 AM
I called you remember? Told you I have a fever... Just asked you to do some simple things, add the admin to the CSV file,.make the server present a new UI in the console(options, where the admin can do their tasks... Etc) if you could do that or "try" that would be a big help. I am legitimately sick. Gripe de verão. Everybody on my house is. I added you as a collaborator on the git repository so you can clone it to a folder of your choice and don't lose it. Just copy and paste the normal CSV files onto the servers debug folder, but I think they are already there (?)
Até o meu tio disse para eu respirar fundo, ir dormir e amanhã sentir me com mais força para trabalhar because I looked miserable.
I know you think I'm being lazy and a coward... Unfortunately (or fortunately) not the case
If I ever really needed your help, is now
Федя — Yesterday at 1:12 AM
Suggestions I made to how to add the admin to the alocar_servico_cliente.csv file
Mas o servidor tem de reconhecer que o.admin tem opções diferentes ( o 1.2 3 4 que aparece do lado do cliente para selecionar) que um cliente normal
Shouldn't be that hard. Os metodos para as acções do administrador posso tratar amanhã
Unless.... Jk
I told you at the end of the phone call to sleep more if you wanted to. I'm only awake still because I have a huge migraine 
Feel free to tell me to go fuck myself... But I'd appreciate your help
Please say something.....
Cari0 — Yesterday at 1:19 AM
is oke
Федя — Yesterday at 1:30 AM
I love you babus
Like a lot
Cari0 — Yesterday at 7:23 AM
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

class Cliente
Expand
message.txt
9 KB
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
Expand
message.txt
18 KB
ClienteID, ServicoID, Tipo
Cl_0001, Servico_A, Cliente
Cl_0002, Servico_A, Cliente
Cl_0003, Servico_A, Cliente
Cl_0004, Servico_B, Cliente
Cl_0005, Servico_B, Cliente
Expand
Alocacao_Cliente_Servico.csv
1 KB
awooga
Федя — Yesterday at 11:43 AM
tysmmmmmmm
Cari0 — Yesterday at 4:39 PM
Oi
Федя — Yesterday at 4:39 PM
Oiii
Федя
 started a call that lasted 3 minutes.
 — Yesterday at 5:25 PM
Федя — Yesterday at 5:26 PM
babs
Федя — Yesterday at 5:36 PM
Image
nao ha tipo de cliente, sao todos clientes, o servidor tem de reconhecer o admin apenas pelo client id
also vi que tens 2 admins no ficheiro csv, so pode haver 1.... e nao pode estar alocado a 1 servico especifico, o servico do admin e a administracao 
Cari0
 started a call that lasted 4 hours.
 — Yesterday at 5:39 PM
Федя — Yesterday at 7:24 PM
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
Expand
message.txt
16 KB
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

class Cliente
Expand
message.txt
9 KB
Attachment file type: unknown
taskmanager.proto
514 bytes
https://chatgpt.com/share/dd11c754-89f5-4b46-a791-0df8e008d819
ChatGPT
A conversational AI system that listens, learns, and challenges
Image
Cari0 — Yesterday at 7:46 PM
https://www.rabbitmq.com/docs/install-windows
Installing on Windows | RabbitMQ
Федя — Yesterday at 8:03 PM
Image
<ItemGroup>
  < PackageReference Include = "Grpc.Core" Version = "2.45.0" />
  < PackageReference Include = "RabbitMQ.Client" Version = "6.4.1" />
  < !--Include other packages as needed -->
</ItemGroup>
Федя — Yesterday at 9:47 PM
https://chatgpt.com/share/a90d4b80-18b0-4117-a2eb-683557520173
ChatGPT
A conversational AI system that listens, learns, and challenges
Image
Cari0 — Today at 3:39 AM
Hey
Федя — Today at 3:39 AM...
Федя — Today at 3:49 AM
going back to bed?
Cari0 — Today at 3:50 AM
no
Федя — Today at 3:50 AM
surprising!
tu contas como os caloiros?
1 + 3, 4, 1 + 5
realmente chegaste a 1 + 2
mais bem disposto?
Федя
 started a call.
 — Today at 3:53 AM
Федя — Today at 3:56 AM
Image
server
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
Expand
message.txt
15 KB
client
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

class Cliente
Expand
message.txt
10 KB
Федя — Today at 4:08 AM
Image
Image
Image
Cari0 — Today at 4:17 AM
service_allocations
Федя — Today at 5:16 AM
client:
case "2":
    escritor.WriteLine("CONSULT_TASKS");
    string respostaConsultTasks;
    while ((respostaConsultTasks = leitor.ReadLine()) != null)
    {
        if (respostaConsultTasks == "<END_OF_RESPONSE>")
        {
            break;
        }
        Console.WriteLine("Resposta do servidor: " + respostaConsultTasks);
    }
    Thread.Sleep(1000);
    break;
server:
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
    Cari0 — Today at 5:18 AM
using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    Expand
message.txt
15 KB
Федя — Today at 5:30 AM
Image
Cari0 — Today at 5:32 AM
private static string ChangeTaskStatus(string serviceFilePath, string taskDescription, string newStatus, string additionalField)
    {
        try
        {
            // Validate taskDescription
            if (string.IsNullOrEmpty(taskDescription))
                Expand
message.txt
3 KB
Cari0 — Today at 5:43 AM
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

... (367 lines left)
Collapse
message.txt
17 KB
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

class Cliente
Expand
message.txt
10 KB
Cari0 — Today at 6:39 AM
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
Expand
message.txt
17 KB
Cari0 — Today at 6:46 AM
Image
Cari0 — Today at 7:42 AM
private static bool IsValidStatus(string status)
{
    // Define your validation rules here
    // For example, if status can only be one of a predefined set:
    string[] validStatuses = { "nao alocada", "concluido", "em progresso" };
    return validStatuses.Contains(status.ToLower());
}
private static string ChangeTaskStatus(string serviceFilePath, string taskDescription, string newStatus, string additionalField)
{
    try
    {
        // Validate taskDescription
        if (string.IsNullOrEmpty(taskDescription))
            Expand
message.txt
3 KB
using System;
        using System.Collections.Generic;
        using System.IO;
        using System.Linq;
        using System.Net;
        using System.Net.Sockets;
        Expand
message.txt
17 KB
Cari0 — Today at 9:40 AM
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
Expand
message.txt
20 KB
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

class Cliente
Expand
message.txt
10 KB
﻿
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
message.txt
20 KB