using System.Globalization;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
class Program
{
    static string idInstance = "1103912324";
    static string apiTokenInstance = "6e579cfae2194032b03f9c9d20fd7e6263ec7402b1d64f3384";
    static readonly HttpClient client = new HttpClient();
    static List<string> nomberReCall = new List<string> { "79642181512" };

	static async Task Main()
    {
        GetCreditorNomber();

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
            //foreach (var debtor in debtors)
            //{
            //    // Пример условия: отправка сообщения раз в 30 дней
            //    if (DateTime.Now.Day % 30 == 0)
            //    {
            //        await SendCallNotification(debtor.ChatId, debtor.Message);
            //    }
            //}
        }
        catch (Exception e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }

    static async Task GetNotification()
    {
		DateTime currentTimeUtc = DateTime.UtcNow;

		// Выбор часового пояса, например "Eastern Standard Time" для восточного побережья США
		TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("North Asia East Standard Time");

		// Конвертация UTC времени в выбранный часовой пояс
		DateTime currentTimeZone = TimeZoneInfo.ConvertTimeFromUtc(currentTimeUtc, easternZone);
		TimeSpan currentTime = currentTimeZone.TimeOfDay;
		var startTime = new TimeSpan(10, 0, 0); // 18:00
        var endTime = new TimeSpan(9, 0, 0); // 09:00

		// Проверяем, находится ли текущее время вне диапазона с 09:00 до 18:00
		if (currentTime > endTime && currentTime < startTime)
        {
            Console.WriteLine($"Рабочее время {currentTime.Hours} ");
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
                        foreach (var nomber in nomberReCall)
                        {
                            await SendCallNotification($"{nomber}@c.us", $"Вам звонил номер {notifications.Body.From.Substring(0, notifications.Body.From.Length - 5)} в нерабочее время!");
                        }
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
    static void GetCreditorNomber()
    {
		string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
		string ApplicationName = "Google Sheets API .NET Quickstart";
		string spreadsheetId = "1dpg2LYy3QA8YllteHdnQOYOnhLilUV2K0Q-LP8XDPAk";
		string range = "Лист2!A:Z"; // Укажите диапазон, который хотите прочитать

		GoogleCredential credential;

		using (var stream = new FileStream("D://notificationcreditor-4f4dd8981059.json", FileMode.Open, FileAccess.Read))
		{
			credential = GoogleCredential.FromStream(stream)
				.CreateScoped(Scopes);
		}

		// Создание Google Sheets API сервиса.
		var service = new SheetsService(new BaseClientService.Initializer()
		{
			HttpClientInitializer = credential,
			ApplicationName = ApplicationName,
		});

		// Чтение данных
		SpreadsheetsResource.ValuesResource.GetRequest request =
				service.Spreadsheets.Values.Get(spreadsheetId, range);

		ValueRange response = request.Execute();
		IList<IList<Object>> values = response.Values;
		DateTime now = DateTime.Now;
        int nowMountRow = 0;

		switch (now.Month)
        {
            case 9:
                {
					nowMountRow = 2; break;
                }
            case 10:
                {
					nowMountRow = 3; break;
                }
			case 11:
				{
					nowMountRow = 4; break;
				}
			case 12:
				{
					nowMountRow = 5; break;
				}
			case 1:
				{
					nowMountRow = 6; break;
				}
			case 2:
				{
					nowMountRow = 7; break;
				}
			case 3:
				{
					nowMountRow = 8; break;
				}
			case 4:
				{
					nowMountRow = 9; break;
				}
			case 5:
				{
					nowMountRow = 10; break;
				}
			case 6:
				{
					nowMountRow = 11; break;
				}
		}
		string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(now.Month);

		if (values != null && values.Count > 0)
		{
            foreach (var row in values)
            {
				if ((long.TryParse(row[nowMountRow-1].ToString(), out long resultBackMounth) && resultBackMounth != 0) && (long.TryParse(row[nowMountRow].ToString(), out long resultCurMounth) && resultCurMounth == 0) && (long.TryParse(row[nowMountRow+1].ToString(), out long resultNextMounth) && resultNextMounth == 0))
				{
                    if(row.Count>12) Console.WriteLine($"{row[0] + $" не внесли деньги за {monthName} - " + row[12].ToString()}");
                }
            }
		}
		else
		{
			Console.WriteLine("No data found.");
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