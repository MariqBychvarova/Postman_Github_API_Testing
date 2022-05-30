using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;



namespace API_Testing_Github
{
    public class Test_Github
    {
        private  RestClient client;
        const string userName = "MariqBychvarova";
        const string password = "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";
        const string repo = "QA-Automation-Project-Collections";

        [SetUp]
        public void Setup()
        {
            client = new RestClient("https://api.github.com");
        }

        [Test]
        public async Task Test_Get_Issue_From_Repo_Status_Code()
        {
            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues/1");
            RestResponse response = await client.ExecuteAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task Test_Get_All_Issues_Status_Code()
        {
            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues");
            RestResponse response = await client.ExecuteAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task Test_Get_Comment_For_Issue()
        {
            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues/1/comments");
            RestResponse response = await client.ExecuteAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task Test_Get_Specific_Comment()
        {
            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues/comments/1125746812");
            RestResponse response = await client.ExecuteAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task Test_Get_All_Issues()
        {
            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues");
            RestResponse response = await client.ExecuteAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            List<Issue> issues = JsonSerializer.Deserialize<List<Issue>>(response.Content);

            Assert.That(issues.Count > 1);

            foreach (var issue in issues)
            {
                Assert.Greater(issue.id,0);
                Assert.Greater(issue.number, 0);
                Assert.IsNotEmpty(issue.title);
                Assert.IsNotEmpty(issue.body);
            }           
        }

        [Test]
        public async Task Test_Get_Issue_By_Number()
        {
            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues/1");
            RestResponse response = await client.ExecuteAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.IsTrue(response.ContentType.StartsWith("application/json"));
            
            Issue issue = JsonSerializer.Deserialize<Issue>(response.Content);

            Assert.Greater(issue.id, 0);
            Assert.AreEqual(issue.number, 1);
            Assert.IsNotEmpty(issue.body);
            Assert.IsNotEmpty(issue.title);           
        }

        [Test]
        public async Task Test_Get_Issue_By_Invalid_Number()
        {
            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues/100");
            RestResponse response = await client.ExecuteAsync(request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

       public async Task<Issue> Create_New_Issue(string title,string body)
        {
            RestRequest request = new RestRequest($"repos/{userName}/{repo}/issues");
            request.AddBody(new { body, title });
            RestResponse response = await client.ExecuteAsync(request, Method.Post);
            Issue issue = JsonSerializer.Deserialize<Issue>(response.Content);

            return issue;
        }

        [Test]
        public async Task Test_Create_New_Issue()
        {
            client.Authenticator = new HttpBasicAuthenticator(userName, password);
            string title = "First Issue From RestSharp";
            string body = "Some issue from RestSharp";
            Issue issue = await Create_New_Issue(title, body);

            Assert.Greater(issue.id, 0);
            Assert.Greater(issue.number, 0);
            Assert.IsNotEmpty(issue.title);
            Assert.IsNotEmpty(issue.body);
        }

        public async Task<Issue> Edit_Issue(string body,string title,int issueNumber)
        {
            client.Authenticator = new HttpBasicAuthenticator(userName, password);
            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues/{issueNumber}");
            request.AddBody(new { body, title });
            RestResponse response = await client.ExecuteAsync(request, Method.Patch);
            Issue issue = JsonSerializer.Deserialize<Issue>(response.Content);

            return issue;
        }
        [Test]
        public async Task Test_Edit_Existing_Issue()
        {
            string body = "Some issue edited";
            string title = "This issue is edited";
            int issueNumber = 5;

            Issue editedIssue = await Edit_Issue(body, title, issueNumber);

            Assert.AreEqual(editedIssue.body, body);
            Assert.AreEqual(editedIssue.title, title);
            Assert.AreEqual(editedIssue.number, issueNumber);            
        }

        [Test]
        public async Task Test_Edit_Non_Existing_Issue()
        {
            client.Authenticator = new HttpBasicAuthenticator(userName, password);
            string body = "Some issue edited";
            string title = "This issue is edited";
            int issueNumber = 100;

            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues/{issueNumber}");
            request.AddBody(new { body, title });
            RestResponse response = await client.ExecuteAsync(request, Method.Patch);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Test_Edit_Issue_Without_Authentication()
        {
            string body = "Some issue edited";
            string title = "This issue is edited";
            int issueNumber = 5;

            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues/{issueNumber}");
            request.AddBody(new { body, title });
            RestResponse response = await client.ExecuteAsync(request, Method.Patch);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task Test_Edit_Comment()
        {
            int commentId = 1140845092;
            string body = "Just edited";
          
            client.Authenticator = new HttpBasicAuthenticator(userName, password);
            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues/comments/{commentId}");
            request.AddBody(new { body });
            RestResponse response = await client.ExecuteAsync(request, Method.Patch);
            Issue issue = JsonSerializer.Deserialize<Issue>(response.Content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(issue.id, commentId);
            Assert.AreEqual(issue.body, body);
        }

        [Test]
        public async Task Test_Edit_Non_Existing_Comment()
        {
            int commentId = 00000000;
            string body = "Just edited";
            client.Authenticator = new HttpBasicAuthenticator(userName, password);
            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues/comments/{commentId}");
            request.AddBody(new { body });
            RestResponse response = await client.ExecuteAsync(request, Method.Patch);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Test_Edit_Comment_Without_Authenticaton()
        {
            int commentId = 1140845092;
            string body = "Just edited";
            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues/comments/{commentId}");
            request.AddBody(new { body });
            RestResponse response = await client.ExecuteAsync(request, Method.Patch);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        
        [Test]
        public async Task Test_Delete_Existing_Comment()
        {
            string body = "some comment to be deleted";
            int issueNumber = 5;

            client.Authenticator = new HttpBasicAuthenticator(userName, password);
            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues/{issueNumber}/comments");
            request.AddBody(new { body });
            RestResponse response = await client.ExecuteAsync(request, Method.Post);
            Issue issue = JsonSerializer.Deserialize<Issue>(response.Content);
            int commentId =issue.id;

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.Greater(issue.id, 0);
            Assert.IsNotEmpty(issue.body);

            RestRequest newRequest = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues/comments/{commentId}");
            RestResponse newResponse = await client.ExecuteAsync(newRequest, Method.Delete);

            Assert.AreEqual(HttpStatusCode.NoContent, newResponse.StatusCode);
        }

        [Test]
        public async Task Test_Delete_Non_Existing_Comment()
        {
            client.Authenticator = new HttpBasicAuthenticator(userName, password);
            int commentId = 0000000;
            RestRequest request = new RestRequest($"https://api.github.com/repos/{userName}/{repo}/issues/comments/{commentId}");
            RestResponse response = await client.ExecuteAsync(request, Method.Delete);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }      
    }
}