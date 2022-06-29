using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using DatabasesII;


namespace DatabasesII
{
    public class DBConn
    {
        public string connString = "Server=localhost;User Id=postgres;Password=334334;Database=rental;Port=5432";

        public DBConn()
        {
            
        }
    }
}
