using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using DatabasesII;

namespace DatabasesII
{
    class Customer
    {
		DBConn databaseConfig = new DBConn();

		public static readonly string table = "clients";

		public int Id { get; private set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime Birthday { get; set; }
		public Customer(int id, string firstname, string lastname, DateTime birthday)
		{
			this.Id = id;
			this.FirstName = firstname;
			this.LastName = lastname;
			this.Birthday = birthday;
		}

		public Customer(string firstname, string lastname, DateTime birthday)
		{
			this.Id = Customer.GetAll().Max(c => c.Id) + 1;
			this.FirstName = firstname;
			this.LastName = lastname;
			this.Birthday = birthday;
		}

		public void Delete()
		{

			using (NpgsqlConnection conn = new NpgsqlConnection(databaseConfig.connString))
			{
				conn.Open();
				using (var command = new NpgsqlCommand($"DELETE FROM {table} WHERE client_id = @Id", conn))
				{
					command.Parameters.AddWithValue("@Id", Id);
					command.ExecuteNonQuery();
				}
			}
		}

		public static IEnumerable<Customer> GetAll()
		{

			List<Customer> clients = new List<Customer>();
			DBConn databaseConfig = new DBConn();
			using (NpgsqlConnection conn = new NpgsqlConnection(databaseConfig.connString))
			{
				conn.Open();
				using (var command = new NpgsqlCommand($"SELECT * FROM {table}", conn))
				{
					NpgsqlDataReader reader = command.ExecuteReader();
					while (reader.Read())
						clients.Add(new Customer((int)reader["client_id"], (string)reader["first_name"], (string)reader["last_name"], (DateTime)reader["birthday"]));

					if (clients.Count() != 0)
						return clients;
				}
			}
			return null;
		}

		public static Customer GetByID(int id)
		{
			DBConn databaseConfig = new DBConn();
			using (NpgsqlConnection conn = new NpgsqlConnection(databaseConfig.connString))
			{
				conn.Open();
				using (var command = new NpgsqlCommand($"SELECT * FROM {table} WHERE client_id = @Id", conn))
				{
					command.Parameters.AddWithValue("@Id", id);

					NpgsqlDataReader reader = command.ExecuteReader();
					if (reader.HasRows)
					{
						reader.Read();
						return new Customer(id, (string)reader["first_name"], (string)reader["last_name"], (DateTime)reader["birthday"]);
					}
				}
			}
			return null;
		}

		public void Save()
		{
			using (NpgsqlConnection conn = new NpgsqlConnection(databaseConfig.connString))
			{
				conn.Open(); 

				using (var command = new NpgsqlCommand($"INSERT INTO {table}(client_id, first_name, last_name, birthday) " +
					"VALUES (@Id, @FirstName, @LastName, @Birthday) " +
					"ON CONFLICT (client_id) DO UPDATE " +
					"SET first_name = @FirstName, last_name = @LastName, birthday = @Birthday", conn))
				{
					command.Parameters.AddWithValue("@Id", Id);
					command.Parameters.AddWithValue("@FirstName", FirstName);
					command.Parameters.AddWithValue("@LastName", LastName);
					command.Parameters.AddWithValue("@Birthday", Birthday);

					command.ExecuteNonQuery();
				}
			}
		}
	}
}
