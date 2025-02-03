using Azure.AI.OpenAI;
using Financial_ForeCast.Models;
using OpenAI.Chat;
using System.Text;
using System.Text.Json;

namespace DBChatPro.Services
{
    // Use this constructor if you're using vanilla OpenAI instead of Azure OpenAI
    // Make sure to update your Program.cs as well
    //public class OpenAIService(OpenAIClient aiClient)
    

    public class OpenAIService(AzureOpenAIClient aiClient)
    {
        
        public async Task<IncomeExpense> GetAISQLQuery(string userPrompt, string type = "")
        {

            var deploymentName = "gpt-4o";
            ChatClient chatClient = aiClient.GetChatClient(deploymentName);
            
            List<ChatMessage> chatHistory = new List<ChatMessage>();
            var builder = new StringBuilder();

            builder.AppendLine("Your are a helpful, cheerful data input assistant. Do not respond with any information unrelated to taking the users input," +
                "and turning it into valid form data for my application.");


            builder.AppendLine("Include column name headers in the results.");
            builder.AppendLine("Always provide your answer in the JSON format below:");
            builder.AppendLine(@"{ ""Name"": ""name-given"", ""amount"":  ""amount-given"",  ""type"":  ""type-given""}");
            builder.AppendLine("Output ONLY JSON formatted on a single line. Do not use new line characters.");
            builder.AppendLine(@"In the preceding JSON response, substitute ""name-given"" with the name you think the user gave you.");
            builder.AppendLine(@"In the preceding JSON response, substitute ""amount-given"" with the amount you think the user gave you.");
            if (type == "") 
            {
                builder.AppendLine(@"In the preceding JSON response, substitute ""type-given"" with the type you think the user gave you. that is one of the following listed in the array below:");
                builder.AppendLine(@"[""Savings"",""Checkings"",""Credit Card"",""Loan"",""Asset Value""]");
            }
            else
            {
                builder.AppendLine(@"In the preceding JSON response, substitute ""type-given"" with the passed in value below: ");
                builder.AppendLine(type);
            }
            //builder.AppendLine("Do not use MySQL syntax.");
            //builder.AppendLine("Always limit the SQL Query to 100 rows.");
            //builder.AppendLine("Always include all of the table columns and details.");

            // Build the AI chat/prompts
            chatHistory.Add(new SystemChatMessage(builder.ToString()));
            chatHistory.Add(new UserChatMessage(userPrompt));

            // Send request to Azure OpenAI model
            var response = await chatClient.CompleteChatAsync(chatHistory);
            var responseContent = response.Value.Content[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", "");

            try
            {
                return JsonSerializer.Deserialize<IncomeExpense>(responseContent);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to parse AI response as a SQL Query. The AI response was: " + response.Value.Content[0].Text);
            }
        }

        public async Task<ChatCompletion> ChatPrompt(List<ChatMessage> prompt)
        {
            var deploymentName = "gpt-4o";
            ChatClient chatClient = aiClient.GetChatClient(deploymentName);

            return (await chatClient.CompleteChatAsync(prompt)).Value;
        }
    }
}
