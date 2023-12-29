namespace DapperSqlGenerator.Console.Generator.Repositories
{
    public interface IRepositoryMethodsGenerator
    {
        string GenerateInsertMethod();
        string GenerateUpdateMethod();
        string GenerateDeleteMethod();
        string GenerateGetAllMethod();
        string GenerateGetByPKMethod();
    }
}
