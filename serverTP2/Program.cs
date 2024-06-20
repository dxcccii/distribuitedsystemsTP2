using System; // Import the System namespace.
using System.Collections.Generic; // Import the System.Collections.Generic namespace.
using System.IO; // Import the System.IO namespace.
using System.Linq; // Import the System.Linq namespace.
using System.Text; // Import the System.Text namespace.
using System.Threading; // Import the System.Threading namespace.
using RabbitMQ.Client; // Import the RabbitMQ.Client namespace.
using RabbitMQ.Client.Events; // Import the RabbitMQ.Client.Events namespace.

class Servidor // Define a class named Servidor.
{
    public static Dictionary<string, (string ServiceId, string Password)> serviceDict = new Dictionary<string, (string ServiceId, string Password)>(); // Dictionary to store service ID and password for each client.
    public static Dictionary<string, List<string>> taskDict = new Dictionary<string, List<string>>(); // Dictionary to store tasks for each service.
    private static Mutex mutex = new Mutex(); // Mutex for thread safety.

    private static IConnection rabbitConnection; // Declare a variable for RabbitMQ connection.
    private static IModel rabbitChannel; // Declare a variable for RabbitMQ channel.
    private static string rpcQueueName = "rpc_queue"; // Define the RPC queue name.

    static void Main(string[] args) // Main method, entry point of the application.
    {
        InitRabbitMQ(); // Initialize RabbitMQ connection and channel.
        PrintWorkingDirectory(); // Print the current working directory.
        LoadServiceAllocationsFromCSV(); // Load service allocations from a CSV file.
        LoadDataFromCSVForAllServices(); // Load tasks data from CSV files for all services.

        var consumer = new EventingBasicConsumer(rabbitChannel); // Create a new consumer for RabbitMQ.
        consumer.Received += (model, ea) => // Define the event handler for received messages.
        {
            var body = ea.Body.ToArray(); // Get the body of the message.
            var props = ea.BasicProperties; // Get the properties of the message.
            var replyProps = rabbitChannel.CreateBasicProperties(); // Create properties for the reply message.
            replyProps.CorrelationId = props.CorrelationId; // Set the correlation ID for the reply.

            string response = null; // Initialize response to null.
            try
            {
                var message = Encoding.UTF8.GetString(body); // Convert message body to string.
                Console.WriteLine($"Received message: {message}"); // Print the received message.
                response = HandleRequest(message); // Handle the request and get the response.
            }
            catch (Exception ex) // Catch any exceptions.
            {
                Console.WriteLine($"Error handling request: {ex.Message}"); // Print the error message.
                response = "500 INTERNAL SERVER ERROR"; // Set response to internal server error.
            }
            finally
            {
                var responseBytes = Encoding.UTF8.GetBytes(response); // Convert response to bytes.
                rabbitChannel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes); // Publish the response.
                rabbitChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false); // Acknowledge the message.
            }
        };

        rabbitChannel.BasicConsume(queue: rpcQueueName, autoAck: false, consumer: consumer); // Start consuming messages from the RPC queue.
        Console.WriteLine("RPC Server is running. Waiting for requests..."); // Print server running message.
        Console.ReadLine(); // Wait for user input to keep the server running.
    }

    private static void InitRabbitMQ() // Method to initialize RabbitMQ.
    {
        var factory = new ConnectionFactory() { HostName = "localhost" }; // Create a connection factory with the hostname.
        rabbitConnection = factory.CreateConnection(); // Create a connection to RabbitMQ.
        rabbitChannel = rabbitConnection.CreateModel(); // Create a channel.
        rabbitChannel.QueueDeclare(queue: rpcQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null); // Declare the RPC queue.
        rabbitChannel.ExchangeDeclare(exchange: "service_notifications", type: ExchangeType.Topic); // Declare the exchange for notifications.
        Console.WriteLine("RabbitMQ Initialized."); // Print RabbitMQ initialized message.
    }

    private static string HandleRequest(string message) // Method to handle incoming requests.
    {
        string response = null; // Initialize response to null.
        if (message.StartsWith("CONNECT")) // Check if message is CONNECT.
        {
            response = "100 OK"; // Set response to OK.
        }
        else if (message.StartsWith("CLIENT_ID:")) // Check if message contains client ID.
        {
            string clientId = message.Substring("CLIENT_ID:".Length).Trim(); // Extract client ID from the message.
            response = $"ID_CONFIRMED:{clientId}"; // Set response to ID confirmed.
        }
        else if (message.StartsWith("PASSWORD:")) // Check if message contains password.
        {
            string[] parts = message.Substring("PASSWORD:".Length).Trim().Split(','); // Split the message to get client ID and password.
            string clientId = parts[0].Trim(); // Extract client ID.
            string password = parts[1].Trim(); // Extract password.

            if (serviceDict.ContainsKey(clientId) && serviceDict[clientId].Password == password) // Check if client ID and password match.
            {
                response = "PASSWORD_CONFIRMED"; // Set response to password confirmed.
            }
            else
            {
                response = "403 FORBIDDEN"; // Set response to forbidden.
            }
        }
        else if (message.StartsWith("ADMIN_SERVICE_ID:")) // Check if message contains admin service ID.
        {
            string serviceId = message.Substring("ADMIN_SERVICE_ID:".Length).Trim(); // Extract service ID.

            if (!serviceId.StartsWith("Servico_")) // Check if service ID is valid.
            {
                response = "500 BAD REQUEST"; // Set response to bad request.
            }
            else
            {
                string serviceFilePath = Path.Combine(serviceId + ".csv"); // Create file path for the service.
                response = File.Exists(serviceFilePath) ? "SERVICE_CONFIRMED" : "SERVICE_NOT_FOUND"; // Check if file exists and set response accordingly.
            }
        }
        else
        {
            string[] parts = message.Split('|'); // Split the message to get command and data.
            string command = parts[0]; // Extract command.
            string clientId = parts[1]; // Extract client ID.
            string data = parts.Length > 2 ? parts[2] : null; // Extract data if present.

            switch (command) // Switch based on the command.
            {
                case "ADD_TASK":
                    response = AddTask(clientId, data); // Call AddTask method and get the response.
                    break;
                case "CONSULT_TASKS":
                    response = ConsultTasks(clientId); // Call ConsultTasks method and get the response.
                    break;
                case "CHANGE_TASK_STATUS":
                    var statusParts = data.Split(','); // Split data to get task details.
                    response = ChangeTaskStatus(clientId, statusParts[0], statusParts[1], statusParts[2]); // Call ChangeTaskStatus method and get the response.
                    break;
                case "REQUEST_TASK":
                    response = AllocateTask(clientId); // Call AllocateTask method and get the response.
                    break;
                case "TASK_COMPLETED":
                    response = MarkTaskAsCompleted(clientId, data); // Call MarkTaskAsCompleted method and get the response.
                    break;
                case "SUBSCRIBE":
                    SubscribeToService(clientId, data); // Call SubscribeToService method.
                    response = "SUBSCRIBED"; // Set response to subscribed.
                    break;
                case "UNSUBSCRIBE":
                    UnsubscribeFromService(clientId, data); // Call UnsubscribeFromService method.
                    response = "UNSUBSCRIBED"; // Set response to unsubscribed.
                    break;
                default:
                    response = "500 BAD REQUEST"; // Set response to bad request for unknown commands.
                    break;
            }
        }
        return response; // Return the response.
    }

    private static void LoadServiceAllocationsFromCSV() // Method to load service allocations from a CSV file.
    {
        string csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "service_allocations.csv"); // Get the file path.

        try
        {
            var lines = File.ReadAllLines(csvFilePath); // Read all lines from the CSV file.
            foreach (var line in lines.Skip(1)) // Iterate through the lines, skipping the header.
            {
                var parts = line.Split(','); // Split each line to get parts.

                if (parts.Length >= 3) // Check if there are enough parts.
                {
                    string clientId = parts[0].Trim(); // Extract client ID.
                    string serviceId = parts[1].Trim(); // Extract service ID.
                    string password = parts[2].Trim(); // Extract password.

                    serviceDict[clientId] = (serviceId, password); // Add client ID, service ID, and password to the dictionary.
                }
            }
        }
        catch (Exception ex) // Catch any exceptions.
        {
            Console.WriteLine($"Error loading data from CSV file {csvFilePath}: {ex.Message}"); // Print error message.
        }
    }

    private static string AddTask(string serviceId, string taskDescription) // Method to add a task.
    {
        string serviceFilePath = serviceId + ".csv"; // Create file path for the service.
        try
        {
            string newTask = $"{taskDescription},nao alocada,"; // Create a new task string.
            File.AppendAllLines(serviceFilePath, new string[] { newTask }); // Append the new task to the file.
            PublishNotification($"\nTASK_ADDED:{serviceId}: {taskDescription}", isAdminChange: true); // Publish a notification for the new task.
            return "201 CREATED"; // Return created response.
        }
        catch (Exception ex) // Catch any exceptions.
        {
            Console.WriteLine($"Error adding task: {ex.Message}"); // Print error message.
            return "500 INTERNAL SERVER ERROR"; // Return internal server error response.
        }
    }

    private static string ConsultTasks(string serviceFilePath) // Method to consult tasks.
    {
        serviceFilePath = serviceFilePath + ".csv"; // Create file path for the service.
        try
        {
            string[] tasks = File.ReadAllLines(serviceFilePath); // Read all tasks from the file.
            StringBuilder response = new StringBuilder(); // Create a StringBuilder for the response.
            foreach (string task in tasks) // Iterate through the tasks.
            {
                response.AppendLine(task); // Append each task to the response.
            }
            response.AppendLine("END"); // Append end to the response.
            return response.ToString(); // Return the response as a string.
        }
        catch (Exception ex) // Catch any exceptions.
        {
            Console.WriteLine($"Error consulting tasks: {ex.Message}"); // Print error message.
            return "500 Internal Server Error"; // Return internal server error response.
        }
    }

    private static string ChangeTaskStatus(string serviceId, string taskDescription, string newStatus, string additionalField) // Method to change the status of a task.
    {
        string serviceFilePath = serviceId + ".csv"; // Create file path for the service.
        try
        {
            string[] lines = File.ReadAllLines(serviceFilePath); // Read all lines from the file.
            bool taskFound = false; // Initialize task found flag.

            for (int i = 0; i < lines.Length; i++) // Iterate through the lines.
            {
                string line = lines[i]; // Get the line.
                string[] parts = line.Split(','); // Split the line to get parts.

                if (parts.Length >= 3 && parts[1].Trim() == taskDescription) // Check if the line matches the task description.
                {
                    if (!IsValidStatus(newStatus)) // Check if the new status is valid.
                    {
                        return "500 BAD REQUEST - Invalid newStatus"; // Return bad request response.
                    }

                    if (newStatus.ToLower() == "nao alocada") // Check if the new status is nao alocada.
                    {
                        additionalField = ""; // Set additional field to empty.
                    }
                    else if (!string.IsNullOrEmpty(additionalField) && !additionalField.StartsWith("Cl_")) // Check if the additional field is valid.
                    {
                        return "500 BAD REQUEST - Additional field must start with 'Cl_'"; // Return bad request response.
                    }

                    parts[2] = newStatus; // Set the new status.
                    if (parts.Length == 3) // Check if there are only 3 parts.
                    {
                        line = string.Join(",", parts[0], parts[1], parts[2], additionalField); // Create a new line with the additional field.
                    }
                    else
                    {
                        parts[3] = additionalField; // Set the additional field.
                        line = string.Join(",", parts); // Create a new line.
                    }

                    lines[i] = line; // Update the line.
                    taskFound = true; // Set task found flag to true.
                    break; // Break the loop.
                }
            }

            if (taskFound) // Check if the task was found.
            {
                File.WriteAllLines(serviceFilePath, lines); // Write the lines to the file.
                string notificationMessage = $"\nTASK_STATUS_CHANGED:{serviceId}:{taskDescription}:{newStatus}"; // Create a notification message.
                PublishNotification(notificationMessage, isAdminChange: true); // Publish the notification.
                Console.WriteLine($"Published notification: {notificationMessage}"); // Print the notification message.
                return "200 OK"; // Return OK response.
            }
            else
            {
                return "404 NOT FOUND - Task not found"; // Return not found response.
            }
        }
        catch (IOException) // Catch IO exceptions.
        {
            return "500 INTERNAL SERVER ERROR - IOException"; // Return internal server error response.
        }
        catch (Exception ex) // Catch any exceptions.
        {
            Console.WriteLine($"Error changing task status: {ex.Message}"); // Print error message.
            return "500 INTERNAL SERVER ERROR"; // Return internal server error response.
        }
    }

    private static bool IsValidStatus(string status) // Method to check if a status is valid.
    {
        string[] validStatuses = { "Nao alocada", "Concluido", "Em curso" }; // Define valid statuses.
        return validStatuses.Contains(status); // Check if the status is valid.
    }

    private static void PrintWorkingDirectory() // Method to print the current working directory.
    {
        string currentDirectory = Directory.GetCurrentDirectory(); // Get the current working directory.
        Console.WriteLine("Current working directory: " + currentDirectory); // Print the current working directory.
    }

    private static void LoadDataFromCSVForAllServices() // Method to load data from CSV files for all services.
    {
        string servicesFilePath = Directory.GetCurrentDirectory(); // Get the current working directory.
        try
        {
            foreach (var serviceFile in Directory.GetFiles(servicesFilePath, "*.csv")) // Get all CSV files in the directory.
            {
                var serviceLines = File.ReadAllLines(serviceFile); // Read all lines from the file.
                string serviceId = Path.GetFileNameWithoutExtension(serviceFile); // Get the service ID from the file name.

                if (!taskDict.ContainsKey(serviceId)) // Check if the task dictionary contains the service ID.
                {
                    taskDict[serviceId] = new List<string>(); // Add the service ID to the task dictionary.
                }

                for (int i = 1; i < serviceLines.Length; i++) // Iterate through the lines, skipping the header.
                {
                    var line = serviceLines[i]; // Get the line.
                    var parts = line.Split(','); // Split the line to get parts.

                    if (parts.Length == 3) // Check if there are only 3 parts.
                    {
                        line = $"{parts[0].Trim()},{parts[1].Trim()},{parts[2].Trim()},"; // Create a new line with an empty field.
                    }
                    else if (parts.Length < 4) // Check if there are less than 4 parts.
                    {
                        line = $"{line.Trim()},"; // Add an empty field to the line.
                    }

                    taskDict[serviceId].Add(line.Trim()); // Add the line to the task dictionary.
                }
            }
        }
        catch (Exception ex) // Catch any exceptions.
        {
            Console.WriteLine($"Error loading data from CSV file {servicesFilePath}: {ex.Message}"); // Print error message.
        }
    }

    public static string AllocateTask(string clientId) // Method to allocate a task.
    {
        mutex.WaitOne(); // Acquire the mutex.
        try
        {
            if (!serviceDict.ContainsKey(clientId)) // Check if the service dictionary contains the client ID.
            {
                return "ERROR: Service not found for client"; // Return error response.
            }

            string serviceId = serviceDict[clientId].ServiceId; // Get the service ID for the client.

            if (taskDict.ContainsKey(serviceId)) // Check if the task dictionary contains the service ID.
            {
                var unallocatedTask = taskDict[serviceId].FirstOrDefault(task => task.Split(',')[2].Trim().ToLower() == "nao alocada"); // Get the first unallocated task.

                if (unallocatedTask != null) // Check if there is an unallocated task.
                {
                    var taskParts = unallocatedTask.Split(','); // Split the task to get parts.

                    if (taskParts.Length != 4) // Check if there are not exactly 4 parts.
                    {
                        return "500 INTERNAL SERVER ERROR"; // Return internal server error response.
                    }

                    taskParts[2] = "Em curso"; // Set the status to "Em curso".
                    taskParts[3] = clientId; // Set the client ID.

                    string updatedTask = string.Join(",", taskParts); // Create an updated task string.
                    int taskIndex = taskDict[serviceId].IndexOf(unallocatedTask); // Get the index of the unallocated task.
                    taskDict[serviceId][taskIndex] = updatedTask; // Update the task dictionary.

                    string serviceFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{serviceId}.csv"); // Create file path for the service.
                    File.WriteAllLines(serviceFilePath, taskDict[serviceId]); // Write the tasks to the file.

                    string message = $"TASK_ALLOCATED:{taskParts[1]}"; // Create an allocation message.

                    return message; // Return the allocation message.
                }
                else
                {
                    return "NO_TASK_AVAILABLE"; // Return no task available response.
                }
            }
            else
            {
                return "NO_TASK_AVAILABLE"; // Return no task available response.
            }
        }
        finally
        {
            mutex.ReleaseMutex(); // Release the mutex.
        }
    }

    public static string MarkTaskAsCompleted(string clientId, string taskDescription) // Method to mark a task as completed.
    {
        mutex.WaitOne(); // Acquire the mutex.
        try
        {
            if (!serviceDict.ContainsKey(clientId)) // Check if the service dictionary contains the client ID.
            {
                return "ERROR: Service not found for client"; // Return error response.
            }

            string serviceId = serviceDict[clientId].ServiceId; // Get the service ID for the client.

            if (taskDict.ContainsKey(serviceId)) // Check if the task dictionary contains the service ID.
            {
                var taskIndex = taskDict[serviceId].FindIndex(task => task.Split(',')[1].Trim() == taskDescription); // Get the index of the task.

                if (taskIndex != -1) // Check if the task was found.
                {
                    var taskParts = taskDict[serviceId][taskIndex].Split(','); // Split the task to get parts.

                    if (taskParts.Length != 4) // Check if there are not exactly 4 parts.
                    {
                        return $"ERROR: Incorrect task format for task: {taskDict[serviceId][taskIndex]}"; // Return error response.
                    }

                    taskParts[2] = "Concluido"; // Set the status to "Concluido".
                    taskParts[3] = clientId; // Set the client ID.

                    string updatedTask = string.Join(",", taskParts); // Create an updated task string.
                    taskDict[serviceId][taskIndex] = updatedTask; // Update the task dictionary.

                    string serviceFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{serviceId}.csv"); // Create file path for the service.
                    File.WriteAllLines(serviceFilePath, taskDict[serviceId]); // Write the tasks to the file.

                    string notificationMessage = $"TASK_COMPLETED:{clientId}: {taskDescription}"; // Create a notification message.
                    PublishNotification(notificationMessage, isAdminChange: false); // Publish the notification.
                    return $"TASK_MARKED_AS_COMPLETED:{taskDescription}"; // Return the completion message.
                }
                else
                {
                    return $"ERROR_TASK_NOT_FOUND:{taskDescription}"; // Return task not found response.
                }
            }
            else
            {
                return $"ERROR_SERVICE_NOT_FOUND:{serviceId}"; // Return service not found response.
            }
        }
        catch (Exception)
        {
            return "500 INTERNAL SERVER ERROR"; // Return internal server error response.
        }
        finally
        {
            mutex.ReleaseMutex(); // Release the mutex.
        }
    }

    private static void PublishNotification(string message, bool isAdminChange) // Method to publish a notification.
    {
        var body = Encoding.UTF8.GetBytes($"{message}"); // Convert the message to bytes.
        string exchange = "service_notifications"; // Define the exchange name.
        string routingKey = $"NOTIFICATION.{message.Split(':')[1]}"; // Define the routing key using the service ID.
        rabbitChannel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: null, body: body); // Publish the message.
        Console.WriteLine($"Sent notification: {message} with routing key {routingKey}"); // Print the notification message.
    }

    private static void PublishUnsubscribeNotification(string clientId, string serviceId) // Method to publish an unsubscribe notification.
    {
        var body = Encoding.UTF8.GetBytes($"\nUNSUBSCRIBE:{clientId}:{serviceId}"); // Convert the unsubscribe message to bytes.
        string exchange = "service_notifications"; // Define the exchange name.
        string routingKey = $"NOTIFICATION.{serviceId}"; // Define the routing key using the service ID.
        rabbitChannel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: null, body: body); // Publish the unsubscribe message.
        Console.WriteLine($"Sent unsubscription notification for client {clientId} from service {serviceId}"); // Print the unsubscribe notification.
    }

    public static Dictionary<string, List<string>> subscriptions = new Dictionary<string, List<string>>(); // Dictionary to store subscriptions.

    private static void SubscribeToService(string clientId, string serviceId) // Method to subscribe to a service.
    {
        if (!subscriptions.ContainsKey(serviceId)) // Check if the subscriptions dictionary contains the service ID.
        {
            subscriptions[serviceId] = new List<string>(); // Add the service ID to the subscriptions dictionary.
        }
        if (!subscriptions[serviceId].Contains(clientId)) // Check if the client ID is already subscribed.
        {
            subscriptions[serviceId].Add(clientId); // Add the client ID to the subscriptions.
        }
        Console.WriteLine($"Client {clientId} subscribed to {serviceId}"); // Print the subscription message.
    }

    private static void UnsubscribeFromService(string clientId, string serviceId) // Method to unsubscribe from a service.
    {
        if (subscriptions.ContainsKey(serviceId)) // Check if the subscriptions dictionary contains the service ID.
        {
            subscriptions[serviceId].Remove(clientId); // Remove the client ID from the subscriptions.
        }
        Console.WriteLine($"Client {clientId} unsubscribed from {serviceId}"); // Print the unsubscription message.
    }
}
