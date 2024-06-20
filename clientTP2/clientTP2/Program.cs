using System; // Importing System namespace for basic functionalities
using System.Collections.Generic; // Importing System.Collections.Generic namespace for using collections like Dictionary
using System.Text; // Importing System.Text namespace for text encoding
using RabbitMQ.Client; // Importing RabbitMQ.Client namespace for RabbitMQ functionalities
using RabbitMQ.Client.Events; // Importing RabbitMQ.Client.Events namespace for RabbitMQ event-based consumer

class Cliente // Declaring a class named Cliente
{
    private static IConnection rabbitConnection; // Declaring a static variable for RabbitMQ connection
    private static IModel rabbitChannel; // Declaring a static variable for RabbitMQ channel
    private static string replyQueueName; // Declaring a static variable for reply queue name
    private static EventingBasicConsumer consumer; // Declaring a static variable for RabbitMQ consumer
    private static string correlationId; // Declaring a static variable for correlation ID
    private static IBasicProperties props; // Declaring a static variable for basic properties of RabbitMQ messages

    private static Dictionary<string, string> subscribedServices = new Dictionary<string, string>(); // Declaring a static dictionary to store subscribed services
    private static IConnection notificationConnection; // Declaring a static variable for notification connection
    private static IModel notificationChannel; // Declaring a static variable for notification channel

    static void Main(string[] args) // Main method, entry point of the program
    {
        Console.WriteLine("Bem-vindo à ServiMoto!"); // Printing welcome message
        Console.WriteLine($"\n"); // Printing a new line
        Console.Write("Por favor, insira o endereço IP do servidor: "); // Prompting the user to enter the server IP address
        string enderecoServidor = Console.ReadLine(); // Reading the server IP address from the user input

        try
        {
            InitRabbitMQ(enderecoServidor); // Initializing RabbitMQ with the server IP address

            Console.WriteLine("Conectado ao servidor. Aguardando resposta..."); // Printing a message indicating connection to the server

            string response = Call("CONNECT"); // Sending "CONNECT" message to the server and waiting for a response
            Console.WriteLine("Resposta do servidor: " + response); // Printing the server's response

            if (response == "100 OK") // Checking if the response is "100 OK"
            {
                Console.WriteLine($"\n"); // Printing a new line
                Console.Write("Por favor, insira o seu ID: "); // Prompting the user to enter their ID
                string idCliente = Console.ReadLine(); // Reading the client ID from the user input
                response = Call($"CLIENT_ID:{idCliente}"); // Sending "CLIENT_ID" message to the server and waiting for a response
                Console.WriteLine("Resposta: " + response); // Printing the server's response

                if (response.StartsWith("ID_CONFIRMED")) // Checking if the response starts with "ID_CONFIRMED"
                {
                    Console.WriteLine($"\n"); // Printing a new line
                    Console.Write("Por favor, insira a sua password: "); // Prompting the user to enter their password
                    string senha = Console.ReadLine(); // Reading the password from the user input
                    response = Call($"PASSWORD:{idCliente},{senha}"); // Sending "PASSWORD" message to the server and waiting for a response
                    Console.WriteLine("Resposta: " + response); // Printing the server's response

                    if (response == "PASSWORD_CONFIRMED") // Checking if the response is "PASSWORD_CONFIRMED"
                    {
                        InitNotificationListener(); // Initializing the notification listener
                        StartNotificationListener(idCliente); // Starting the notification listener with the client ID

                        if (idCliente.StartsWith("Adm")) // Checking if the client ID starts with "Adm"
                        {
                            while (true)
                            {
                                Console.WriteLine($"\n"); // Printing a new line
                                Console.Write("Por favor, insira o ID do serviço que quer gerir (e.g., Servico_X): "); // Prompting the user to enter the service ID to manage
                                string adminServiceId = Console.ReadLine(); // Reading the admin service ID from the user input
                                response = Call($"ADMIN_SERVICE_ID:{adminServiceId}"); // Sending "ADMIN_SERVICE_ID" message to the server and waiting for a response
                                Console.WriteLine("Resposta do servidor: " + response); // Printing the server's response

                                if (response == "SERVICE_CONFIRMED") // Checking if the response is "SERVICE_CONFIRMED"
                                {
                                    AdministradorMenu(idCliente, adminServiceId); // Calling the administrator menu with the client ID and admin service ID
                                    break; // Breaking the loop
                                }
                                else if (response == "SERVICE_NOT_FOUND") // Checking if the response is "SERVICE_NOT_FOUND"
                                {
                                    Console.WriteLine(response); // Printing the response
                                }
                                else
                                {
                                    Console.WriteLine("Resposta do servidor desconhecida: " + response); // Printing an unknown server response
                                    break; // Breaking the loop
                                }
                            }
                        }
                        else
                        {
                            ClienteMenu(idCliente); // Calling the client menu with the client ID
                        }
                    }
                    else
                    {
                        Console.WriteLine("Autenticação falhou."); // Printing authentication failed message
                    }
                }
            }

            Console.WriteLine("Comunicação com o servidor encerrada."); // Printing communication with server closed message
        }
        catch (Exception ex) // Catching any exceptions
        {
            Console.WriteLine("Ocorreu um erro: " + ex.Message); // Printing the error message
        }
        finally
        {
            rabbitChannel.Close(); // Closing the RabbitMQ channel
            rabbitConnection.Close(); // Closing the RabbitMQ connection
            Environment.Exit(0); // Exiting the application
        }
    }

    private static void InitRabbitMQ(string enderecoServidor)
    {
        var factory = new ConnectionFactory() { HostName = enderecoServidor }; // Creating a connection factory with the server IP address
        rabbitConnection = factory.CreateConnection(); // Creating a RabbitMQ connection
        rabbitChannel = rabbitConnection.CreateModel(); // Creating a RabbitMQ channel
        replyQueueName = rabbitChannel.QueueDeclare().QueueName; // Declaring a reply queue and getting its name
        consumer = new EventingBasicConsumer(rabbitChannel); // Creating a new EventingBasicConsumer for the channel
        correlationId = Guid.NewGuid().ToString(); // Generating a new unique correlation ID
        props = rabbitChannel.CreateBasicProperties(); // Creating basic properties for the RabbitMQ message
        props.CorrelationId = correlationId; // Setting the correlation ID in the properties
        props.ReplyTo = replyQueueName; // Setting the reply queue name in the properties
    }

    private static string Call(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message); // Converting the message to a byte array
        rabbitChannel.BasicPublish(exchange: "", routingKey: "rpc_queue", basicProperties: props, body: messageBytes); // Publishing the message to the RabbitMQ queue

        var responseReceived = false; // Flag to check if the response is received
        string response = null; // Variable to store the response

        consumer.Received += (model, ea) => // Event handler for received messages
        {
            if (ea.BasicProperties.CorrelationId == correlationId) // Checking if the correlation ID matches
            {
                response = Encoding.UTF8.GetString(ea.Body.ToArray()); // Decoding the response body
                responseReceived = true; // Setting the flag to true
            }
        };

        rabbitChannel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: consumer); // Consuming messages from the reply queue

        while (!responseReceived) // Waiting for the response
        {
            // Busy-wait for the response
        }

        return response; // Returning the response
    }

    private static void ClienteMenu(string idCliente)
    {
        while (true)
        {
            Console.WriteLine("\n");
            Console.WriteLine("1. Solicitar tarefa");
            Console.WriteLine("2. Marcar tarefa como concluída");
            Console.WriteLine("3. Subscrever a um servico");
            Console.WriteLine("4. Cancelar a subscricao as notificacoes de um servico");
            Console.WriteLine("5. Sair");
            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            string response;

            switch (opcao)
            {
                case "1":
                    response = Call($"REQUEST_TASK|{idCliente}"); // Sending a request task message to the server
                    Console.WriteLine("Resposta do servidor: " + response); // Printing the server's response
                    break;

                case "2":
                    Console.Write("Por favor, insira a descrição da tarefa concluída: "); // Prompting the user to enter the task description
                    string descricaoTarefa = Console.ReadLine(); // Reading the task description from the user input
                    response = Call($"TASK_COMPLETED|{idCliente}|{descricaoTarefa}"); // Sending a task completed message to the server
                    Console.WriteLine("Resposta do servidor: " + response); // Printing the server's response
                    break;

                case "3":
                    Console.Write("Por favor, insira o ID do serviço para subscrever: "); // Prompting the user to enter the service ID to subscribe
                    string serviceIdSubscribe = Console.ReadLine(); // Reading the service ID from the user input
                    response = Call($"SUBSCRIBE|{idCliente}|{serviceIdSubscribe}"); // Sending a subscribe message to the server
                    if (response == "SUBSCRIBED") // Checking if the response is "SUBSCRIBED"
                    {
                        subscribedServices[serviceIdSubscribe] = $"{idCliente}_{serviceIdSubscribe}"; // Adding the service to subscribed services
                        BindNotificationQueue(idCliente, serviceIdSubscribe); // Binding the notification queue
                        Console.WriteLine($"Subscribed to service: {serviceIdSubscribe}"); // Printing the subscription message
                    }
                    Console.WriteLine("Resposta do servidor: " + response); // Printing the server's response
                    break;

                case "4":
                    Console.Write("Por favor, insira o ID do serviço para cancelar a subscricao: "); // Prompting the user to enter the service ID to unsubscribe
                    string serviceIdUnsubscribe = Console.ReadLine(); // Reading the service ID from the user input
                    response = Call($"UNSUBSCRIBE|{idCliente}|{serviceIdUnsubscribe}"); // Sending an unsubscribe message to the server
                    if (response == "UNSUBSCRIBED") // Checking if the response is "UNSUBSCRIBED"
                    {
                        UnbindNotificationQueue(serviceIdUnsubscribe); // Unbinding the notification queue
                        subscribedServices.Remove(serviceIdUnsubscribe); // Removing the service from subscribed services
                        Console.WriteLine($"Unsubscribed from service: {serviceIdUnsubscribe}"); // Printing the unsubscription message
                    }
                    Console.WriteLine("Resposta do servidor: " + response); // Printing the server's response
                    break;

                case "5":
                    return; // Returning from the method to exit the menu

                default:
                    Console.WriteLine("Opção inválida. Por favor, tente novamente."); // Printing invalid option message
                    break;
            }
        }
    }

    private static void AdministradorMenu(string idCliente, string adminServiceId)
    {
        while (true)
        {
            Console.WriteLine("\n");
            Console.WriteLine("1. Criar nova tarefa");
            Console.WriteLine("2. Consultar tarefas");
            Console.WriteLine("3. Alterar status da tarefa");
            Console.WriteLine("4. Sair");
            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            string response;

            switch (opcao)
            {
                case "1":
                    Console.Write("Por favor, insira a descrição da nova tarefa com o formato SX_TNUMERO DA TAREFA,DESCRICAO DA TAREFA: "); // Prompting the user to enter the task description
                    string descricaoTarefa = Console.ReadLine(); // Reading the task description from the user input
                    response = Call($"ADD_TASK|{adminServiceId}|{descricaoTarefa}"); // Sending an add task message to the server
                    Console.WriteLine("Resposta do servidor: " + response); // Printing the server's response
                    break;

                case "2":
                    response = Call($"CONSULT_TASKS|{adminServiceId}"); // Sending a consult tasks message to the server
                    Console.WriteLine($"\nTarefas no {adminServiceId}:\n{response}"); // Printing the server's response
                    break;

                case "3":
                    Console.Write("Por favor, insira a descrição da tarefa a ser alterada: "); // Prompting the user to enter the task description
                    string taskDescription = Console.ReadLine(); // Reading the task description from the user input

                    Console.Write("Insira o novo status da tarefa: "); // Prompting the user to enter the new status
                    string newStatus = Console.ReadLine(); // Reading the new status from the user input

                    Console.Write("Insira o campo adicional: "); // Prompting the user to enter the additional field
                    string additionalField = Console.ReadLine(); // Reading the additional field from the user input

                    response = Call($"CHANGE_TASK_STATUS|{adminServiceId}|{taskDescription},{newStatus},{additionalField}"); // Sending a change task status message to the server
                    Console.WriteLine("Resposta do servidor: " + response); // Printing the server's response
                    break;

                case "4":
                    return; // Returning from the method to exit the menu

                default:
                    Console.WriteLine("Opção inválida. Por favor, tente novamente."); // Printing invalid option message
                    break;
            }
        }
    }

    private static void InitNotificationListener()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" }; // Creating a connection factory with the host name "localhost"
        notificationConnection = factory.CreateConnection(); // Creating a RabbitMQ connection for notifications
        notificationChannel = notificationConnection.CreateModel(); // Creating a RabbitMQ channel for notifications
        notificationChannel.ExchangeDeclare(exchange: "service_notifications", type: ExchangeType.Topic); // Declaring an exchange for service notifications
        Console.WriteLine("Notification listener initialized."); // Printing notification listener initialized message
    }

    private static void StartNotificationListener(string clientId)
    {
        foreach (var serviceId in subscribedServices.Keys) // Iterating through subscribed services
        {
            BindNotificationQueue(clientId, serviceId); // Binding the notification queue for each service
        }

        Console.WriteLine("Client is waiting for notifications..."); // Printing waiting for notifications message
    }

    private static void BindNotificationQueue(string clientId, string serviceId)
    {
        var queueName = $"{clientId}_{serviceId}"; // Constructing the queue name
        notificationChannel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null); // Declaring the queue
        notificationChannel.QueueBind(queue: queueName, exchange: "service_notifications", routingKey: $"NOTIFICATION.{serviceId}"); // Binding the queue to the exchange

        var consumer = new EventingBasicConsumer(notificationChannel); // Creating a new EventingBasicConsumer for notifications
        consumer.Received += (model, ea) => // Event handler for received notifications
        {
            var body = ea.Body.ToArray(); // Getting the body of the message
            var message = Encoding.UTF8.GetString(body); // Decoding the message
            if (!message.StartsWith("UNSUBSCRIBE:")) // Checking if the message does not start with "UNSUBSCRIBE:"
            {
                Console.WriteLine($"\n");
                Console.WriteLine($"\nReceived notification for {serviceId}: {message}"); // Printing the received notification
            }
            else
            {
                var parts = message.Split('-'); // Splitting the message by "-"
                if (parts.Length == 3 && parts[1] == clientId && parts[2] == serviceId) // Checking if the message format matches
                {
                    notificationChannel.QueueUnbind(queue: queueName, exchange: "service_notifications", routingKey: $"NOTIFICATION.{serviceId}"); // Unbinding the queue from the exchange
                    Console.WriteLine($"Unbound from notifications for service: {serviceId}"); // Printing the unbound message
                }
            }
        };

        notificationChannel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer); // Consuming messages from the queue
        Console.WriteLine($"Subscribed to notifications for service: {serviceId}"); // Printing the subscribed message
    }

    private static void UnbindNotificationQueue(string serviceId)
    {
        if (subscribedServices.ContainsKey(serviceId)) // Checking if the service is in the subscribed services
        {
            var queueName = subscribedServices[serviceId]; // Getting the queue name
            notificationChannel.QueueUnbind(queue: queueName, exchange: "service_notifications", routingKey: $"NOTIFICATION.{serviceId}"); // Unbinding the queue from the exchange
            Console.WriteLine($"Unbound from notifications for service: {serviceId}"); // Printing the unbound message
        }
    }
}

