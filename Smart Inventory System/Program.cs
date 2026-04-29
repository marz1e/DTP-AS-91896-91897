using System.Text.Json.Nodes;
using System.Text.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;


class Ingredient

{
    public string IngredientName {get;set;}
    public string ExpiryDate {get;set;}
}

class Program
{
    static void Main()
    {
        List <Ingredient> IngredientsList = new List<Ingredient>();

        while (true)
        {
            Console.WriteLine("\n== Ingredient Manager ===");
            Console.WriteLine("1. Add Ingredient");
            Console.WriteLine("2. View Ingredients");
            Console.WriteLine("3. Save to File");
            Console.WriteLine("4. Exit");
            Console.Write("Choose an option: ");

            string choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    AddIngredient(IngredientsList);
                    break;

                case "2":
                    ShowIngredients(IngredientsList);
                    break;

                case "3":
                    SaveToFile(IngredientsList);
                    break;

                case "4":
                    Console.WriteLine("Thank you for using this program!");
                    return;

                default:
                    Console.WriteLine("Invalid option. Please enter numbers 1-4");
                    break;
                
            }
        }
    }
    static void AddIngredient(List<Ingredient> IngredientsList)
    {
        string input;

        do
        {
            // Create a new ingredient object for this loop iteration.
            Ingredient FullIngredient = new Ingredient(); // object creation


            // Prompt user for the ingredient name, then read it from console.
             // prompt for name
            while (true)
                {
                    Console.Write("Enter ingredient name. Please enter only characters and hyphens: ");
                    string name = Console.ReadLine();


                    if (string.IsNullOrWhiteSpace(name))
                    {
                        Console.WriteLine("Ingredient name cannot be empty.");
                        continue;
                    }
                    name = name.Trim();


                    if (name.Length > 50)
                    {
                        Console.WriteLine("Name is too long. Max 50 characters.");
                        continue;
                    }
                    if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z\s\-]+$"))
                    {
                        Console.WriteLine("Only letters, spaces, and hyphens are allowed!");
                        continue;
                       
                    }


                    // confirmation step
                    Console.Write($"You entered '{name}'. Is this correct? (y/n): ");
                    string confirm = Console.ReadLine()?.ToLower();


                    if (confirm == "y")
                {
                     FullIngredient.IngredientName = name;
                    break;
                }
               
                }


            // Prompt user for expiry date, then read it from console // prompt for expiry
            DateTime expiryDate;
            while (true)
            {
                  while (true)
            {
                Console.Write("Enter expiry date (yyyy-MM-dd): ");
                string NewInput = Console.ReadLine();


            if (DateTime.TryParseExact(
                NewInput,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out expiryDate))
            {
                break;
            }


            Console.WriteLine("Invalid format. Please use yyyy-MM-dd");
        }


        //confirmation step
        Console.Write($"You entered '{expiryDate:yyyy-MM-dd}. Is this correct? (y/n): ");
        string confirm_2 = Console.ReadLine()?.ToLower();


        if (confirm_2 == "y")
            {
                FullIngredient.ExpiryDate = expiryDate.ToString("yyyy-MM-dd");
                break;
            }


            }
          
            IngredientsList.Add(FullIngredient);


            // Ask if the user wants to continue adding ingredients.
            Console.Write("Do you want to add another ingredient? (y/n) "); // continuation prompt
            input = Console.ReadLine()?.ToLower(); // normalize response


        } while (input == "y"); // repeat while user enters y

        }

    
    static void SaveToFile(List<Ingredient> IngredientsList)
        {
            string json = JsonSerializer.Serialize(IngredientsList, new JsonSerializerOptions { WriteIndented = true});

            string path = "IngredientsSaved.json";

            File.WriteAllText(path,json);

            Console.WriteLine("Saved to file.");

        }
    static void ShowIngredients(List<Ingredient> IngredientList)
        {
            Console.WriteLine("\nIngredients List:");
            // add something that will tell the user that its empty
            foreach (var item in IngredientList)
            {
                Console.WriteLine($"Name: {item.IngredientName}, Expiry: {item.ExpiryDate}");
            }
        }
    
    }

