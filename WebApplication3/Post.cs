using Newtonsoft.Json.Linq;

namespace WebApplication3
{
    public class Post
    {
        public int Id { get; set; }
        public string address { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string by { get; set; }
        public DateTime datePublish { get; set; }
        public DateTime dateStart { get; set; }
        public DateTime dateFinish { get; set; }
        public string title {  get; set; }
        public string text { get; set; }
        public string type { get; set; }
        public JObject tags { get; set; }
        public JObject images { get; set; }
    }
}
