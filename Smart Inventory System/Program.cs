using System.Text.Json.Nodes;
using System.Text.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;


class Ingredient

{
    public string IngredientName {get;set;}
    public string ExpiryDate {get;set;}
    public decimal IngredientCost { get; set;}
}

class Recipe
{
    public string RecipeName {  get; set;}
    public List<string> RequiredIngredients {get; set;}
}


class Program
{
    static void Main()
    {
        List <Ingredient> IngredientsList = new List<Ingredient>();
        List<Recipe> recipes = LoadRecipes();

        while (true)
        {
            Console.WriteLine("\n== Ingredient Manager ===");
            Console.WriteLine("1. Add Ingredient");
            Console.WriteLine("2. View Ingredients");
            Console.WriteLine("3. Save to File");
            Console.WriteLine("4. Calculate money loss");
            Console.WriteLine("5. Suggest Recipes");
            Console.WriteLine("6. Exit");
            Console.Write("Choose an option: ");

            string choice = Console.ReadLine().Trim();
            

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
                    CalculateLoss(IngredientsList);
                    break;
                    

                case "5":
                    SuggestRecipes(IngredientsList, recipes);
                    break;

                case "6":
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
                        name.Trim().ToLower();
                    break;
                }
                else
                {
                    continue;
                }
                }


            // Prompt user for expiry date, then read it from console // prompt for expiry
            DateTime expiryDate;
            while (true)
            {
                Console.Write("Enter expiry date (yyyy-MM-dd): ");
                string NewInput = Console.ReadLine();
                
                if (!DateTime.TryParseExact(
                NewInput,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out expiryDate))
                    {
                        Console.WriteLine("Invalid format. Please use yyyy-MM-dd");
                        continue;
                    }

                    Console.Write($"You entered '{expiryDate:yyyy-MM-dd}. Is this correct? (y/n): ");
                    string confirm_2 = Console.ReadLine()?.ToLower();

                    if (confirm_2 == "y")
                    {
                        FullIngredient.ExpiryDate = expiryDate.ToString("yyyy-MM-dd");
                        break;
                    }

                }
        
        decimal ingredientcost;

        while (true)
                {
                    Console.Write("Enter the cost of your ingredient. Please enter a number: ");
                    string? inputCost = Console.ReadLine();

                    if (!decimal.TryParse(inputCost,out ingredientcost) || ingredientcost < 0)
                {
                    Console.WriteLine("Invalid number. Try again");
                    continue;
                }
                Console.Write($"You entered '{ingredientcost:C}'. Is this correct? (y/n)");
                    string confirm = (Console.ReadLine() ?? "").ToLower();

                    if (confirm == "y")
                    {
                        FullIngredient.IngredientCost = ingredientcost;
                        break;
                    }
                }
          
            IngredientsList.Add(FullIngredient);


            // Ask if the user wants to continue adding ingredients.
            while (true)
            {
                Console.Write("Do you want to add another ingredient? (y/n) "); // continuation prompt
                input = (Console.ReadLine()?? "").ToLower().Trim(); // normalize response

            if (input == "y" || input == "n")
                break;
            Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
            }

        } while (input == "y"); // repeat while user enters y
        

        }

    static List<Recipe> LoadRecipes()
    {
        string path = "recipes.json";
        

        if (!File.Exists(path))
        {
            Console.WriteLine("Recipe file does not found.");
            return new List<Recipe>();
        }
        string json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<List<Recipe>>(json)
                ?? new List<Recipe>();
    }

    static void SuggestRecipes(List<Ingredient> ingredients, List<Recipe> recipes)
    {
        Console.WriteLine("\n=== Recipe Suggestions ===");

        List <string> userIngredients = ingredients
            .Select(i => i.IngredientName.ToLower())
            .ToList();

        bool foundAny = false;


        var sortedRecipes = recipes
        .OrderByDescending(recipe =>
            (double)recipe.RequiredIngredients.Count(required => 
                userIngredients.Contains(required.Trim().ToLower())
                )
            )
            .ToList();

        foreach (var recipe in sortedRecipes)
        {
            int matchCount = 0;

            List<string> missingIngredients = new List<string>();
            foreach (var required in recipe.RequiredIngredients)
            {
                if (userIngredients.Contains(required.ToLower()))
                {
                    matchCount ++;
                }
                else
                {
                    missingIngredients.Add(required);
                }
        
            }
        double percentage =
            (double)matchCount / recipe.RequiredIngredients.Count *100;

            if (matchCount > 0)
            {
                foundAny = true;

                Console.WriteLine($"\nRecipe: {recipe.RecipeName}");
                Console.WriteLine($"Match: {percentage:F0}%");

                if (missingIngredients.Count > 0)
                {
                    Console.WriteLine("Missing ingredients:");

                    foreach (var item in missingIngredients)
                    {
                        Console.WriteLine($"- {item}");
                    }
                }
                else
                {
                    Console.WriteLine("You have everything needed!");
                }
            }
        }
        if (!foundAny)
        {
            Console.WriteLine("No recipe match found.");
        }
    }
    

    static void CalculateLoss(List<Ingredient> ingredients)
    {
        decimal totalLoss = 0;
        DateTime today = DateTime.Today;

        foreach (var item in ingredients)
        {
            if (DateTime.TryParse(item.ExpiryDate, out DateTime expiry))
            {
                if (expiry < today)
                {
                    totalLoss += item.IngredientCost;
                }
            }
        }
        Console.WriteLine($"\nTotal loss from expired ingredients: {totalLoss:C}");
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
                Console.WriteLine($"Name: {item.IngredientName}, Expiry: {item.ExpiryDate}, Cost: {item.IngredientCost + "$"}");
            }
        }
    
    }

