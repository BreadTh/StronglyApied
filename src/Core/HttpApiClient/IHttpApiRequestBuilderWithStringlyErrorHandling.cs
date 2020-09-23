namespace BreadTh.StronglyApied
{
    public interface IHttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME>
    {
        IHttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME> AddHeader(string name, string value);
        IHttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME> AddJsonBody(object body);
        ICallResultParserWithStringlyErrorHandling<OUTCOME> PerformCall();
    }
}