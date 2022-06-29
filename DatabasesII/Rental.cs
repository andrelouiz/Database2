using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using DatabasesII;

namespace DatabasesII
{
    class Rental
    {
        DBConn databaseConfig = new DBConn();
		public static readonly string table = "rentals";
		public int CopyId { get; private set; }
		public int ClientId { get; set; }
		public DateTime DateOfRental { get; set; }
		public DateTime? DateOfReturn { get; set; }
		public Rental(int copyId, int clientId, DateTime dateOfRental, DateTime? dateOfReturn)
		{
			this.CopyId = copyId;
			this.ClientId = clientId;
			this.DateOfRental = dateOfRental;
			this.DateOfReturn = dateOfReturn;
		}

		public Rental(int copyId, int clientId, DateTime dateOfRental)
		{
			this.CopyId = copyId;
			this.ClientId = clientId;
			this.DateOfRental = dateOfRental;
			this.DateOfReturn = null;
		}

		public void Delete()
		{
			using (NpgsqlConnection conn = new NpgsqlConnection(databaseConfig.connString))
			{
				conn.Open();
				using (var command = new NpgsqlCommand($"DELETE FROM {table} WHERE copy_id = @CopyId AND client_id = @ClientId AND date_of_rental = @DateOfRental AND date_of_return = @DateOfReturn", conn))
				{
					command.Parameters.AddWithValue("@CopyId", CopyId);
					command.Parameters.AddWithValue("@ClientId", ClientId);
					command.Parameters.AddWithValue("@DateOfRental", DateOfRental);
					if (DateOfReturn == null)
						command.Parameters.AddWithValue("@DateOfReturn", DBNull.Value);
					else
						command.Parameters.AddWithValue("@DateOfReturn", DateOfReturn);
					command.ExecuteNonQuery();
				}
			}
		}

		public static IEnumerable<Rental> GetAll()
		{
			DBConn databaseConfig = new DBConn();

			List<Rental> Rentals = new List<Rental>();

			using (NpgsqlConnection conn = new NpgsqlConnection(databaseConfig.connString))
			{
				conn.Open();
				using (var command = new NpgsqlCommand($"SELECT * FROM {table}", conn))
				{
					NpgsqlDataReader reader = command.ExecuteReader();
					while (reader.Read())
						Rentals.Add(new Rental((int)reader["copy_id"], (int)reader["client_id"], (DateTime)reader["date_of_rental"], (reader["date_of_return"] == DBNull.Value) ? (DateTime?)null : (DateTime)reader["date_of_return"]));

					if (Rentals.Count() != 0)
						return Rentals;
				}
			}
			return null;
		}

		public void InsertAndSave()
		{
			DBConn databaseConfig = new DBConn();
			using (NpgsqlConnection conn = new NpgsqlConnection(databaseConfig.connString))
			{
				conn.Open();

				using (var command = new NpgsqlCommand($"INSERT INTO {table}(copy_id, client_id, date_of_rental, date_of_return) " +
					"VALUES (@CopyId, @ClientId, @DateOfRental, @DateOfReturn) ", conn))
				{
					command.Parameters.AddWithValue("@CopyId", CopyId);
					command.Parameters.AddWithValue("@ClientId", ClientId);
					command.Parameters.AddWithValue("@DateOfRental", DateOfRental);
					if (DateOfReturn == null)
						command.Parameters.AddWithValue("@DateOfReturn", DBNull.Value);
					else
						command.Parameters.AddWithValue("@DateOfReturn", DateOfReturn);

					command.ExecuteNonQuery();
				}
			}
		}

		public void Return()
		{
			DBConn databaseConfig = new DBConn();
			using (NpgsqlConnection conn = new NpgsqlConnection(databaseConfig.connString))
			{
				conn.Open();

				using (var command = new NpgsqlCommand($"UPDATE {table} " +
					"SET date_of_return = @DateOfReturn " +
					"WHERE copy_id = @CopyId AND client_id = @ClientId AND date_of_rental = @DateOfRental", conn))
				{
					command.Parameters.AddWithValue("@CopyId", CopyId);
					command.Parameters.AddWithValue("@ClientId", ClientId);
					command.Parameters.AddWithValue("@DateOfRental", DateOfRental);
					command.Parameters.AddWithValue("@DateOfReturn", DateTime.Now);

					command.ExecuteNonQuery();
				}
			}
		}

	}
}
