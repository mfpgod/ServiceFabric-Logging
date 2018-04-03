namespace Shared.ServiceFabric.Correlation
{
    internal interface IMethodNameProvider
    {
        string GetMethodName(int interfaceId, int methodId);
    }
}
