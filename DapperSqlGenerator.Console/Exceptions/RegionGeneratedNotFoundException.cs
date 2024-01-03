using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.Console.Exceptions
{
    public class RegionGeneratedNotFoundException : Exception
    {
        public RegionGeneratedNotFoundException()
        {

        }
        public RegionGeneratedNotFoundException(string message) : base(message)
        {
                
        }
    }
}
