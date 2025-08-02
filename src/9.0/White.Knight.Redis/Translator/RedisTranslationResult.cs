namespace White.Knight.Redis.Translator
{
    public class RedisTranslationResult
    {
        public string Query { get; set; }

        public string SortByField { get; set; }

        public bool? SortDescending { get; set; }

        public int? Page { get; set; }

        public int? PageSize { get; set; }
    }
}