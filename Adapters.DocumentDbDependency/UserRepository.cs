namespace Adapters.DocumentDbDependency
{
    using Services.Contracts.Adapters.DocumentDbDependency;
    using Shared.DocumentDb;

    public class UserRepository : DocumentDbRepository<UserEntity>
    {
        public UserRepository(IDocumentDbClientFactory documentDbInitializer)
            : base(documentDbInitializer, null)
        {
        }

        public override string DatabaseId => UserCollectionInitializer.DatabaseId;

        public override string CollectionId => UserCollectionInitializer.CollectionId;
    }
}