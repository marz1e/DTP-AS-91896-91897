using System.Text.Json.Nodes;
using System.Text.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

class Ingredient
{
    public string IngredientName {get;set;}
    public string ExpiryDate {get; set;}
}
//class recipe

class Program
{
    static void Main()
    {
        // Create a dynamic list to store ingredients; length is not known yet.
        List<Ingredient> IngredientsList = new List<Ingredient>(); // list initialization

        // Temporary variable to store user's continuation input.
        string input; // input placeholder for y/n loop control

        // Begin loop to gather ingredient entries until user opts out.
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
                
                    FullIngredient.IngredientName = name;
                    break;
                
                }

            // Prompt user for expiry date, then read it from console.
            Ingredient ExpiryDateObject = new Ingredient();
            
            Console.Write("Enter expiry date (e.g. 2026-04-01): "); // prompt for expiry
            DateTime expiryDate;
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
            FullIngredient.ExpiryDate = expiryDate.ToString("yyyy-MM-dd");

            IngredientsList.Add(FullIngredient);

            // Ask if the user wants to continue adding ingredients.
            Console.Write("Do you want to add another ingredient? (y/n) "); // continuation prompt
            input = Console.ReadLine()?.ToLower(); // normalize response

        } while (input == "y"); // repeat while user enters y

        // After input collection is complete, print a header line.
        Console.WriteLine("\nIngredients List:"); // section header

        // Loop through each saved ingredient and print its properties.
        foreach (var item in IngredientsList) // iterate all entries
        {
            // Print one ingredient per line with both fields.
            Console.WriteLine($"Name: {item.IngredientName}, Expiry: {item.ExpiryDate}"); // display entry
        }
        string JsonSaved = JsonSerializer.Serialize(IngredientsList);

        string FilePath = Path.Combine(AppContext.BaseDirectory, "IngredientsSaved.json");

        File.WriteAllText(FilePath, JsonSaved);
    }
}