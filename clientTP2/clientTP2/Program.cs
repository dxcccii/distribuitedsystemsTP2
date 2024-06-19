using System;
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
                    response = Call("REQUEST_TASK");
                    Console.WriteLine("Resposta do servidor: " + response);
                    break;

                case "2":
                    Console.Write("Por favor, insira a descrição da tarefa concluída: ");
                    string descricaoTarefa = Console.ReadLine();
                    response = Call($"TASK_COMPLETED:{descricaoTarefa}");
                    Console.WriteLine("Resposta do servidor: " + response);
                    break;

                case "3":
                    Console.Write("Por favor, insira o ID do serviço para subscrever: ");
                    string serviceIdSubscribe = Console.ReadLine();
                    response = Call($"SUBSCRIBE:{serviceIdSubscribe}");
                    Console.WriteLine("Resposta do servidor: " + response);
                    break;

                case "4":
                    Console.Write("Por favor, insira o ID do serviço para cancelar a subscricao: ");
                    string serviceIdUnsubscribe = Console.ReadLine();
                    response = Call($"UNSUBSCRIBE:{serviceIdUnsubscribe}");
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
                    response = Call($"ADD_TASK:{descricaoTarefa}");
                    Console.WriteLine("Resposta do servidor: " + response);
                    break;

                case "2":
                    response = Call("CONSULT_TASKS");
                    Console.WriteLine($"Tarefas no {adminServiceId}:\n");
                    Console.WriteLine(response);
                    break;

                case "3":
                    Console.Write("Por favor, insira a descrição da tarefa a ser alterada: ");
                    string taskDescription = Console.ReadLine();

                    Console.Write("Insira o novo status da tarefa: ");
                    string newStatus = Console.ReadLine();

                    Console.Write("Insira o campo adicional: ");
                    string additionalField = Console.ReadLine();

                    response = Call($"CHANGE_TASK_STATUS:{taskDescription},{newStatus},{additionalField}");
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
}
