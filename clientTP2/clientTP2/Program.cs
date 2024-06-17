using System; // Required for basic system operations and data manipulation
using System.IO; // Required for file input/output operations
using System.Net.Sockets; // Required for TCP client-server communication
using System.Threading; // Required for thread management and synchronization

class Cliente
{
    static void Main(string[] args)
    {
        Console.WriteLine("Bem-vindo à ServiMoto!"); // Welcome message

        Console.Write("Por favor, insira o endereço IP do servidor: ");
        string enderecoServidor = Console.ReadLine(); // Prompt user for server IP address

        try
        {
            while (true)
            {
                // Establish TCP connection with the server
                using (TcpClient cliente = new TcpClient(enderecoServidor, 1234))
                using (NetworkStream stream = cliente.GetStream())
                using (StreamReader leitor = new StreamReader(stream))
                using (StreamWriter escritor = new StreamWriter(stream) { AutoFlush = true })
                {
                    Console.WriteLine("Conectado ao servidor. Aguardando resposta..."); // Connected to server message

                    // Send initial connection request to server
                    escritor.WriteLine("CONNECT");
                    string resposta = leitor.ReadLine(); // Read server's response
                    Console.WriteLine("Resposta do servidor: " + resposta); // Display server's response
                    Thread.Sleep(1000); // Pause for readability

                    if (resposta == "100 OK")
                    {
                        Console.Write("Por favor, insira o seu ID de cliente: ");
                        string idCliente = Console.ReadLine(); // Prompt user for client ID

                        // Send client ID to server for authentication
                        escritor.WriteLine($"CLIENT_ID:{idCliente}");
                        resposta = leitor.ReadLine(); // Read server's response
                        Console.WriteLine("Resposta: " + resposta); // Display server's response
                        Thread.Sleep(1000); // Pause for readability

                        if (resposta.StartsWith("ID_CONFIRMED"))
                        {
                            Console.Write("Por favor, insira a sua senha: ");
                            string senha = Console.ReadLine(); // Prompt user for password

                            // Send password to server for authentication
                            escritor.WriteLine($"PASSWORD:{senha}");
                            resposta = leitor.ReadLine(); // Read server's response
                            Console.WriteLine("Resposta: " + resposta); // Display server's response
                            Thread.Sleep(1000); // Pause for readability

                            if (resposta == "PASSWORD_CONFIRMED")
                            {
                                if (idCliente.StartsWith("Adm"))
                                {
                                    while (true)
                                    {
                                        Console.Write("Por favor, insira o ID do serviço que você deseja gerenciar (e.g., Servico_X): ");
                                        string adminServiceId = Console.ReadLine(); // Prompt admin for service ID

                                        // Send admin service ID to server for validation
                                        escritor.WriteLine($"ADMIN_SERVICE_ID:{adminServiceId}");
                                        resposta = leitor.ReadLine(); // Read server's response
                                        Console.WriteLine("Resposta do servidor: " + resposta); // Display server's response

                                        if (resposta == "SERVICE_CONFIRMED")
                                        {
                                            // Enter administrator menu for service management
                                            AdministradorMenu(escritor, leitor, idCliente, adminServiceId);
                                            break;
                                        }
                                        else if (resposta == "SERVICE_NOT_FOUND")
                                        {
                                            Console.WriteLine(resposta); // Service not found message
                                        }
                                        else
                                        {
                                            Console.WriteLine("Resposta do servidor desconhecida: " + resposta); // Unknown server response
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    // Enter client menu for task management
                                    ClienteMenu(escritor, leitor, idCliente);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Autenticação falhou."); // Authentication failed message
                            }
                        }
                    }
                }
                Console.WriteLine("Comunicação com o servidor encerrada."); // Server communication ended message
                break;
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine("Ocorreu um erro de E/S: " + ex.ToString()); // I/O error message
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ocorreu um erro: " + ex.Message); // General error message
        }
        finally
        {
            Environment.Exit(0); // Exit the application
        }
    }

    // Method for handling client menu options
    private static void ClienteMenu(StreamWriter escritor, StreamReader leitor, string idCliente)
    {
        while (true)
        {
            // Display menu options for the client
            Console.WriteLine("1. Solicitar tarefa");
            Console.WriteLine("2. Marcar tarefa como concluída");
            Console.WriteLine("3. Subscrever a um servico");
            Console.WriteLine("4. Cancelar a subscricao as notificacoes de um servico");
            Console.WriteLine("5. Sair");
            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine(); // Prompt user for menu option

            switch (opcao)
            {
                case "1":
                    // Request a new task from the server
                    escritor.WriteLine("REQUEST_TASK");
                    string respostaRequestTask = leitor.ReadLine(); // Read server's response
                    Console.WriteLine("Resposta do servidor: " + respostaRequestTask); // Display server's response
                    Thread.Sleep(1000); // Pause for readability
                    break;

                case "2":
                    // Mark a task as completed and send description to server
                    Console.Write("Por favor, insira a descrição da tarefa concluída: ");
                    string descricaoTarefa = Console.ReadLine(); // Prompt user for task description
                    escritor.WriteLine("TASK_COMPLETED:" + descricaoTarefa);
                    string respostaTaskCompleted = leitor.ReadLine(); // Read server's response
                    Console.WriteLine("Resposta do servidor: " + respostaTaskCompleted); // Display server's response
                    Thread.Sleep(1000); // Pause for readability
                    break;

                case "3":
                    // Subscribe to a service by sending service ID to server
                    Console.Write("Por favor, insira o ID do serviço para subscrever: ");
                    string serviceIdSubscribe = Console.ReadLine(); // Prompt user for service ID
                    escritor.WriteLine("SUBSCRIBE:" + serviceIdSubscribe);
                    string respostaSubscribe = leitor.ReadLine(); // Read server's response
                    Console.WriteLine("Resposta do servidor: " + respostaSubscribe); // Display server's response
                    Thread.Sleep(1000); // Pause for readability
                    break;

                case "4":
                    // Unsubscribe from a service by sending service ID to server
                    Console.Write("Por favor, insira o ID do serviço para cancelar a subscricao: ");
                    string serviceIdUnsubscribe = Console.ReadLine(); // Prompt user for service ID
                    escritor.WriteLine("UNSUBSCRIBE:" + serviceIdUnsubscribe);
                    string respostaUnsubscribe = leitor.ReadLine(); // Read server's response
                    Console.WriteLine("Resposta do servidor: " + respostaUnsubscribe); // Display server's response
                    Thread.Sleep(1000); // Pause for readability
                    break;

                case "5":
                    return; // Exit client menu

                default:
                    Console.WriteLine("Opção inválida. Por favor, tente novamente."); // Invalid option message
                    break;
            }
        }
    }

    // Method for handling administrator menu options
    private static void AdministradorMenu(StreamWriter escritor, StreamReader leitor, string idCliente, string adminServiceId)
    {
        while (true)
        {
            // Display menu options for the administrator
            Console.WriteLine("1. Criar nova tarefa");
            Console.WriteLine("2. Consultar tarefas");
            Console.WriteLine("3. Alterar status da tarefa");
            Console.WriteLine("4. Sair");
            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine(); // Prompt admin for menu option

            switch (opcao)
            {
                case "1":
                    // Create a new task by sending task description to server
                    Console.Write("Por favor, insira a descrição da nova tarefa com o formato SX_TNUMERO DA TAREFA,DESCRICAO DA TAREFA: ");
                    string descricaoTarefa = Console.ReadLine(); // Prompt admin for task description
                    escritor.WriteLine($"ADD_TASK:{descricaoTarefa}");
                    string respostaAddTask = leitor.ReadLine(); // Read server's response
                    Console.WriteLine("Resposta do servidor: " + respostaAddTask); // Display server's response
                    Thread.Sleep(1000); // Pause for readability
                    break;

                case "2":
                    // Consult tasks for the specified service and display to admin
                    escritor.WriteLine("CONSULT_TASKS");
                    Console.Write($"Tarefas no {adminServiceId}:\n");
                    string respostaConsultTasks = leitor.ReadLine(); // Read server's response
                    while ((respostaConsultTasks = leitor.ReadLine()) != null)
                    {
                        if (respostaConsultTasks == "<END_OF_RESPONSE>")
                        {
                            break; // End of task list response
                        }
                        Console.WriteLine(respostaConsultTasks); // Display each task
                    }
                    Thread.Sleep(1000); // Pause for readability
                    break;

                case "3":
                    // Change status of a task by sending task details to server
                    Console.Write("Por favor, insira a descrição da tarefa a ser alterada: ");
                    string taskDescription = Console.ReadLine(); // Prompt admin for task description

                    Console.Write("Insira o novo status da tarefa: ");
                    string newStatus = Console.ReadLine(); // Prompt admin for new task status

                    Console.Write("Insira o campo adicional: ");
                    string additionalField = Console.ReadLine(); // Prompt admin for additional field

                    escritor.WriteLine($"CHANGE_TASK_STATUS:{taskDescription},{newStatus},{additionalField}"); // assemble formatted task 

                    string respostaChangeStatus = leitor.ReadLine(); // Read server's response
                    Console.WriteLine("Resposta do servidor: " + respostaChangeStatus); // Display server's response
                    Thread.Sleep(1000); // Pause for readability
                    break;

                case "4":
                    return; // Exit administrator menu

                default:
                    Console.WriteLine("Opção inválida. Por favor, tente novamente."); // Invalid option message
                    break;
            }
        }
    }
}