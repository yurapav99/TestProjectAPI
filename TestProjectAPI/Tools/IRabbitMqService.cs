namespace TestProjectAPI.Tools
{
    public interface IRabbitMqService
    {
        void SendMessage(string message);
    }
}
