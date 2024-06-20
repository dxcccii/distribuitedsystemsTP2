using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Cliente
{
    private static IConnection rabbitConnection;
    private static IModel rabbitChannel;
    private static string replyQueueName;
    private static EventingBasicConsumer consumer;
    private static string correlationId;
    private static IBasicProperties props;

    private static Dictionary<string, string> subscribedServices = new Dictionary<string, string>();
    private static IConnection notificationConnection;
    private static IModel notificationChannel;

    static void Main(string[] args)
    {
        Console.WriteLine("Bem-vindo à ServiMoto!");

        Console.Write("Por favor, insira o endereço IP do servidor RabbitMQ: ");
        string enderecoServidor = Console.ReadLine();

        try
        {
            InitRabbitMQ(enderecoServidor);

            Console.WriteLine("Conectado ao servidor. Aguardando resposta...");

            string response = Call("CONNECT");
            Console.WriteLine("Resposta do servidor: " + response);

            if (response == "100 OK")
            {
                Console.Write("Por favor, insira o seu ID de cliente: ");
                string idCliente = Console.ReadLine();
                response = Call($"CLIENT_ID:{idCliente}");
                Console.WriteLine("Resposta: " + response);

                if (response.StartsWith("ID_CONFIRMED"))
                {
                    Console.Write("Por favor, insira a sua senha: ");
                    string senha = Console.ReadLine();
                    response = Call($"PASSWORD:{idCliente},{senha}");
                    Console.WriteLine("Resposta: " + response);

                    if (response == "PASSWORD_CONFIRMED")
                    {
                        InitNotificationListener();
                        StartNotificationListener(idCliente);

                        if (idCliente.StartsWith("Adm"))
                        {
                            while (true)
                            {
                                Console.Write("Por favor, insira o ID do serviço que você deseja gerenciar (e.g., Servico_X): ");
                                string adminServiceId = Console.ReadLine();
                                response = Call($"ADMIN_SERVICE_ID:{adminServiceId}");
                                Console.WriteLine("Resposta do servidor: " + response);

                                if (response == "SERVICE_CONFIRMED")
                                {
                                    AdministradorMenu(idCliente, adminServiceId);
                                    break;
                                }
                                else if (response == "SERVICE_NOT_FOUND")
                                {
                                    Console.WriteLine(response);
                                }
                                else
                                {
                                    Console.WriteLine("Resposta do servidor desconhecida: " + response);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            ClienteMenu(idCliente);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Autenticação falhou.");
                    }
                }
            }

            Console.WriteLine("Comunicação com o servidor encerrada.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ocorreu um erro: " + ex.Message);
        }
        finally
        {
            rabbitChannel.Close();
            rabbitConnection.Close();
            Environment.Exit(0);
        }
    }

    private static void InitRabbitMQ(string enderecoServidor)
    {
        var factory = new ConnectionFactory() { HostName = enderecoServidor };
        rabbitConnection = factory.CreateConnection();
        rabbitChannel = rabbitConnection.CreateModel();
        replyQueueName = rabbitChannel.QueueDeclare().QueueName;
        consumer = new EventingBasicConsumer(rabbitChannel);

        correlationId = Guid.NewGuid().ToString();
        props = rabbitChannel.CreateBasicProperties();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
    }

    private static string Call(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        rabbitChannel.BasicPublish(exchange: "", routingKey: "rpc_queue", basicProperties: props, body: messageBytes);

        var responseReceived = false;
        string response = null;

        consumer.Received += (model, ea) =>
        {
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                response = Encoding.UTF8.GetString(ea.Body.ToArray());
                responseReceived = true;
            }
        };

        rabbitChannel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: consumer);

        while (!responseReceived)
        {
            // Waiting for the response
        }

        return response;
    }

    private static void ClienteMenu(string idCliente)
    {
        while (true)
        {
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
                    response = Call($"REQUEST_TASK|{idCliente}");
                    Console.WriteLine("Resposta do servidor: " + response);
                    break;

                case "2":
                    Console.Write("Por favor, insira a descrição da tarefa concluída: ");
                    string descricaoTarefa = Console.ReadLine();
                    response = Call($"TASK_COMPLETED|{idCliente}|{descricaoTarefa}");
                    Console.WriteLine("Resposta do servidor: " + response);
                    break;

                case "3":
                    Console.Write("Por favor, insira o ID do serviço para subscrever: ");
                    string serviceIdSubscribe = Console.ReadLine();
                    response = Call($"SUBSCRIBE|{idCliente}|{serviceIdSubscribe}");
                    if (response == "SUBSCRIBED")
                    {
                        subscribedServices[serviceIdSubscribe] = $"{idCliente}_{serviceIdSubscribe}";
                        BindNotificationQueue(idCliente, serviceIdSubscribe);
                        Console.WriteLine($"Subscribed to service: {serviceIdSubscribe}");
                    }
                    Console.WriteLine("Resposta do servidor: " + response);
                    break;

                case "4":
                    Console.Write("Por favor, insira o ID do serviço para cancelar a subscricao: ");
                    string serviceIdUnsubscribe = Console.ReadLine();
                    response = Call($"UNSUBSCRIBE|{idCliente}|{serviceIdUnsubscribe}");
                    if (response == "UNSUBSCRIBED")
                    {
                        UnbindNotificationQueue(serviceIdUnsubscribe);
                        subscribedServices.Remove(serviceIdUnsubscribe);
                        Console.WriteLine($"Unsubscribed from service: {serviceIdUnsubscribe}");
                    }
                    Console.WriteLine("Resposta do servidor: " + response);
                    break;

                case "5":
                    return;

                default:
                    Console.WriteLine("Opção inválida. Por favor, tente novamente.");
                    break;
            }
        }
    }

    private static void AdministradorMenu(string idCliente, string adminServiceId)
    {
        while (true)
        {
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
                    Console.Write("Por favor, insira a descrição da nova tarefa com o formato SX_TNUMERO DA TAREFA,DESCRICAO DA TAREFA: ");
                    string descricaoTarefa = Console.ReadLine();
                    response = Call($"ADD_TASK|{adminServiceId}|{descricaoTarefa}");
                    Console.WriteLine("Resposta do servidor: " + response);
                    break;

                case "2":
                    response = Call($"CONSULT_TASKS|{adminServiceId}");
                    Console.WriteLine($"Tarefas no {adminServiceId}:\n{response}");
                    break;

                case "3":
                    Console.Write("Por favor, insira a descrição da tarefa a ser alterada: ");
                    string taskDescription = Console.ReadLine();

                    Console.Write("Insira o novo status da tarefa: ");
                    string newStatus = Console.ReadLine();

                    Console.Write("Insira o campo adicional: ");
                    string additionalField = Console.ReadLine();

                    response = Call($"CHANGE_TASK_STATUS|{adminServiceId}|{taskDescription},{newStatus},{additionalField}");
                    Console.WriteLine("Resposta do servidor: " + response);
                    break;

                case "4":
                    return;

                default:
                    Console.WriteLine("Opção inválida. Por favor, tente novamente.");
                    break;
            }
        }
    }

    private static void InitNotificationListener()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        notificationConnection = factory.CreateConnection();
        notificationChannel = notificationConnection.CreateModel();
        notificationChannel.ExchangeDeclare(exchange: "service_notifications", type: ExchangeType.Topic);
        Console.WriteLine("Notification listener initialized.");
    }

    private static void StartNotificationListener(string clientId)
    {
        foreach (var serviceId in subscribedServices.Keys)
        {
            BindNotificationQueue(clientId, serviceId);
        }

        Console.WriteLine("Client is waiting for notifications...");
    }

    private static void BindNotificationQueue(string clientId, string serviceId)
    {
        var queueName = $"{clientId}_{serviceId}";
        notificationChannel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        notificationChannel.QueueBind(queue: queueName, exchange: "service_notifications", routingKey: $"NOTIFICATION.{serviceId}");

        var consumer = new EventingBasicConsumer(notificationChannel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            if (!message.StartsWith("UNSUBSCRIBE:"))
            {
                Console.WriteLine($"Received notification for {serviceId}: {message}");
            }
            else
            {
                var parts = message.Split(':');
                if (parts.Length == 3 && parts[1] == clientId && parts[2] == serviceId)
                {
                    notificationChannel.QueueUnbind(queue: queueName, exchange: "service_notifications", routingKey: $"NOTIFICATION.{serviceId}");
                    Console.WriteLine($"Unbound from notifications for service: {serviceId}");
                }
            }
        };

        notificationChannel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        Console.WriteLine($"Subscribed to notifications for service: {serviceId}");
    }

    private static void UnbindNotificationQueue(string serviceId)
    {
        if (subscribedServices.ContainsKey(serviceId))
        {
            var queueName = subscribedServices[serviceId];
            notificationChannel.QueueUnbind(queue: queueName, exchange: "service_notifications", routingKey: $"NOTIFICATION.{serviceId}");
            Console.WriteLine($"Unbound from notifications for service: {serviceId}");
        }
    }
}
