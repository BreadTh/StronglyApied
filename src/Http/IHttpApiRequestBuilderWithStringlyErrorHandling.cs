namespace BreadTh.StronglyApied.Http
{
    public interface IHttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME>
    {
        IHttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME> AddHeader(string name, string value);
        IHttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME> AddJsonBody(object body);
        IHttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME> AddParameter(string name, string value);
        ICallResultParserWithStringlyErrorHandling<OUTCOME> PerformCall();
    }
}