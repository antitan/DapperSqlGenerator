namespace DapperSqlGenerator.App.Generator.Repositories
{
    public interface IRepositoryMethodsGenerator
    {
        string GenerateInsertMethod();
        string GenerateUpdateMethod(); 
        string GenerateDeleteMethod();
        string GenerateDeleteByExpressionMethod();
        string GenerateGetAllMethod();
        string GenerateGetAllPaginatedMethod();
        string GenerateGetByPKMethod();
        string GenerateGetByExpressionMethod();
    }
}
