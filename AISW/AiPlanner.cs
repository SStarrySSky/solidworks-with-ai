using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AISW
{
    // AI 返回的方块建模计划
    public class BlockPlan
    {
        [JsonProperty("operation")]
        public string Operation { get; set; }   // "create_block"

        [JsonProperty("plane")]
        public string Plane { get; set; }       // "Front" | "Top" | "Right"

        [JsonProperty("width")]
        public double Width { get; set; }       // mm

        [JsonProperty("height")]
        public double Height { get; set; }      // mm

        [JsonProperty("thickness")]
        public double Thickness { get; set; }   // mm

        [JsonProperty("unit")]
        public string Unit { get; set; }        // "mm"
    }

    public static class AiPlanner
    {
        // 先尝试从环境变量拿 key，拿不到就用硬编码（自己改）
        private static readonly string ApiKey = 
            Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? "your api key"; // TODO：把你的 key 填在这里，或配置环境变量

        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task<string> CreateBlockPlanAsync(string userPrompt)
        {
            if (string.IsNullOrWhiteSpace(ApiKey) || ApiKey == "YOUR_API_KEY_HERE")
            {
                throw new Exception("还没有设置 OpenAI API Key，请在 AiPlanner.ApiKey 里填好，或者配置环境变量 OPENAI_API_KEY。");
            }

            var requestBody = new
            {
                model = "gpt-4.1-mini",  // 你可以换成 gpt-4o-mini 或 gpt-4.1
                messages = new object[]
                {
                    new {
                        role = "system",
                        content =
                            "你是一个 SolidWorks 建模规划器。" +
                            "根据用户的自然语言描述，识别一个长方体特征的参数。" +
                            "所有尺寸单位用毫米（mm）。" +
                            "只允许输出一个 JSON，对象结构必须是：" +
                            "{ \"operation\": \"create_block\", " +
                            "\"plane\": \"Front\" | \"Top\" | \"Right\", " +
                            "\"width\": <number>, \"height\": <number>, \"thickness\": <number>, " +
                            "\"unit\": \"mm\" }。" +
                            "不要输出任何解释、注释、额外文字，也不要包在代码块里。"
                    },
                    new {
                        role = "user",
                        content = userPrompt
                    }
                },
                response_format = new { type = "json_object" },
                temperature = 0
            };

            string bodyJson = JsonConvert.SerializeObject(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
            request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string respText = await response.Content.ReadAsStringAsync();

            // 解析 choices[0].message.content
            var root = JObject.Parse(respText);
            string content = (string)root["choices"][0]["message"]["content"];

            if (content == null)
            {
                throw new Exception("AI 返回的 content 为空。");
            }

            // 防止被 ```json 包裹
            if (content.StartsWith("```"))
            {
                int firstNewLine = content.IndexOf('\n');
                int lastFence = content.LastIndexOf("```", StringComparison.Ordinal);
                if (firstNewLine >= 0 && lastFence > firstNewLine)
                {
                    content = content.Substring(firstNewLine + 1, lastFence - firstNewLine - 1).Trim();
                }
            }

            // 验证结构是否能反序列化成 BlockPlan
            JsonConvert.DeserializeObject<BlockPlan>(content);

            return content;
        }
    }
}
