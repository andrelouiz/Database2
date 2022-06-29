using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabasesII;
using Npgsql;

namespace DVD_Rental_Store_App
{

	class Actor
	{
		DBConn databaseConfig = new DBConn();

		public static readonly string table = "actors";
		public int Id { get; private set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime Birthday { get; set; }
		public Actor(int id, string firstname, string lastname, DateTime birthday)
		{
			this.Id = id;
			this.FirstName = firstname;
			this.LastName = lastname;
			this.Birthday = birthday;
		}

		public void Delete()
		{
			using (NpgsqlConnection conn = new NpgsqlConnection(databaseConfig.connString))
			{
				conn.Open();
				using (var command = new NpgsqlCommand($"DELETE FROM {table} WHERE actor_id = @Id", conn))
				{
					command.Parameters.AddWithValue("@Id", Id);
					command.ExecuteNonQuery();
				}
			}
		}

		public static IEnumerable<Actor> GetAll()
		{
			List<Actor> actors = new List<Actor>();
			DBConn databaseConfig = new DBConn();
			using (NpgsqlConnection conn = new NpgsqlConnection(databaseConfig.connString))
			{
				conn.Open();
				using (var command = new NpgsqlCommand($"SELECT * FROM {table}", conn))
				{
					NpgsqlDataReader reader = command.ExecuteReader();
					while (reader.Read())
						actors.Add(new Actor((int)reader["actor_id"], (string)reader["first_name"], (string)reader["last_name"], (DateTime)reader["birthday"]));

					if (actors.Count() != 0)
						return actors;
				}
			}
			return null;
		}

		public static Actor GetByID(int id)
		{
			DBConn databaseConfig = new DBConn();
			using (NpgsqlConnection conn = new NpgsqlConnection(databaseConfig.connString))
			{
				conn.Open();
				using (var command = new NpgsqlCommand($"SELECT * FROM {table} WHERE actor_id = @Id", conn))
				{
					command.Parameters.AddWithValue("@Id", id);

					NpgsqlDataReader reader = command.ExecuteReader();
					if (reader.HasRows)
					{
						reader.Read();
						return new Actor(id, (string)reader["first_name"], (string)reader["last_name"], (DateTime)reader["birthday"]);
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

				using (var command = new NpgsqlCommand($"INSERT INTO {table}(actor_id, first_name, last_name, birthday) " +
					"VALUES (@Id, @FirstName, @LastName, @Birthday) " +
					"ON CONFLICT (actor_id) DO UPDATE " +
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
