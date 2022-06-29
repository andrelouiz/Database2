using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabasesII;
using Npgsql;

namespace DatabasesII
{
    class Menu
    {
        DBConn databaseConfig = new DBConn();
        public static void Start()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\t▀▄▀▄▀▄ DVD STORE ▄▀▄▀▄▀");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n\t1 - DVD List \n \t2 - Rental List \n \t3 - Create New Rental \n \t4 - DVD Returns\n \t5 - Add new user\n \t6 - Add new movie\n \t7 - Rental Stats\n\t8 - Overdue rentals\n \t9 - Exit\n");
            Console.WriteLine("Insert menu option ");
            while (true)
            {
                switch (Console.ReadLine())
                {
                    case "1":
                        DVDList();
                        break;
                    case "2":
                        RentalList();
                        break;
                    case "3":
                        NewRental();
                        break;
                    case "4":
                        Returns();
                        break;
                    case "5":
                        AddNewUser();
                        break;
                    case "6":
                        AddNewMovie();
                        break;
                    case "7":
                        Stats();
                        break;
                    case "8":
                        Overdue();
                        break;
                    case "9":
                        Environment.Exit(0);
                        break;
                }
            }
            void DVDList()
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("{0,35}{1,10}{2,20}{3,10}", "Title", "Year", "Age Restriction", "Price");
                var movies = Movie.GetAll();

                foreach (Movie m in movies)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("{0,35}{1,10}{2,20}{3,10}", CropString(m.Title, 35), m.Year, m.AgeRestriction, m.Price);
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nCopies:");
                Console.WriteLine("{0,35}{1,20}", "Movie", "Availability");
                Console.ForegroundColor = ConsoleColor.White;
                foreach (Copy c in Copy.GetAll())
                Console.WriteLine("{0,35}{1,20}", CropString(movies.First(obj => obj.Id == c.MovieId).Title, 35), c.Available == true ? "Available" : "Not Available");

                while (true)
                {
                    Return();
                }
            }

            void RentalList()
            {
                int customerID;
                while (true)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Insert customer's ID: ");
                    if (!Int32.TryParse(Console.ReadLine(), out customerID))
                        continue;
                    else
                        break;
                }

                Console.WriteLine("\n\tCustomer:");
                var client = Customer.GetAll().FirstOrDefault(obj => obj.Id == customerID);

                if (client == null)
                    Console.WriteLine("Customer does not exist in the database.");
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0,40}{1,30}", "Full Name", "Birthday");
                    Console.WriteLine("{0,40}{1,30}", CropString($"{client.FirstName} {client.LastName}", 35), client.Birthday);
                    IEnumerable<Rental> rentals = Rental.GetAll().Where(obj => obj.ClientId == customerID);

                    Console.WriteLine("\nRentals:");
                    Console.WriteLine("{0,10}{1,40}{2,30}{3,30}", "Id", "Movie", "Date Of Rental", "Date Of Return");

                    var resultTable = from c in Copy.GetAll()
                                      join r in rentals on c.Id equals r.CopyId
                                      join m in Movie.GetAll() on c.MovieId equals m.Id
                                      where r.ClientId == customerID
                                      select new { CopyId = r.CopyId, MovieTitle = m.Title, DateOfRental = r.DateOfRental, DateOfReturn = r.DateOfReturn };

                    Console.WriteLine("Active:");

                    Console.ForegroundColor = ConsoleColor.White;
                    foreach (var r in resultTable)
                        if (r.DateOfReturn == null) Console.WriteLine("{0,10}{1,40}{2,30}{3,30}", r.CopyId, CropString(r.MovieTitle, 35), r.DateOfRental, "Pending...");

                    Console.WriteLine("\nPrevious rentals:");

                    foreach (var r in resultTable)
                        if (r.DateOfReturn <= DateTime.Now) Console.WriteLine("{0,10}{1,40}{2,30}{3,30}", r.CopyId, CropString(r.MovieTitle, 35), r.DateOfRental, r.DateOfReturn);
                }
                while (true)
                {
                    Return();
                }
            }

            void NewRental()
            {
                string errorMsg = null;
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("New rental");

                    if (errorMsg != null)
                    {
                        Console.WriteLine("Error: " + errorMsg);
                    }

                    Console.Write("Insert write the client ID");
                    String clientId = Console.ReadLine();
                    if (clientId == "go back") return;

                    Console.Write("Insert the copy ID");
                    String copyId = Console.ReadLine();
                    if (copyId == "go back") return;

                    try
                    {
                        Copy copy = Copy.GetByID(Int32.Parse(copyId));
                        if (!copy.Available) throw new Exception("This Copy is not available right now in the database.");

                        Rental rental = new Rental(Int32.Parse(copyId), Int32.Parse(clientId), DateTime.Now);
                        rental.InsertAndSave();

                        copy.Available = false;
                        copy.Save();
                    }
                    catch (Exception e)
                    {
                        errorMsg = e.Message;
                        continue;
                    }

                    Console.WriteLine("Rental added successfully.");
                    break;
                }

                while (true)
                {
                    Return();
                }
            }
            void Returns()
            {
                Console.Clear();
                Console.WriteLine("Register return:");
                int clientId;

                while (true)
                {
                    Console.Clear();
                    Console.Write("Insert the Client ID");
                    if (!Int32.TryParse(Console.ReadLine(), out clientId))
                        continue;
                    else
                        break;
                }

                Console.WriteLine("\nClient:");
                Customer client = Customer.GetAll().Where(obj => obj.Id == clientId).FirstOrDefault();

                if (client == null)
                    Console.WriteLine("Client was not found in the database.");
                else
                {
                    Console.WriteLine("{0,40}{1,30}", "Full Name", "Birthday");

                    Console.WriteLine("{0,40}{1,30}", CropString($"{client.FirstName} {client.LastName}", 35), client.Birthday);
                }

                int copyId;

                while (true)
                {
                    Console.WriteLine("\nInsert write thecopy ID:");
                    if (!Int32.TryParse(Console.ReadLine(), out copyId))
                        continue;
                    else
                        break;
                }

                Copy copy = Copy.GetAll().Where(obj => obj.Id == copyId).FirstOrDefault();

                if (copy == null)
                    Console.WriteLine("Copy was not found in the database.");
                else
                {
                    Rental rental = Rental.GetAll().Where(obj => obj.CopyId == copyId && obj.ClientId == clientId).FirstOrDefault();

                    if (rental == null)
                        Console.WriteLine("\nRental was not found in the databse.");
                    else if (rental.DateOfReturn != null)
                        Console.WriteLine("\nRental has already been returned.");
                    else
                    {
                        rental.Return();

                        Console.WriteLine("\nCopy was successfully returned in the database.");
                    }
                }

                while (true)
                {
                    Return();
                }
            }

            void AddNewUser()
            {
                string errorMsg = null;
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Customer management\n");

                    if (errorMsg != null)
                        Console.WriteLine("Error: " + errorMsg);

                    Console.WriteLine("Insert first name of the customer:");
                    String firstName = Console.ReadLine();

                    Console.WriteLine("Insert last name of the customer");
                    String lastName = Console.ReadLine();

                    Console.WriteLine("Insert birthday of the user (day/month/year):");
                    String birthday = Console.ReadLine();

                    try
                    {
                        Customer client = new Customer(firstName, lastName, DateTime.Parse(birthday));
                        client.Save();
                    }
                    catch (Exception e)
                    {
                        errorMsg = e.Message;
                        continue;
                    }

                    Console.WriteLine("User added successfully.");
                    break;
                }

                while (true)
                {
                    Return();
                }
            }

            void AddNewMovie()
            {
                Console.Clear();
                string errorMsg = null;
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Creation of a new movie in the database.");

                    if (errorMsg != null)
                        Console.WriteLine("Error: " + errorMsg);

                    Console.WriteLine("Insert the title:");
                    String title = Console.ReadLine();

                    Console.WriteLine("Insert the year:");
                    String year = Console.ReadLine();

                    Console.WriteLine("Insert the age restriction:");
                    String ageRestriction = Console.ReadLine();

                    Console.WriteLine("Insert the price:");
                    String price = Console.ReadLine();

                    NpgsqlTransaction transaction = null;
                    NpgsqlConnection connection = null;

                    try
                    {
                        DBConn databaseConfig = new DBConn();
                        connection = new NpgsqlConnection(databaseConfig.connString);
                        connection.Open();
                        transaction = connection.BeginTransaction();

                        Movie movie = new Movie(title, Int32.Parse(year), Int32.Parse(ageRestriction), Int32.Parse(price));
                        movie.Save();

                        Copy copy = new Copy(true, movie.Id);
                        copy.Save();

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        errorMsg = e.Message;

                        transaction?.Rollback();
                        continue;
                    }
                    finally
                    {
                        connection?.Close();
                    }

                    Console.WriteLine("Movie title added successfully.");
                    break;
                }

                while (true)
                {
                    Return();
                }
            }

            void Overdue()
            {
                Console.Clear();
                Console.WriteLine("Overdue rentals:");

                var overdueRentals = Rental.GetAll().Where(obj => obj.DateOfReturn == null && (DateTime.Now - obj.DateOfRental).TotalDays > 14);

                Console.WriteLine("\nRentals:");
                Console.WriteLine("{0,10}{1,40}{2,30}{3,30}", "Id", "Movie", "Date Of Rental", "Date Of Return");

                foreach (var rental in overdueRentals)
                    Console.WriteLine("{0,10}{1,40}{2,30}{3,30}", rental.CopyId, rental.ClientId, rental.DateOfRental, "Pending...");

                while (true)
                {
                    Return();
                }
            }
            void Stats()
            {
                Console.Clear();
                string errorMsg = null;
                while (true)
                {
                    Console.Clear();
                    Console.Write("Statistics Description\n\n");

                    if (errorMsg != null)
                        Console.WriteLine($"Error: {errorMsg}\n");

                    Console.WriteLine($"Write the date from when you want to see statistics (day/month/year)");
                    String date = Console.ReadLine();

                    try
                    {
                        DateTime dt = date == "full" ? DateTime.Parse("1/1/1000") : DateTime.Parse(date);

                        var resultTable = from c in Copy.GetAll()
                                          join r in Rental.GetAll() on c.Id equals r.CopyId
                                          join m in Movie.GetAll() on c.MovieId equals m.Id
                                          where r.DateOfRental > dt
                                          select new { Price = m.Price };

                        Console.WriteLine($"\nTotal rentals: {resultTable.Count()}");
                        Console.WriteLine($"Total price of rented movies: {resultTable.Sum(obj => obj.Price)}$");
                    }
                    catch (Exception e)
                    {
                        errorMsg = e.Message;
                        continue;
                    }

                    break;
                }

                while (true)
                {
                    Return();
                }
            }

            string CropString(string str, int limit)
            {
                return str.Substring(0, str.Length < limit ? str.Length : limit - 3) + (str.Length >= limit - 3 ? "..." : "");
            }

            void Return()
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n\n\tPress enter to return to main menu...");
                ConsoleKeyInfo keyPressed = Console.ReadKey();
                if (keyPressed.Key == ConsoleKey.Enter)
                {
                    Start();
                }
            }
        }

    }

}

