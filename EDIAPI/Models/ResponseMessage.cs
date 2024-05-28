using System.Text.Json.Serialization;

namespace EDIAPI.Models
{
    [JsonSerializable(typeof(ResponseMessage))]
    public class ResponseMessage
    {
        public string Message { get; set; }

        public ResponseMessage(string message)
        {
            Message = message;
        }
    }
}
