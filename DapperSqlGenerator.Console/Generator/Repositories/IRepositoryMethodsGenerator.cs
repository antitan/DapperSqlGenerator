namespace DapperSqlGenerator.App.Generator.Repositories
{
    public interface IRepositoryMethodsGenerator
    {
        string GenerateInsertMethod();
        string GenerateUpdateMethod(); 
        string GenerateDeleteMethod();
        string GenerateDeleteByExpressionMethod();
        string GenerateGetAllMethod();
        string GenerateGePaginatedMethod();
        string GenerateGetByPKMethod();
        string GenerateGetByExpressionMethod();

        //transaction methods
        string GenerateInsertTransactionMethod();
        string GenerateUpdateTransactionMethod();
        string GenerateDeleteTransactionMethod(); 
    }
}
