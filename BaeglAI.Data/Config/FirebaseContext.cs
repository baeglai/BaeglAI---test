using Google.Cloud.Firestore;

public class FirebaseContext
    {
        public HttpClient HttpClient { get; }
        public string ApiKey { get; }
        public string ProjectId { get; }
        public string BaseUrl => $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";

        public FirebaseContext(HttpClient httpClient, string apiKey, string projectId)
        {
            HttpClient = httpClient;
            ApiKey = apiKey;
            ProjectId = projectId;
        }

        public string GetCollectionUrl(string collectionPath)
        {
            return $"{BaseUrl}/{collectionPath}?key={ApiKey}";
        }

        public string GetDocumentUrl(string collectionPath, string documentId)
        {
            return $"{BaseUrl}/{collectionPath}/{documentId}?key={ApiKey}";
        }
    }