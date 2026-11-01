using System.Net.Sockets;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

try
{
    using var client = new TcpClient();
    await client.ConnectAsync("127.0.0.1", 5000);
    Console.WriteLine("Подключено к серверу");
    Console.WriteLine("Формат: ORDER;Товар;Количество;Цена   (EXIT - выход)");

    using NetworkStream stream = client.GetStream();
    using var reader = new StreamReader(stream, Encoding.UTF8);
    using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

    while (true)
    {
        Console.Write("> ");
        string text = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(text)) continue;

        await writer.WriteLineAsync(text);

        string response = await reader.ReadLineAsync();
        if (response == null)
        {
            Console.WriteLine("Сервер закрыл соединение");
            break;
        }

        Console.WriteLine($"Ответ: {response}");

        if (text.Trim().ToUpper() == "EXIT") break;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
}

Console.WriteLine("Клиент завершён");