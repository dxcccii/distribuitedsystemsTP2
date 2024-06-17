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