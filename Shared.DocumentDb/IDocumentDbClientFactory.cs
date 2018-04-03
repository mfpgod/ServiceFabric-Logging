namespace Shared.DocumentDb
{
    using Microsoft.Azure.Documents;

    public interface IDocumentDbClientFactory
    {
        IDocumentClient GetClient();
    }
}
