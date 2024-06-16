using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

class Cliente
{
    static void Main(string[] args)
    {
        Console.WriteLine("Bem-vindo à ServiMoto!");

        // Prompt for the server's IP address
        Console.Write("Por favor, insira o endereço IP do servidor: ");
        string enderecoServidor = Console.ReadLine();

        try
        {
            while (true)
            {
                // Connect to the server
                using (TcpClient cliente = new TcpClient(enderecoServidor, 1234))
                using (NetworkStream stream = cliente.GetStream())
                using (StreamReader leitor = new StreamReader(stream))
                using (StreamWriter escritor = new StreamWriter(stream) { AutoFlush = true })
                {
                    Console.WriteLine("Conectado ao servidor. Aguardando resposta...");

                    // Send CONNECT message to initiate communication
                    escritor.WriteLine("CONNECT");
                    string resposta = leitor.ReadLine();
                    Console.WriteLine("Resposta do servidor: " + resposta);
                    Thread.Sleep(1000);

                    // If the connection was successfully established, request and send the client ID and type
                    if (resposta == "100 OK")
                    {
                        Console.Write("Por favor, insira o seu ID de cliente: ");
                        string idCliente = Console.ReadLine();

                        Console.Write("Por favor, insira o seu tipo de cliente (Admin/Cliente): ");
                        string tipoCliente = Console.ReadLine();

                        // Send the client ID and type to the server
                        escritor.WriteLine($"CLIENT_ID:{idCliente},{tipoCliente}");

                        // Receive confirmation from the server
                        resposta = leitor.ReadLine();
                        Console.WriteLine("Resposta: " + resposta);
                        Thread.Sleep(1000);

                        if (resposta.StartsWith("ID_CONFIRMED"))
                        {
                            string clientType = resposta.Split(':')[2].Trim();
                            if (clientType == "Admin")
                            {
                                AdministradorMenu(escritor, leitor, idCliente);
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

    private static void AdministradorMenu(StreamWriter escritor, StreamReader leitor, string idCliente)
    {
        while (true)
        {
            Console.WriteLine("1. Criar nova tarefa");
            Console.WriteLine("2. Atualizar informação");
            Console.WriteLine("3. Consultar informação");
            Console.WriteLine("4. Assinar notificações");
            Console.WriteLine("5. Cancelar assinatura de notificações");
            Console.WriteLine("6. Sair");
            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            if (opcao == "1")
            {
                Console.Write("Por favor, insira a descrição da nova tarefa: ");
                string descricaoTarefa = Console.ReadLine();
                escritor.WriteLine($"CREATE_TASK:{descricaoTarefa} CLIENT_ID:{idCliente}");
                string resposta = leitor.ReadLine();
                Console.WriteLine("Resposta do servidor: " + resposta);
                Thread.Sleep(1000);
            }
            else if (opcao == "2")
            {
                // Implement update information logic here
            }
            else if (opcao == "3")
            {
                // Implement query information logic here
            }
            else if (opcao == "4")
            {
                Console.Write("Por favor, insira o ID do serviço para assinar: ");
                string serviceId = Console.ReadLine();
                escritor.WriteLine("SUBSCRIBE:" + serviceId);
                string resposta = leitor.ReadLine();
                Console.WriteLine("Resposta do servidor: " + resposta);
                Thread.Sleep(1000);
            }
            else if (opcao == "5")
            {
                Console.Write("Por favor, insira o ID do serviço para cancelar a assinatura: ");
                string serviceId = Console.ReadLine();
                escritor.WriteLine("UNSUBSCRIBE:" + serviceId);
                string resposta = leitor.ReadLine();
                Console.WriteLine("Resposta do servidor: " + resposta);
                Thread.Sleep(1000);
            }
            else if (opcao == "6")
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
}