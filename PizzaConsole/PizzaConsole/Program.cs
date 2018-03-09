using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PizzaConsole
{
  class Program
  {
    static void Main(string[] args)
    {
      List<PizzaModel> pizzaList = Top20Pizzas();
      int i = 1;
      foreach (var topping in pizzaList) {
        Console.WriteLine(i + ". " + topping.Toppings + ": " + topping.Count);
        i++;
      }
      Console.ReadLine();
    }
    private static List<PizzaModel> Top20Pizzas()
    {
      //Get pizza data from json and parse
      string json = GetJsonData("http://files.olo.com/pizzas.json");
      JArray pizzaJson = JArray.Parse(json) as JArray;

      //Create pizza list from json
      List<PizzaModel> pizzaList = new List<PizzaModel>();
      foreach (dynamic item in pizzaJson)
      {
        pizzaList.Add(new PizzaModel(item.toppings.ToObject<string[]>()));
      }

      //Summarize pizza list by toppings, with count
      var sumToppings = from topping in pizzaList
                        group topping by topping.Toppings into toppingGroup
                        select new
                        {
                          Toppings = toppingGroup.Key,
                          Count = toppingGroup.Count()
                        };

      //Get and return top 20 most ordered pizza toppings
      int counter = 1;
      var orderedToppings = (from topping in sumToppings
                             orderby topping.Count descending
                             select new PizzaModel { Rank = counter++, Toppings = topping.Toppings, Count = topping.Count }).Take(20);

      return orderedToppings.ToList();

    }


    public static string GetJsonData(string url)
    {
      Uri uri = new Uri(url);
      HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
      request.Method = WebRequestMethods.Http.Get;
      HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      StreamReader reader = new StreamReader(response.GetResponseStream());
      string json = reader.ReadToEnd();
      response.Close();

      return json;
    }

  }

  public class PizzaModel
  {
    public string Toppings { get; set; }
    public int Rank { get; set; }
    public int Count { get; set; }

    public PizzaModel() { }

    public PizzaModel(string[] toppings)
    {
      //Data assumptions:
      //  - order of toppings does not matter (cheese,pepperoni is the same as pepperoni,cheese)
      //  - data is all lower case
      //  - don't ignore duplicate toppings as it could mean extra (cheese,pepperoni is not the same as cheese,pepperoni,pepperoni)

      //Sort toppings first to ensure order in raw data is consistent.
      Array.Sort(toppings);

      //Join array elements into a string.
      Toppings = string.Join(",", toppings);
    }

  }
}
