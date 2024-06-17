using System;// Required for basic system operations and data manipulation
using System.Collections.Generic; // Required for collections like dictionaries and lists
using System.IO; // Required for file input/output operations
using System.Linq; // Required for LINQ queries and operations on collections
using System.Net; // Required for network-related classes such as IPAddress and IPEndPoint
using System.Net.Sockets; // Required for TCP client-server communication
using System.Text; // Required for encoding and manipulating strings
using System.Threading; // Required for multi-threading support
using RabbitMQ.Client; // Required for RabbitMQ client functionalities
using RabbitMQ.Client.Events; // Required for handling RabbitMQ client events
using TaskManager; // Assuming TaskManager namespace is imported for additional functionality

class Servidor
{
    // Dictionary to store service information mapped by client ID
    public static Dictionary<string, (string ServiceId, string Password)> serviceDict = new Dictionary<string, (string ServiceId, string Password)>();

    // Dictionary to store tasks mapped by service ID
    public static Dictionary<string, List<string>> taskDict = new Dictionary<string, List<string>>();

    // Mutex for thread synchronization
    private static Mutex mutex = new Mutex();

    // RabbitMQ connection and channel variables
    private static IConnection rabbitConnection;
    private static IModel rabbitChannel;
    private static string rabbitQueueName = "task_notifications"; // Queue name for RabbitMQ notifications

    // Main method
    static void Main(string[] args)
    {
        // Initialize RabbitMQ connection and channel
        InitRabbitMQ();

        // Print current working directory
        PrintWorkingDirectory();

        // Load service allocations from CSV file
        LoadServiceAllocationsFromCSV();

        // Load task data from CSV files for all services
        LoadDataFromCSVForAllServices();

        TcpListener servidor = null;
        try
        {
            // Start TCP listener on any IP address, port 1234
            servidor = new TcpListener(IPAddress.Any, 1234);
            servidor.Start();
            Console.WriteLine("Servidor iniciado. Aguardando conexões..."); // Server started, waiting for connections...

            // Main server loop
            while (true)
            {
                TcpClient cliente = servidor.AcceptTcpClient(); // Accept incoming client connection
                Console.WriteLine("Cliente conectado!"); // Client connected!

                // Handle client connection in a new thread from the thread pool
                ThreadPool.QueueUserWorkItem(HandleClient, cliente);
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Erro de Socket: " + ex.ToString()); // Socket error
        }
        finally
        {
            servidor?.Stop(); // Stop server listener
        }
    }

    // Method to initialize RabbitMQ connection and channel
    private static void InitRabbitMQ()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" }; // RabbitMQ connection factory
        rabbitConnection = factory.CreateConnection(); // Create RabbitMQ connection
        rabbitChannel = rabbitConnection.CreateModel(); // Create RabbitMQ channel

        // Declare the RabbitMQ queue for task notifications
        rabbitChannel.QueueDeclare(queue: rabbitQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        // Setup consumer to receive messages from RabbitMQ queue
        var consumer = new EventingBasicConsumer(rabbitChannel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received notification: {message}"); // Received notification message
        };
        rabbitChannel.BasicConsume(queue: rabbitQueueName, autoAck: true, consumer: consumer);
    }

    // Method to publish notification message to RabbitMQ queue
    public static void PublishNotification(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        rabbitChannel.BasicPublish(exchange: "", routingKey: rabbitQueueName, basicProperties: null, body: body);
        Console.WriteLine($"Sent notification: {message}"); // Sent notification message
    }

    // Method to load service allocations from CSV file
    private static void LoadServiceAllocationsFromCSV()
    {
        string csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "service_allocations.csv"); // CSV file path

        try
        {
            var lines = File.ReadAllLines(csvFilePath); // Read all lines from CSV file

            // Iterate through each line (skipping header line)
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(','); // Split line into parts by comma

                // Ensure there are at least 3 parts
                if (parts.Length >= 3)
                {
                    string clientId = parts[0].Trim(); // Client ID
                    string serviceId = parts[1].Trim(); // Service ID
                    string password = parts[2].Trim(); // Password

                    // Add client ID and associated service ID/password to serviceDict
                    serviceDict[clientId] = (serviceId, password);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading data from CSV file {csvFilePath}: {ex.Message}"); // Error loading CSV file
        }
    }

    // Method to handle client connections
    private static void HandleClient(object obj)
    {
        TcpClient cliente = (TcpClient)obj; // Cast object to TcpClient
        NetworkStream stream = cliente.GetStream(); // Get network stream from client
        StreamReader leitor = new StreamReader(stream); // StreamReader to read from stream
        StreamWriter escritor = new StreamWriter(stream) { AutoFlush = true }; // StreamWriter to write to stream

        try
        {
            string clientId = null;
            string serviceId = null;

            // Main client communication loop
            while (true)
            {
                string message = leitor.ReadLine(); // Read message from client

                if (message == null)
                    break; // Break loop if message is null (client disconnected)

                // Handle CONNECT command
                if (message.StartsWith("CONNECT"))
                {
                    escritor.WriteLine("100 OK"); // Respond with "100 OK"
                }
                // Handle CLIENT_ID command
                else if (message.StartsWith("CLIENT_ID:"))
                {
                    clientId = message.Substring("CLIENT_ID:".Length).Trim(); // Extract clientId from message
                    escritor.WriteLine($"ID_CONFIRMED:{clientId}"); // Confirm client ID
                }
                // Handle PASSWORD command
                else if (message.StartsWith("PASSWORD:"))
                {
                    string password = message.Substring("PASSWORD:".Length).Trim(); // Extract password from message

                    // Check if client ID exists and password matches
                    if (serviceDict.ContainsKey(clientId) && serviceDict[clientId].Password == password)
                    {
                        escritor.WriteLine("PASSWORD_CONFIRMED"); // Confirm password
                    }
                    else
                    {
                        escritor.WriteLine("403 FORBIDDEN"); // Password incorrect or client ID not found
                    }
                }
                // Handle ADMIN_SERVICE_ID command
                else if (message.StartsWith("ADMIN_SERVICE_ID:"))
                {
                    serviceId = message.Substring("ADMIN_SERVICE_ID:".Length).Trim(); // Extract serviceId from message

                    // Validate service ID format
                    if (!serviceId.StartsWith("Servico_"))
                    {
                        escritor.WriteLine("500 BAD REQUEST"); // Invalid service ID format
                        Console.WriteLine($"Invalid service ID format: {serviceId}");
                        continue; // Skip to next message
                    }

                    // Construct service file path
                    string serviceFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{serviceId}.csv");
                    Console.WriteLine($"Attempting to load service file: {serviceFilePath}");

                    // Check if service file exists
                    if (File.Exists(serviceFilePath))
                    {
                        escritor.WriteLine("SERVICE_CONFIRMED"); // Confirm service ID
                    }
                    else
                    {
                        escritor.WriteLine("SERVICE_NOT_FOUND"); // Service file not found
                        Console.WriteLine($"Service not found: {serviceId}");
                    }
                }
                // Handle commands from administrative clients (clientId starts with "Adm")
                else if (clientId != null && clientId.StartsWith("Adm"))
                {
                    // Ensure serviceId is specified
                    if (serviceId != null)
                    {
                        // Construct service file path
                        string serviceFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{serviceId}.csv");

                        // Process administrative command and send response
                        string response = ProcessAdminCommand(message, clientId, serviceFilePath);
                        escritor.WriteLine(response);
                    }
                    else
                    {
                        escritor.WriteLine("403 SERVICE_ID_NOT_SPECIFIED"); // Service ID not specified
                    }
                }
                // Handle commands from regular clients
                else if (clientId != null)
                {
                    // Process client command and send response
                    string response = ProcessClientCommand(message, clientId);
                    escritor.WriteLine(response);
                }
                else
                {
                    escritor.WriteLine("403 FORBIDDEN"); // Invalid or unauthorized request
                }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine("Erro de E/S: " + ex.ToString()); // IO error
        }
        finally
        {
            cliente.Close(); // Close client connection
        }
    }

    // Method to process administrative commands
    private static string ProcessAdminCommand(string message, string clientId, string serviceFilePath)
    {
        // Handle ADD_TASK command
        if (message.StartsWith("ADD_TASK:"))
        {
            string taskDescription = message.Substring("ADD_TASK:".Length).Trim(); // Extract task description
            return AddTask(serviceFilePath, taskDescription); // Add task and return response
        }
        // Handle CONSULT_TASKS command
        else if (message.StartsWith("CONSULT_TASKS"))
        {
            return ConsultTasks(serviceFilePath); // Consult tasks and return response
        }
        // Handle CHANGE_TASK_STATUS command
        else if (message.StartsWith("CHANGE_TASK_STATUS:"))
        {
            string taskStatusInfo = message.Substring("CHANGE_TASK_STATUS:".Length).Trim(); // Extract task status info
            string[] statusParts = taskStatusInfo.Split(','); // Split status info into parts

            // Validate number of parts
            if (statusParts.Length < 3)
            {
                return "500 BAD REQUEST"; // Insufficient parts in status info
            }

            string taskDescription = statusParts[0].Trim(); // Extract task description
            string newStatus = statusParts[1].Trim(); // Extract new status
            string additionalField = statusParts[2].Trim(); // Extract additional field

            // Process and change task status
            return ChangeTaskStatus(serviceFilePath, taskDescription, newStatus, additionalField);
        }
        else
        {
            return "500 BAD REQUEST"; // Invalid administrative command
        }
    }

    // Method to process commands from regular clients
    private static string ProcessClientCommand(string message, string clientId)
    {
        // Handle REQUEST_TASK command
        if (message.StartsWith("REQUEST_TASK"))
        {
            return AllocateTask(clientId); // Allocate task and return response
        }
        // Handle TASK_COMPLETED command
        else if (message.StartsWith("TASK_COMPLETED:"))
        {
            string taskDescription = message.Substring("TASK_COMPLETED:".Length).Trim(); // Extract task description
            return MarkTaskAsCompleted(clientId, taskDescription); // Mark task as completed and return response
        }
        // Handle SUBSCRIBE command
        else if (message.StartsWith("SUBSCRIBE:"))
        {
            string serviceId = message.Substring("SUBSCRIBE:".Length).Trim(); // Extract service ID
            SubscribeToService(clientId, serviceId); // Subscribe client to service
            return "SUBSCRIBED"; // Return subscription confirmation
        }
        // Handle UNSUBSCRIBE command
        else if (message.StartsWith("UNSUBSCRIBE:"))
        {
            string serviceId = message.Substring("UNSUBSCRIBE:".Length).Trim(); // Extract service ID
            UnsubscribeFromService(clientId, serviceId); // Unsubscribe client from service
            return "UNSUBSCRIBED"; // Return unsubscription confirmation
        }
        else
        {
            return "500 BAD REQUEST"; // Invalid client command
        }
    }

    // Method to add a new task to a service file
    private static string AddTask(string serviceFilePath, string taskDescription)
    {
        try
        {
            // Format new task entry
            string newTask = $"{taskDescription},nao alocada,";

            // Append new task to service file
            File.AppendAllLines(serviceFilePath, new string[] { newTask });

            return "201 CREATED"; // Task creation successful
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding task: {ex.Message}"); // Error adding task
            return "500 INTERNAL SERVER ERROR"; // Internal server error
        }
    }

    // Method to consult tasks from a service file
    private static string ConsultTasks(string serviceFilePath)
    {
        try
        {
            // Read all tasks from service file
            string[] tasks = File.ReadAllLines(serviceFilePath);

            // Build response with all tasks
            StringBuilder response = new StringBuilder();
            foreach (string task in tasks)
            {
                response.AppendLine(task); // Append each task to response
            }
            response.AppendLine("<END_OF_RESPONSE>"); // Mark end of response
            Console.WriteLine("ConsultTasks response: " + response.ToString()); // Log response

            return response.ToString(); // Return complete response
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error consulting tasks: {ex.Message}"); // Error consulting tasks
            return "500 Internal Server Error"; // Internal server error
        }
    }

    // Method to change the status of a task in a service file
    private static string ChangeTaskStatus(string serviceFilePath, string taskDescription, string newStatus, string additionalField)
    {
        try
        {
            Console.WriteLine($"Received request - Task: '{taskDescription}', New Status: '{newStatus}', Additional Field: '{additionalField}'"); // Log task status change request

            if (string.IsNullOrEmpty(taskDescription))
            {
                Console.WriteLine("Task description is empty."); // Log empty task description error
                return "500 BAD REQUEST - Task description cannot be empty"; // Task description cannot be empty
            }

            // Read all lines from service file
            string[] lines = File.ReadAllLines(serviceFilePath);
            bool taskFound = false;

            // Iterate through each line in service file
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] parts = line.Split(','); // Split line into parts by comma

                Console.WriteLine($"Processing line: {line}"); // Log line processing

                // Check if line contains correct number of parts and matches task description
                if (parts.Length >= 3 && parts[1].Trim() == taskDescription)
                {
                    Console.WriteLine("Task found."); // Log task found

                    // Validate new status
                    if (!IsValidStatus(newStatus))
                    {
                        Console.WriteLine("Invalid new status."); // Log invalid status error
                        return "500 BAD REQUEST - Invalid newStatus"; // Invalid new status
                    }

                    // Handle special case for 'nao alocada' status
                    if (newStatus.ToLower() == "nao alocada")
                    {
                        additionalField = ""; // Clear additional field
                    }
                    // Validate additional field format
                    else if (!string.IsNullOrEmpty(additionalField) && !additionalField.StartsWith("Cl_"))
                    {
                        Console.WriteLine("Additional field does not start with 'Cl_'."); // Log invalid additional field error
                        return "500 BAD REQUEST - Additional field must start with 'Cl_'"; // Invalid additional field
                    }

                    // Update parts with new status and additional field
                    parts[2] = newStatus;
                    if (parts.Length == 3)
                    {
                        line = string.Join(",", parts[0], parts[1], parts[2], additionalField); // Join parts into updated line
                    }
                    else
                    {
                        parts[3] = additionalField;
                        line = string.Join(",", parts); // Join parts into updated line
                    }

                    lines[i] = line; // Update line in lines array
                    taskFound = true; // Set task found flag
                    break; // Exit loop
                }
            }

            // Check if task was found
            if (taskFound)
            {
                File.WriteAllLines(serviceFilePath, lines); // Write all lines to service file
                Console.WriteLine("Task status updated successfully."); // Log successful task status update
                return "200 OK"; // Task status updated successfully
            }
            else
            {
                Console.WriteLine("Task not found."); // Log task not found
                return "404 NOT FOUND - Task not found"; // Task not found
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error accessing file {serviceFilePath}: {ex.Message}"); // Log file access error
            return "500 INTERNAL SERVER ERROR - IOException"; // Internal server error
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error changing task status: {ex.Message}"); // Log error changing task status
            return "500 INTERNAL SERVER ERROR"; // Internal server error
        }
    }

    // Method to validate task status
    private static bool IsValidStatus(string status)
    {
        // Define valid status values
        string[] validStatuses = { "Nao alocada", "Concluido", "Em curso" };
        return validStatuses.Contains(status); // Check if status is valid
    }

    // Method to print current working directory
    private static void PrintWorkingDirectory()
    {
        string currentDirectory = Directory.GetCurrentDirectory(); // Get current working directory
        Console.WriteLine("Current working directory: " + currentDirectory); // Log current working directory
    }

    // Method to load data from CSV files for all services
    private static void LoadDataFromCSVForAllServices()
    {
        string servicesFilePath = Directory.GetCurrentDirectory(); // Get current directory path
        try
        {
            // Iterate through all CSV files in current directory
            foreach (var serviceFile in Directory.GetFiles(servicesFilePath, "*.csv"))
            {
                var serviceLines = File.ReadAllLines(serviceFile); // Read all lines from CSV file
                string serviceId = Path.GetFileNameWithoutExtension(serviceFile); // Extract service ID

                // Initialize task list for service ID if not already initialized
                if (!taskDict.ContainsKey(serviceId))
                {
                    taskDict[serviceId] = new List<string>(); // Create new task list
                }

                // Iterate through each line in service file (skip header line)
                for (int i = 1; i < serviceLines.Length; i++)
                {
                    var line = serviceLines[i]; // Get line
                    var parts = line.Split(','); // Split line into parts by comma

                    // Ensure there are exactly 4 parts
                    if (parts.Length == 3)
                    {
                        line = $"{parts[0].Trim()},{parts[1].Trim()},{parts[2].Trim()},";
                    }
                    else if (parts.Length < 4)
                    {
                        line = $"{line.Trim()},";
                    }

                    // Add formatted line to task list for service ID
                    taskDict[serviceId].Add(line.Trim());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading data from CSV file {servicesFilePath}: {ex.Message}"); // Log error loading CSV file
        }
    }

    // Method to allocate task to client
    public static string AllocateTask(string clientId)
    {
        mutex.WaitOne(); // Wait for mutex lock
        try
        {
            // Check if client ID exists in service dictionary
            if (!serviceDict.ContainsKey(clientId))
            {
                return "ERROR: Service not";
            }

            string serviceId = serviceDict[clientId].ServiceId; // Get service ID for client

            // Check if service ID exists in task dictionary
            if (taskDict.ContainsKey(serviceId))
            {
                // Find first unallocated task in service
                var unallocatedTask = taskDict[serviceId].FirstOrDefault(task => task.Split(',')[2].Trim().ToLower() == "nao alocada");

                // Check if unallocated task found
                if (unallocatedTask != null)
                {
                    var taskParts = unallocatedTask.Split(','); // Split unallocated task into parts

                    // Log task parts for debugging
                    Console.WriteLine($"Task parts: {string.Join(" | ", taskParts)}");

                    // Ensure there are exactly 4 parts in taskParts
                    if (taskParts.Length != 4)
                    {
                        Console.WriteLine($"Error: Task format is incorrect for task: {unallocatedTask}");
                        return "500 INTERNAL SERVER ERROR"; // Return internal server error if task format incorrect
                    }

                    // Mark the task as allocated to the client
                    taskParts[2] = "Em curso"; // Change status to "Em curso"
                    taskParts[3] = clientId; // Assign client ID

                    string updatedTask = string.Join(",", taskParts); // Join task parts into updated task
                    int taskIndex = taskDict[serviceId].IndexOf(unallocatedTask); // Get index of unallocated task
                    taskDict[serviceId][taskIndex] = updatedTask; // Update task in task dictionary

                    // Update the CSV file with updated task list
                    string serviceFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{serviceId}.csv");
                    File.WriteAllLines(serviceFilePath, taskDict[serviceId]);

                    string message = $"TASK_ALLOCATED:{taskParts[1]}"; // Prepare notification message
                    PublishNotification(message); // Publish task allocation notification
                    return message; // Return task allocation confirmation message
                }
                else
                {
                    return "NO_TASK_AVAILABLE"; // Return message if no unallocated tasks available
                }
            }
            else
            {
                return "NO_TASK_AVAILABLE"; // Return message if service ID not found in task dictionary
            }
        }
        finally
        {
            mutex.ReleaseMutex(); // Release mutex lock
        }
    }

    // Method to mark task as completed by client
    public static string MarkTaskAsCompleted(string clientId, string taskDescription)
    {
        mutex.WaitOne(); // Wait for mutex lock
        try
        {
            // Check if client ID exists in service dictionary
            if (!serviceDict.ContainsKey(clientId))
            {
                return "ERROR: Service not found for client"; // Return error message if client ID not found
            }

            string serviceId = serviceDict[clientId].ServiceId; // Get service ID for client

            // Check if service ID exists in task dictionary
            if (taskDict.ContainsKey(serviceId))
            {
                // Find index of task with matching task description
                var taskIndex = taskDict[serviceId].FindIndex(task => task.Split(',')[1].Trim() == taskDescription);

                // Check if task index found
                if (taskIndex != -1)
                {
                    var taskParts = taskDict[serviceId][taskIndex].Split(','); // Split task into parts

                    // Check if task format is correct
                    if (taskParts.Length != 4)
                    {
                        return $"ERROR: Incorrect task format for task: {taskDict[serviceId][taskIndex]}"; // Return error message if task format incorrect
                    }

                    taskParts[2] = "Concluido"; // Change status to "Concluido"
                    taskParts[3] = clientId; // Update client ID

                    string updatedTask = string.Join(",", taskParts); // Join task parts into updated task
                    taskDict[serviceId][taskIndex] = updatedTask; // Update task in task dictionary

                    // Update the CSV file with updated task list
                    string serviceFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{serviceId}.csv");
                    File.WriteAllLines(serviceFilePath, taskDict[serviceId]);

                    string notificationMessage = $"TASK_COMPLETED:{clientId}:{taskDescription}"; // Prepare notification message
                    PublishNotification(notificationMessage); // Publish task completion notification
                    return $"TASK_MARKED_AS_COMPLETED:{taskDescription}"; // Return task completion confirmation message
                }
                else
                {
                    return $"ERROR_TASK_NOT_FOUND:{taskDescription}"; // Return error message if task not found
                }
            }
            else
            {
                return $"ERROR_SERVICE_NOT_FOUND:{serviceId}"; // Return error message if service ID not found
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking task as completed: {ex.Message}"); // Log error marking task as completed
            return "500 INTERNAL SERVER ERROR"; // Return internal server error
        }
        finally
        {
            mutex.ReleaseMutex(); // Release mutex lock
        }
    }

    // Method to publish subscription to a service
    private static void SubscribeToService(string clientId, string serviceId)
    {
        PublishNotification($"SUBSCRIBED:{clientId}:{serviceId}"); // Publish subscription notification
    }

    // Method to publish unsubscription from a service
    private static void UnsubscribeFromService(string clientId, string serviceId)
    {
        PublishNotification($"UNSUBSCRIBED:{clientId}:{serviceId}"); // Publish unsubscription notification
    }
}
