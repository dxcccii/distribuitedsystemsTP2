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
using System.IO;
using System.Net.Sockets;
using System.Threading;

class Cliente
{
    static void Main(string[] args)
    {
        Console.WriteLine("Bem-vindo à ServiMoto!");

        Console.Write("Por favor, insira o endereço IP do servidor: ");
        string enderecoServidor = Console.ReadLine();

        try
        {
            while (true)
            {
                using (TcpClient cliente = new TcpClient(enderecoServidor, 1234))
                using (NetworkStream stream = cliente.GetStream())
                using (StreamReader leitor = new StreamReader(stream))
                using (StreamWriter escritor = new StreamWriter(stream) { AutoFlush = true })
                {
                    Console.WriteLine("Conectado ao servidor. Aguardando resposta...");

                    escritor.WriteLine("CONNECT");
                    string resposta = leitor.ReadLine();
                    Console.WriteLine("Resposta do servidor: " + resposta);
                    Thread.Sleep(1000);

                    if (resposta == "100 OK")
                    {
                        Console.Write("Por favor, insira o seu ID de cliente: ");
                        string idCliente = Console.ReadLine();

                        escritor.WriteLine($"CLIENT_ID:{idCliente}");
                        resposta = leitor.ReadLine();
                        Console.WriteLine("Resposta: " + resposta);
                        Thread.Sleep(1000);

                        if (resposta.StartsWith("ID_CONFIRMED"))
                        {
                            if (idCliente.StartsWith("Adm"))
                            {
                                while (true)
                                {
                                    Console.Write("Por favor, insira o ID do serviço que você deseja gerenciar (e.g., Servico_A): ");
                                    string adminServiceId = Console.ReadLine();
                                    escritor.WriteLine($"ADMIN_SERVICE_ID:{adminServiceId}");
                                    resposta = leitor.ReadLine();
                                    Console.WriteLine("Resposta do servidor: " + resposta);

                                    if (resposta == "SERVICE_CONFIRMED")
                                    {
                                        AdministradorMenu(escritor, leitor, idCliente, adminServiceId);
                                        break;
                                    }
                                    else if (resposta == "SERVICE_NOT_FOUND")
                                    {
                                        Console.WriteLine(resposta);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Resposta do servidor desconhecida: " + resposta);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                ClienteMenu(escritor, leitor, idCliente);
                            }
                        }
                    }
                }
                Console.WriteLine("Comunicação com o servidor encerrada.");
                break;
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine("Ocorreu um erro de E/S: " + ex.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ocorreu um erro: " + ex.Message);
        }
        finally
        {
            Environment.Exit(0);
        }
    }

    private static void ClienteMenu(StreamWriter escritor, StreamReader leitor, string idCliente)
    {
        while (true)
        {
            Console.WriteLine("1. Solicitar tarefa");
            Console.WriteLine("2. Marcar tarefa como concluída");
            Console.WriteLine("3. Assinar notificações");
            Console.WriteLine("4. Cancelar assinatura de notificações");
            Console.WriteLine("5. Sair");
            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    escritor.WriteLine("REQUEST_TASK");
                    string respostaRequestTask = leitor.ReadLine();
                    Console.WriteLine("Resposta do servidor: " + respostaRequestTask);
                    Thread.Sleep(1000);
                    break;

                case "2":
                    Console.Write("Por favor, insira a descrição da tarefa concluída: ");
                    string descricaoTarefa = Console.ReadLine();
                    escritor.WriteLine("TASK_COMPLETED:" + descricaoTarefa);
                    string respostaTaskCompleted = leitor.ReadLine();
                    Console.WriteLine("Resposta do servidor: " + respostaTaskCompleted);
                    Thread.Sleep(1000);
                    break;

                case "3":
                    Console.Write("Por favor, insira o ID do serviço para assinar: ");
                    string serviceIdSubscribe = Console.ReadLine();
                    escritor.WriteLine("SUBSCRIBE:" + serviceIdSubscribe);
                    string respostaSubscribe = leitor.ReadLine();
                    Console.WriteLine("Resposta do servidor: " + respostaSubscribe);
                    Thread.Sleep(1000);
                    break;

                case "4":
                    Console.Write("Por favor, insira o ID do serviço para cancelar a assinatura: ");
                    string serviceIdUnsubscribe = Console.ReadLine();
                    escritor.WriteLine("UNSUBSCRIBE:" + serviceIdUnsubscribe);
                    string respostaUnsubscribe = leitor.ReadLine();
                    Console.WriteLine("Resposta do servidor: " + respostaUnsubscribe);
                    Thread.Sleep(1000);
                    break;

                case "5":
                    return;

                default:
                    Console.WriteLine("Opção inválida. Por favor, tente novamente.");
                    break;
            }
        }
    }

    private static void AdministradorMenu(StreamWriter escritor, StreamReader leitor, string idCliente, string adminServiceId)
    {
        while (true)
        {
            Console.WriteLine("1. Criar nova tarefa");
            Console.WriteLine("2. Consultar tarefas");
            Console.WriteLine("3. Alterar status da tarefa");
            Console.WriteLine("4. Sair");
            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    Console.Write("Por favor, insira a descrição da nova tarefa: ");
                    string descricaoTarefa = Console.ReadLine();
                    escritor.WriteLine($"ADD_TASK:{descricaoTarefa}");
                    string respostaAddTask = leitor.ReadLine();
                    Console.WriteLine("Resposta do servidor: " + respostaAddTask);
                    Thread.Sleep(1000);
                    break;

                case "2":
                    escritor.WriteLine("CONSULT_TASKS");
                    string respostaConsultTasks;
                    while ((respostaConsultTasks = leitor.ReadLine()) != null)
                    {
                        if (respostaConsultTasks == "<END_OF_RESPONSE>")
                        {
                            break;
                        }
                        Console.WriteLine(respostaConsultTasks);
                    }
                    Thread.Sleep(1000);
                    break;

                case "3":
                    Console.Write("Por favor, insira a descrição da tarefa a ser alterada: ");
                    string taskDescription = Console.ReadLine();

                    Console.Write("Insira o novo status da tarefa: ");
                    string newStatus = Console.ReadLine();

                    Console.Write("Insira o campo adicional: ");
                    string additionalField = Console.ReadLine();

                    escritor.WriteLine($"CHANGE_TASK_STATUS:{taskDescription},{newStatus},{additionalField}");
                    string respostaChangeStatus = leitor.ReadLine();
                    Console.WriteLine("Resposta do servidor: " + respostaChangeStatus);
                    Thread.Sleep(1000);
                    break;

                case "4":
                    return;

                default:
                    Console.WriteLine("Opção inválida. Por favor, tente novamente.");
                    break;
            }
        }
    }
}
