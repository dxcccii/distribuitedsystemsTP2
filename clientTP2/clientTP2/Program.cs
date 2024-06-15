using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

class Cliente
{
    static void Main(string[] args)
    {
        Console.WriteLine("Bem-vindo à ServiMoto!"); // Welcome message

        // Prompt for the server's IP address
        Console.Write("Por favor, insira o endereço IP do servidor: ");
        string enderecoServidor = Console.ReadLine();

        try
        {
            while (true) // Keep the client running indefinitely
            {
                // Connect to the server
                using (TcpClient cliente = new TcpClient(enderecoServidor, 1234))
                using (NetworkStream stream = cliente.GetStream())
                using (StreamReader leitor = new StreamReader(stream))
                using (StreamWriter escritor = new StreamWriter(stream) { AutoFlush = true })
                {
                    Console.WriteLine("Conectado ao servidor. Aguardando resposta..."); // Connected to server message

                    // Send CONNECT message to initiate communication
                    escritor.WriteLine("CONNECT");
                    string resposta = leitor.ReadLine();
                    Console.WriteLine("Resposta do servidor: " + resposta); // Server response
                    Thread.Sleep(1000); // Add a delay of 1 second

                    // If the connection was successfully established, request and send the client ID
                    if (resposta == "100 OK")
                    {
                        Console.Write("Por favor, insira o seu ID de cliente: ");
                        string idCliente = Console.ReadLine();

                        // Send the client ID to the server
                        escritor.WriteLine("CLIENT_ID:" + idCliente);

                        // Receive confirmation from the server
                        resposta = leitor.ReadLine();
                        Console.WriteLine("Resposta do servidor: " + resposta); // Server response
                        Thread.Sleep(1000); // Add a delay of 1 second

                        // Determine if the user is an admin based on the client ID
                        bool isAdmin = idCliente.StartsWith("ADMIN_");

                        while (true)
                        {
                            // Present options to the user
                            Console.WriteLine("1. Solicitar tarefa");
                            Console.WriteLine("2. Marcar tarefa como concluída");
                            if (!isAdmin)
                            {
                                Console.WriteLine("3. Subscrever a um serviço");
                                Console.WriteLine("4. Sair");
                            }
                            else
                            {
                                Console.WriteLine("3. Criar tarefa (Administrador)");
                                Console.WriteLine("4. Alocar mota (Administrador)");
                                Console.WriteLine("5. Consultar informação (Administrador)");
                                Console.WriteLine("6. Sair");
                            }

                            Console.Write("Escolha uma opção: ");
                            string opcao = Console.ReadLine();

                            if (opcao == "1")
                            {
                                // Request a new task
                                escritor.WriteLine("REQUEST_TASK CLIENT_ID:" + idCliente);
                                resposta = leitor.ReadLine();
                                Console.WriteLine("Resposta do servidor: " + resposta); // Server response
                                Thread.Sleep(1000); // Add a delay of 1 second

                                if (resposta.StartsWith("TASK_ALLOCATED"))
                                {
                                    string descricaoTarefa = resposta.Substring("TASK_ALLOCATED:".Length).Trim();
                                    Console.WriteLine("Tarefa alocada: " + descricaoTarefa); // Allocated task
                                }
                                else
                                {
                                    Console.WriteLine("Não há tarefas disponíveis no momento."); // No available tasks message
                                }
                            }
                            else if (opcao == "2")
                            {
                                // Mark task as completed
                                Console.Write("Por favor, insira a descrição da tarefa concluída: ");
                                string descricaoTarefa = Console.ReadLine();
                                escritor.WriteLine("TASK_COMPLETED:" + descricaoTarefa);
                                resposta = leitor.ReadLine();
                                Console.WriteLine("Resposta do servidor: " + resposta); // Server response
                                Thread.Sleep(1000); // Add a delay of 1 second
                            }
                            else if (!isAdmin && opcao == "3")
                            {
                                // Non-admin: Subscribe to a service
                                Console.Write("Por favor, insira o ID do serviço para subscrever: ");
                                string serviceId = Console.ReadLine();
                                escritor.WriteLine("SUBSCRIBE:" + serviceId);

                                resposta = leitor.ReadLine();
                                Console.WriteLine("Resposta do servidor: " + resposta); // Server response
                                Thread.Sleep(1000); // Add a delay of 1 second
                            }
                            else if ((isAdmin && opcao == "4") || (!isAdmin && opcao == "4"))
                            {
                                // End communication
                                escritor.WriteLine("SAIR");
                                resposta = leitor.ReadLine();
                                Console.WriteLine("Resposta do servidor: " + resposta); // Server response
                                Thread.Sleep(1000); // Add a delay of 1 second
                                break; // Break out of the loop after receiving server response
                            }
                            else
                            {
                                Console.WriteLine("Opção inválida. Por favor, tente novamente."); // Invalid option message
                            }
                        }
                    }
                }
                Console.WriteLine("Comunicação com o servidor encerrada."); // Communication with server ended message
                break; // Exit the while loop to end the client
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine("Ocorreu um erro de E/S: " + ex.Message); // Input/output error occurred message
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ocorreu um erro: " + ex.Message); // An error occurred message
        }
        finally
        {
            // Ensure the console window closes when execution is complete
            Environment.Exit(0);
        }
    }

