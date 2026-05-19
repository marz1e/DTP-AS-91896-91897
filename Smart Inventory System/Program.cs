using System.Text.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;

// Represents a single ingredient stored in the system with its details
class Ingredient
{
    public string IngredientName { get; set; } = "";   // Stores the name of the ingredient
    public string ExpiryDate { get; set; } = "";       // Stores expiry date in yyyy-MM-dd format
    public decimal IngredientCost { get; set; }        // Stores cost of the ingredient
    public string Quantity { get; set; } = "";         // Stores quantity description (e.g. 1L, 500g)
}

// Represents a recipe and the ingredients required to make it
class Recipe
{
    public string RecipeName { get; set; } = "";            // Name of the recipe
    public List<string> RequiredIngredients { get; set; } = [];  // List of ingredients needed
}

class Program
{
    static void Main()
    {
        try
        {
            // Load previously saved ingredient data from file
            List<Ingredient> ingredientsList = LoadIngredients();

            // Load recipe data from external JSON file
            List<Recipe> recipes = LoadRecipes();

            // Stack used to store deleted ingredients for undo functionality
            Stack<Ingredient> undoStack = new Stack<Ingredient>();

            // Main program loop that continuously displays menu options
            while (true)
            {
                Console.WriteLine("\n=== Ingredient Manager ===");
                Console.WriteLine("1. Add Ingredient");
                Console.WriteLine("2. View Ingredients");
                Console.WriteLine("3. Calculate money loss");
                Console.WriteLine("4. Suggest Recipes");
                Console.WriteLine("5. Check Expired Ingredients");
                Console.WriteLine("6. Remove Ingredient");
                Console.WriteLine("7. Edit Ingredient");
                Console.WriteLine("8. Undo Delete");
                Console.WriteLine("9. Exit (Ingredients will save)");
                Console.Write("Choose an option: ");

                // Reads user menu selection and removes extra spaces
                string choice = (Console.ReadLine() ?? "").Trim();

                switch (choice)
                {
                    case "1":
                        AddIngredient(ingredientsList);   // Adds a new ingredient to the system
                        SaveIngredients(ingredientsList); // Saves updated list to file
                        break;

                    case "2":
                        ShowIngredients(ingredientsList); // Displays all stored ingredients
                        break;

                    case "3":
                        CalculateLoss(ingredientsList);   // Calculates cost of expired ingredients
                        break;

                    case "4":
                        SuggestRecipes(ingredientsList, recipes); // Suggests recipes based on available ingredients
                        break;

                    case "5":
                        CheckExpiredIngredients(ingredientsList); // Displays all expired ingredients
                        break;

                    case "6":
                        RemoveIngredient(ingredientsList, undoStack); // Removes ingredient and stores it for undo
                        SaveIngredients(ingredientsList);             // Saves updated list after deletion
                        break;

                    case "7":
                        EditIngredient(ingredientsList); // Allows editing of ingredient details
                        SaveIngredients(ingredientsList); // Saves changes after editing
                        break;

                    case "8":
                        UndoDelete(ingredientsList, undoStack); // Restores last deleted ingredient
                        break;

                    case "9":
                        // Saves data before exiting to ensure no data loss
                        Console.WriteLine("Thank you for using this program!");
                        SaveIngredients(ingredientsList);
                        return;

                    default:
                        // Handles invalid menu selections outside valid range
                        Console.WriteLine("Invalid option. Please enter numbers 1-9");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            // Global error handler to prevent program crash and display error message
            Console.WriteLine("\nA fatal error has occurred.");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("The program will now exit safely.");
        }
    }

    // Reads a required string input and ensures it is not empty or too long
    static string ReadRequiredString(string prompt, int maxLength = 50)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = (Console.ReadLine() ?? "").Trim();

            // Validates that input is not empty or just whitespace
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Input cannot be empty.");
                continue;
            }

            // Ensures input does not exceed maximum allowed length
            if (input.Length > maxLength)
            {
                Console.WriteLine($"Input too long (max {maxLength}).");
                continue;
            }

            return input;
        }
    }

    // Prompts user for a yes/no confirmation and validates response input
    static bool Confirm(string message)
    {
        while (true)
        {
            Console.WriteLine($"{message} (y/n):");
            string input = (Console.ReadLine() ?? "").Trim().ToLower();

            // Accepts only valid confirmation responses
            if (input == "y") return true;
            if (input == "n") return false;

            Console.WriteLine("Please enter y or n");
        }
    }

    // Reads and validates a decimal number ensuring it is not negative
    static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = (Console.ReadLine() ?? "").Trim();

            // Ensures input is a valid decimal and within acceptable range (>= 0)
            if (decimal.TryParse(input, out decimal value) && value >= 0)
                return value;

            Console.WriteLine("Invalid number");
        }
    }

    // Reads and validates a date in strict yyyy-MM-dd format
    static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = (Console.ReadLine() ?? "").Trim();

            // Ensures correct date format before accepting input
            if (DateTime.TryParseExact(input, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None,
                out DateTime date))
            {
                return date;
            }

            Console.WriteLine("Invalid date format (yyyy-MM-dd).");
        }
    }

    // Updates and validates a string field such as name or quantity
static string UpdateString(string label, string current, int maxLength, string pattern)
{
    while (true)
    {
        Console.WriteLine($"Current {label}: {current}");
        Console.Write($"Enter new {label}: ");

        string input = (Console.ReadLine() ?? "").Trim();

        // Ensures input is not empty
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine($"{label} cannot be empty.");
            continue;
        }

        // Ensures input does not exceed maximum allowed length
        if (input.Length > maxLength)
        {
            Console.WriteLine($"{label} too long.");
            continue;
        }

        // Validates input against required pattern
        if (!Regex.IsMatch(input, pattern))
        {
            Console.WriteLine("Invalid format.");
            continue;
        }

        // Confirms update before saving changes
        if (!Confirm($"Change {label} from '{current}' to '{input}'?"))
            return current;

        return input;
    }
}

// Updates and validates a decimal value such as ingredient cost
static decimal UpdateDecimal(string label, decimal current)
{
    while (true)
    {
        Console.WriteLine($"Current {label}: {current:C}");
        Console.Write($"Enter new {label}: ");

        string input = (Console.ReadLine() ?? "").Trim();

        // Ensures input is a valid positive decimal number
        if (!decimal.TryParse(input, out decimal value) || value < 0)
        {
            Console.WriteLine("Invalid number.");
            continue;
        }

        // Confirms update before saving changes
        if (!Confirm($"Change {label} from {current:C} to {value:C}?"))
            return current;

        return value;
    }
}

// Updates and validates a date field using yyyy-MM-dd format
static string UpdateDate(string label, string current)
{
    while (true)
    {
        Console.WriteLine($"Current {label}: {current}");
        Console.Write($"Enter new {label} (yyyy-MM-dd): ");

        string input = (Console.ReadLine() ?? "").Trim();

        // Ensures date follows required format
        if (!DateTime.TryParseExact(
            input,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _))
        {
            Console.WriteLine("Invalid date.");
            continue;
        }

        // Confirms update before saving changes
        if (!Confirm($"Change {label} from '{current}' to '{input}'?"))
            return current;

        return input;
    }
}

    // Adds a new ingredient after validating uniqueness and input format
    static void AddIngredient(List<Ingredient> list)
    {
        do
        {
            // Reads ingredient name and ensures it is valid text input
            string name = ReadRequiredString("Enter ingredient name: ");

            // Checks that ingredient does not already exist in list
            if (list.Any(i => i.IngredientName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Ingredient already exists.");
                continue;
            }

            // Ensures name only contains valid characters (letters, spaces, hyphens)
            if (!Regex.IsMatch(name, @"^[a-zA-Z\s\-]+$"))
            {
                Console.WriteLine("Invalid characters.");
                continue;
            }

            // Collects validated ingredient details from user
            DateTime expiry = ReadDate("Enter expiry (yyyy-MM-dd): ");

    // Check if the ingredient is already expired at the time of entry
    if (expiry < DateTime.Today)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("WARNING: This ingredient is already expired!");
        Console.ResetColor();

    // Ask user if they want to continue anyway
        if (!Confirm("Do you still want to add this ingredient?"))
        {
            Console.WriteLine("Ingredient not added. Restarting input...");
            continue; // goes back to start of AddIngredient loop
        }
    }
            decimal cost = ReadDecimal("Enter cost: ");
            string quantity = ReadRequiredString("Enter quantity: ", 20);

            // Adds validated ingredient object to list
            list.Add(new Ingredient
            {
                IngredientName = name.ToLower(),
                ExpiryDate = expiry.ToString("yyyy-MM-dd"),
                IngredientCost = cost,
                Quantity = quantity
            });

        } while (Confirm("Add another ingredient?")); // Repeats process if user confirms
    }

    // Displays all ingredients that have passed their expiry date
    static void CheckExpiredIngredients(List<Ingredient> ingredients)
    {
        Console.WriteLine("Expired Ingredient Alerts");

        bool foundExpired = false;

        foreach (var item in ingredients)
        {
            // Converts stored expiry string into DateTime for comparison
            if (DateTime.TryParse(item.ExpiryDate, out DateTime expiry))
            {
                // Checks whether ingredient is expired compared to today's date
                if (expiry < DateTime.Today)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{item.IngredientName} expired on {item.ExpiryDate}");
                    Console.ResetColor();

                    foundExpired = true;
                }
            }
        }

        // Displays message if no expired ingredients are found
        if (!foundExpired)
        {
            Console.WriteLine("No expired ingredients.");
        }
    }

    // Loads recipe data from JSON file with error handling
    static List<Recipe> LoadRecipes()
    {
        try
        {
            string path = "recipes.json";

            // Checks if file exists before attempting to read
            if (!File.Exists(path))
            {
                Console.WriteLine("Recipe file does not found.");
                return new List<Recipe>();
            }

            // Reads JSON content from file
            string json = File.ReadAllText(path);

            // Converts JSON into list of Recipe objects
            return JsonSerializer.Deserialize<List<Recipe>>(json) ?? new List<Recipe>();
        }
        catch (JsonException)
        {
            // Handles corrupted or invalid JSON format
            Console.WriteLine("Recipe file is corrupted.");
            return new List<Recipe>();
        }
        catch (Exception ex)
        {
            // Handles unexpected runtime errors during file loading
            Console.WriteLine($"Unexpected error loading recipes: {ex.Message}");
            return new List<Recipe>();
        }
    }

    // Suggests recipes based on available (non-expired) ingredients
    static void SuggestRecipes(List<Ingredient> ingredients, List<Recipe> recipes)
    {
        Console.WriteLine("\n=== Recipe Suggestions ===");

        // Filters out expired ingredients and normalises names
        List<string> userIngredients = ingredients
            .Where(i =>
            {
                if (DateTime.TryParse(i.ExpiryDate, out DateTime expiry))
                {
                    return expiry >= DateTime.Today;
                }
                return false;
            })
            .Select(i => i.IngredientName.ToLower())
            .ToList();

        bool foundAny = false;

        // Orders recipes based on how many ingredients the user has
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

            // Compares required ingredients against available ingredients
            foreach (var required in recipe.RequiredIngredients)
            {
                if (userIngredients.Contains(required.Trim().ToLower()))
                {
                    matchCount++;
                }
                else
                {
                    missingIngredients.Add(required);
                }
            }

            // Calculates percentage match for recipe completion
            double percentage =
                (double)matchCount / recipe.RequiredIngredients.Count * 100;

            if (matchCount > 0)
            {
                foundAny = true;

                Console.WriteLine($"\nRecipe: {recipe.RecipeName}");
                Console.WriteLine($"Match: {percentage:F0}%");

                // Displays missing ingredients if recipe is incomplete
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

        // Displays message if no matching recipes are found
        if (!foundAny)
        {
            Console.WriteLine("No recipe match found.");
        }
    }

    // Calculates total cost of ingredients that have expired
    static void CalculateLoss(List<Ingredient> ingredients)
    {
        decimal totalLoss = 0;
        DateTime today = DateTime.Today;

        foreach (var item in ingredients)
        {
            // Converts expiry string into DateTime for comparison
            if (DateTime.TryParse(item.ExpiryDate, out DateTime expiry))
            {
                // Adds cost if ingredient has expired
                if (expiry < today)
                {
                    totalLoss += item.IngredientCost;
                }
            }
        }

        Console.WriteLine($"\nTotal loss from expired ingredients: {totalLoss:C}");
    }

    // Removes an ingredient and stores it for undo functionality
    static void RemoveIngredient(List<Ingredient> list, Stack<Ingredient> undoStack)
    {
        string name = ReadRequiredString("Enter ingredient name: ").ToLower();

        // Searches for ingredient ignoring case sensitivity
        var found = list.FirstOrDefault(i =>
            i.IngredientName.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (found == null)
        {
            Console.WriteLine("Ingredient not found");
            return;
        }

        // Confirms deletion before removing item
        if (!Confirm($"Delete {found.IngredientName}"))
            return;

        // Stores item for undo functionality before deletion
        undoStack.Push(found);
        list.Remove(found);

        Console.WriteLine("Ingredient removed. You can undo this.");
    }

    // Restores the most recently deleted ingredient
    static void UndoDelete(List<Ingredient> list, Stack<Ingredient> undoStack)
    {
        if (undoStack.Count == 0)
        {
            Console.WriteLine("Nothing to undo.");
            return;
        }

        var item = undoStack.Pop();
        list.Add(item);

        Console.WriteLine($"Restored {item.IngredientName}");
    }

    // Allows user to modify existing ingredient data fields
    static void EditIngredient(List<Ingredient> ingredients)
    {
        string name = ReadRequiredString("Enter the name of the ingredient you want to edit: ").ToLower();

        var found = ingredients.FirstOrDefault(i =>
            i.IngredientName.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (found == null)
        {
            Console.WriteLine("Ingredient not found");
            return;
        }

        Console.WriteLine("\nSelect field to edit:");
        Console.WriteLine("1. Name");
        Console.WriteLine("2. Expiry date");
        Console.WriteLine("3. Cost");
        Console.WriteLine("4. Quantity");

        switch (Console.ReadLine())
        {
            case "1":
                found.IngredientName = UpdateString("name", found.IngredientName, 50,
                    @"^[a-zA-Z\s\-]+$").ToLower();
                break;

            case "2":
                found.ExpiryDate = UpdateDate("expiry", found.ExpiryDate);
                break;

            case "3":
                found.IngredientCost = UpdateDecimal("cost", found.IngredientCost);
                break;

            case "4":
                found.Quantity = UpdateString("quantity", found.Quantity, 20, @"^[a-zA-Z\s\-]+$");
                break;

            default:
                Console.WriteLine("Invalid option selected.");
                break;
        }
    }

    // Displays all stored ingredients in a readable format
    static void ShowIngredients(List<Ingredient> IngredientList)
    {
        Console.WriteLine("\nIngredients List:");

        if (IngredientList.Count == 0)
        {
            Console.WriteLine("No ingredients stored.");
            return;
        }

        foreach (var item in IngredientList)
        {
            Console.WriteLine(
                $"Name: {item.IngredientName} | " +
                $"Expiry: {item.ExpiryDate} | " +
                $"Cost: ${item.IngredientCost} |" +
                $"Quantity: {item.Quantity}"
            );
        }
    }

    // Loads ingredient data from saved JSON file with error handling
    static List<Ingredient> LoadIngredients()
    {
        try
        {
            string path = "IngredientsSaved.json";

            if (!File.Exists(path))
                return new List<Ingredient>();

            string json = File.ReadAllText(path);

            return JsonSerializer.Deserialize<List<Ingredient>>(json)
                ?? new List<Ingredient>();
        }
        catch (JsonException)
        {
            Console.WriteLine("Error: Ingredient file is corrupted. Starting fresh.");
            return new List<Ingredient>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error loading ingredients: {ex.Message}");
            return new List<Ingredient>();
        }
    }

    // Saves current ingredient list to JSON file
    static void SaveIngredients(List<Ingredient> ingredients)
    {
        try
        {
            File.WriteAllText(
                "IngredientsSaved.json",
                JsonSerializer.Serialize(ingredients, new JsonSerializerOptions
                {
                    WriteIndented = true
                })
            );
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Error: No permission to save the file.");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"File save error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }

        Console.WriteLine("Ingredients saved successfully!");
    }
}