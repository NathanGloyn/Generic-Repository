using System;
using System.Linq;
using Repository;
using Repository.Linq2SQL;

namespace RepositoryTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Display toRun = new Display();
            toRun.DisplayOrders();
            Console.ReadLine();

        }
    }

    class Display
    {
        public void DisplayOrders()
        {
            IRepository<Order> repo = new Repository<Order>(new RepositoryTestDataContext());

            var allOrders = repo.GetAll();

            foreach (var item in allOrders.Take(10))
                {
                    Console.WriteLine("OrderId: {0} OrderDate: {1}", item.OrderID, item.OrderDate);
                }

                Console.WriteLine();
                Console.WriteLine();

                var filteredOrders = repo.Find(o => o.OrderDate <= new DateTime(1996, 7, 4));

                foreach (var item in filteredOrders.Take(10))
                {
                    Console.WriteLine("OrderId: {0} OrderDate: {1}", item.OrderID, item.OrderDate);
                }

                Console.WriteLine();
                Console.WriteLine("Insert new order");

                Order newOrder = new Order
                              {
                                   OrderDate = DateTime.Now,
                                   CustomerID = "ALFKI",
                                   EmployeeID = null,
                                   Freight = null,
                                   RequiredDate = DateTime.Now.AddDays(10),
                                   ShipAddress = "Obere Str. 57",
                                   ShipCity = "Berlin",
                                   ShipCountry = "Germany",
                                   ShipName = "Alfreds Futterkiste",
                                   ShipPostalCode = "12209",
                                   ShipVia = 1
                              };

                repo.Insert(newOrder);

            Order updateOrder;

            IRepository<Order> repo2 = new Repository<Order>(new RepositoryTestDataContext());
            
            updateOrder = repo2.GetById(11078);


            IRepository<Order> repo3 = new Repository<Order>(new RepositoryTestDataContext());
            
            updateOrder.EmployeeID = 1;
            repo3.Update(updateOrder); 
            
                               
            
        }
    }

}
