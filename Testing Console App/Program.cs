using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Testing_Console_App
{
    class Program
    {
        static void Main(string[] args)
        {
            MySQL_Handler.Configuration.MySqlServer = "127.0.0.1";
            MySQL_Handler.Configuration.MySqlPort = 3306;
            MySQL_Handler.Configuration.MySqlDatabase = "testdb";
            MySQL_Handler.Configuration.MySqlUsername = "testdbu";
            MySQL_Handler.Configuration.MySqlPassword = "testdbp";

            Random random = new Random();

            for (int i = 0; i < 50; i++)
            {
                int age = random.Next(1, 101);
                Person eng = new Person("Jane Doe", (age > 50) ? null : (int?)age);
                eng.Sync();
            }

            List<Person> people = Person.SelectAll(new MySQL_Handler.SelectLimiter(10));

            foreach (Person person in people)
            {
                Console.WriteLine($"Person: {person.ID} : {person.Name} : {person.Age}");
            }

            Console.ReadLine();

            Person singlePerson = Person.SelectSingleByPrimary(3);

            Console.WriteLine($"Person: {singlePerson.ID} : {singlePerson.Name} : {singlePerson.Age}");

            Console.ReadLine();
        }
    }
}
