namespace Entities {
    class ApiResponse
    {
        public QueryInfo query { get; set; }
        public ResponseData response { get; set; }
    }

    class QueryInfo
    {
        public string tool { get; set; }
        public string host { get; set; }
    }

    class ResponseData
    {
        public int domainCount { get; set; }
        public List<DomainInfo> domains { get; set; }
    }

    class DomainInfo
    {
        public string name { get; set; }
        public string lastResolved { get; set; }
    }
}

