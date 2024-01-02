namespace DapperSqlGenerator.Console.Generator.Repositories
{
    public interface IRepositoryMethodsGenerator
    {
        string GenerateInsertMethod();
        string GenerateUpdateMethod(); 
        string GenerateDeleteMethod();
        string GenerateDeleteByExpressionMethod();
        string GenerateGetAllMethod();
        string GenerateGetByPKMethod();
        string GenerateGetByExpressionMethod();
    }
}
