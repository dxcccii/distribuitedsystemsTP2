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
                                    Console.Write("Por favor, insira o ID do serviço que você deseja gerenciar (e.g., Servico_X): ");
                                    string adminServiceId = Console.ReadLine();
                                    escritor.WriteLine($"ADMIN_SERVICE_ID:{adminServiceId}");
                                    resposta = leitor.ReadLine();
                                    Console.WriteLine("Resposta do servidor: " + resposta);

                                    if (resposta.StartsWith("SERVICE_CONFIRMED"))
                                    {
                                        AdministradorMenu(escritor, leitor, idCliente, adminServiceId);
                                        break;
                                    }
                                    else if (resposta.StartsWith("SERVICE_NOT_FOUND"))
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

            if (opcao == "1")
            {
                escritor.WriteLine("REQUEST_TASK CLIENT_ID:" + idCliente);
                string resposta = leitor.ReadLine();
                Console.WriteLine("Resposta do servidor: " + resposta);
                Thread.Sleep(1000);
                if (resposta.StartsWith("TASK_ALLOCATED"))
                {
                    string descricaoTarefa = resposta.Substring("TASK_ALLOCATED:".Length).Trim();
                    Console.WriteLine("Tarefa alocada: " + descricaoTarefa);
                }
                else
                {
                    Console.WriteLine("Não há tarefas disponíveis no momento.");
                }
            }
            else if (opcao == "2")
            {
                Console.Write("Por favor, insira a descrição da tarefa concluída: ");
                string descricaoTarefa = Console.ReadLine();
                escritor.WriteLine("TASK_COMPLETED: " + descricaoTarefa);
                string resposta = leitor.ReadLine();
                Console.WriteLine("Resposta do servidor: " + resposta);
                Thread.Sleep(1000);
            }
            else if (opcao == "3")
            {
                Console.Write("Por favor, insira o ID do serviço para assinar: ");
                string serviceId = Console.ReadLine();
                escritor.WriteLine("SUBSCRIBE:" + serviceId);
                string resposta = leitor.ReadLine();
                Console.WriteLine("Resposta do servidor: " + resposta);
                Thread.Sleep(1000);
            }
            else if (opcao == "4")
            {
                Console.Write("Por favor, insira o ID do serviço para cancelar a assinatura: ");
                string serviceId = Console.ReadLine();
                escritor.WriteLine("UNSUBSCRIBE:" + serviceId);
                string resposta = leitor.ReadLine();
                Console.WriteLine("Resposta do servidor: " + resposta);
                Thread.Sleep(1000);
            }
            else if (opcao == "5")
            {
                escritor.WriteLine("SAIR");
                string resposta = leitor.ReadLine();
                Console.WriteLine("Resposta do servidor: " + resposta);
                Thread.Sleep(1000);
                break;
            }
            else
            {
                Console.WriteLine("Opção inválida. Por favor, tente novamente.");
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
                    Console.Write("Por favor, insira a descrição da nova tarefa, no formato SX_NUMERO DA TAREFA,DESCRICAO DA TAREFA: ");
                    string descricaoTarefa = Console.ReadLine();
                    escritor.WriteLine($"ADD_TASK:{descricaoTarefa}");
                    string respostaAddTask = leitor.ReadLine();
                    Console.WriteLine("Resposta do servidor: " + respostaAddTask);
                    Thread.Sleep(1000);
                    break;


                case "2":
                    escritor.WriteLine("CONSULT_TASKS");
                    Console.Write("Lista de tarefas deste servico: \n");
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

                    // Basic client-side validation
                    if (string.IsNullOrWhiteSpace(taskDescription) || string.IsNullOrWhiteSpace(newStatus))
                    {
                        Console.WriteLine("Descrição da tarefa e novo status são obrigatórios.");
                        break;
                    }

                    try
                    {
                        // Send command to server
                        escritor.WriteLine($"CHANGE_TASK_STATUS:{taskDescription},{newStatus},{additionalField}");
                        escritor.Flush(); // Ensure data is sent immediately

                        // Read response from the server
                        string resposta = leitor.ReadLine();

                        // Display server response to the user
                        Console.WriteLine(resposta);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"Erro ao ler resposta do servidor: {ex.Message}");
                    }

                    Thread.Sleep(1000); // Wait briefly before continuing
                    break;

                case "4":
                    escritor.WriteLine("SAIR");
                    string respostaSair = leitor.ReadLine();
                    Console.WriteLine("Resposta do servidor: " + respostaSair);
                    Thread.Sleep(1000);
                    return;

                default:
                    Console.WriteLine("Opção inválida. Por favor, tente novamente.");
                    break;
            }
        }
    }
}
