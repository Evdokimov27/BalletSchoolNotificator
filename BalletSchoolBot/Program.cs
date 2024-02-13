using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using System.Numerics;

class Program
{
    static string idInstance = "1103902967";
    static string apiTokenInstance = "752c49c880144f91a08de861efe943b0cfcd675889fc4cd48b";
    static string webAppUrl = "ВАШ_URL_ВЕБ-ПРИЛОЖЕНИЯ";
    static readonly HttpClient client = new HttpClient();
  
    static async Task Main()
    {
        while (true)
        {
            Console.WriteLine("Checking for messages and debts...");
            await GetNotification();
           // await SendDebtorMounth();
           // await SendCreditor();
            await Task.Delay(1000); // Задержка в 1 секунду между запросами
        }
    }

    static async Task SendCreditor()
    {
        List<string> idChats = new List<string> { "120363239727653355@c.us" };
        try
        {
            foreach (var chatId in idChats)
            {
                // отправка сообщения раз в 3 дня
                if (DateTime.Now.Day % 13 == 0)
                {
                    await SendCallNotification(chatId, "Сообщение для отправки");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }

    static async Task SendDebtorMounth()
    {
        try
        {
            string responseJson = await client.GetStringAsync(webAppUrl);
            var debtors = JsonConvert.DeserializeObject<List<Debtor>>(responseJson);

            foreach (var debtor in debtors)
            {
                // Пример условия: отправка сообщения раз в 30 дней
                if (DateTime.Now.Day % 30 == 0)
                {
                    await SendCallNotification(debtor.ChatId, debtor.Message);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }

    static async Task GetNotification()
    {
        var currentTime = DateTime.Now.TimeOfDay;
        var startTime = new TimeSpan(18, 20, 0); // 18:00
        var endTime = new TimeSpan(9, 0, 0); // 09:00

        // Проверяем, находится ли текущее время вне диапазона с 09:00 до 18:00
        if (currentTime > endTime && currentTime < startTime)
        {
            Console.WriteLine("Рабочее время.");
            string urlTime = $"https://api.green-api.com/waInstance{idInstance}/receiveNotification/{apiTokenInstance}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(urlTime);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    Root notifications = JsonConvert.DeserializeObject<Root>(responseBody);


                    if (responseBody != null && notifications?.ReceiptId != null)
                    {
                        await DeleteNotification(notifications.ReceiptId.ToString());
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            return;
        }
        Console.WriteLine("Не рабочее время.");
        string url = $"https://api.green-api.com/waInstance{idInstance}/receiveNotification/{apiTokenInstance}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    Root notifications = JsonConvert.DeserializeObject<Root>(responseBody);


                    if (responseBody != null && notifications?.ReceiptId != null)
                    {
                        Console.WriteLine($"ReceiptId: {notifications.ReceiptId}");
                        Console.WriteLine($"Type: {notifications.Body.TypeWebhook}");
                        if (notifications.Body.TypeWebhook == "incomingCall" && notifications.Body.Status == "offer")
                        {
                            await SendCallNotification(notifications.Body.InstanceData.Wid, $"Вам звонил номер {notifications.Body.From.Substring(0, notifications.Body.From.Length - 5)} в нерабочее время!");
                            await SendCallNotification(notifications.Body.From, $"Вы позвонили в нерабочее время, мы уведомлены, что вы звонили и обязательно перезвоним вам!");
                        }
                        await DeleteNotification(notifications.ReceiptId.ToString());
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
    }
    static async Task DeleteNotification(string notificationId)
    {
        string url = $"https://api.green-api.com/waInstance{idInstance}/deleteNotification/{apiTokenInstance}/{notificationId}";

        try
        {
            HttpResponseMessage response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Notification {notificationId} deleted successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to delete notification {notificationId}. Response StatusCode: {response.StatusCode}");
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }
    static async Task SendCallNotification(string chatId, string message)
    {
        string url = $"https://api.green-api.com/waInstance{idInstance}/sendMessage/{apiTokenInstance}";

        var payload = new
        {
            chatId = chatId,
            message = message
        };

        var jsonPayload = JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Message sent successfully.");
                Console.WriteLine(responseBody);
            }
            else
            {
                Console.WriteLine($"Failed to send message. Response StatusCode: {response.StatusCode}");
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }
}
    public class Root
{
    [JsonProperty("receiptId")]
    public int ReceiptId { get; set; }

    [JsonProperty("body")]
    public Body Body { get; set; }
}

public class Body
{
    [JsonProperty("from")]
    public string From { get; set; }
    
    [JsonProperty("typeWebhook")]
    public string TypeWebhook { get; set; }

    [JsonProperty("instanceData")]
    public InstanceData InstanceData { get; set; }

    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("idMessage")]
    public string IdMessage { get; set; }

    [JsonProperty("senderData")]
    public SenderData SenderData { get; set; }

    [JsonProperty("messageData")]
    public MessageData MessageData { get; set; }
}

public class InstanceData
{
    [JsonProperty("idInstance")]
    public long IdInstance { get; set; }

    [JsonProperty("wid")]
    public string Wid { get; set; }

    [JsonProperty("typeInstance")]
    public string TypeInstance { get; set; }
}
    class Debtor
    {
        public string ChatId { get; set; }
        public string Message { get; set; }
    }
    public class SenderData
{
    [JsonProperty("chatId")]
    public string ChatId { get; set; }

    [JsonProperty("chatName")]
    public string ChatName { get; set; }

    [JsonProperty("sender")]
    public string Sender { get; set; }

    [JsonProperty("senderName")]
    public string SenderName { get; set; }
}

public class MessageData
{
    [JsonProperty("typeMessage")]
    public string TypeMessage { get; set; }

    [JsonProperty("textMessageData")]
    public TextMessageData TextMessageData { get; set; }
}

public class TextMessageData
{
    [JsonProperty("textMessage")]
    public string TextMessage { get; set; }
}