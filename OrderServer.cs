using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

int port = 5000;
int ordersCount = 0;
decimal totalSum = 0;
object locker = new object();

var listener = new TcpListener(IPAddress.Loopback, port);
listener.Start();
Console.WriteLine($"Сервер запущен на порту {port}");

while (true)
{
    TcpClient client = await listener.AcceptTcpClientAsync();
    Console.WriteLine($"Клиент подключён: {client.Client.RemoteEndPoint}");
    _ = HandleClient(client);
}

async Task HandleClient(TcpClient client)
{
    string address = client.Client.RemoteEndPoint.ToString();

    try
    {
        using (client)
        using (NetworkStream stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
        {
            while (true)
            {
                string line = await reader.ReadLineAsync();
                if (line == null) break;

                line = line.Trim();
                Console.WriteLine($"[{address}] {line}");

                if (line.ToUpper() == "EXIT")
                {
                    await writer.WriteLineAsync("Bye!");
                    break;
                }

                string response = ProcessOrder(line);
                await writer.WriteLineAsync(response);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{address}] Ошибка: {ex.Message}");
    }

    Console.WriteLine($"Клиент отключён: {address}");
}

string ProcessOrder(string line)
{
    string[] parts = line.Split(';');

    if (parts[0].ToUpper() != "ORDER")
        return "Error: unknown command";

    if (parts.Length != 4)
        return "Error: format is ORDER;Name;Quantity;Price";

    string product = parts[1].Trim();

    if (product == "")
        return "Error: product name is empty";

    if (!int.TryParse(parts[2], out int quantity))
        return "Error: quantity is not a number";

    if (quantity <= 0)
        return "Error: quantity must be positive";

    if (!decimal.TryParse(parts[3], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price))
        return "Error: price is not a number";

    if (price <= 0)
        return "Error: price must be positive";

    decimal total = quantity * price;

    lock (locker)
    {
        ordersCount++;
        totalSum += total;
        Console.WriteLine($"Всего заказов: {ordersCount}, общая сумма: {totalSum}");
    }

    return $"Order accepted: {product}, Total: {total}";
}