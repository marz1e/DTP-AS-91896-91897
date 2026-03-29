using System.Text.Json.Nodes;
using System.Text.Json;
using System.IO;

class Ingredient
{
    public string IngredientName {get;set;}
    public string ExpiryDate {get; set;}
}

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
            Ingredient IngredientObject = new Ingredient(); // object creation

            // Prompt user for the ingredient name, then read it from console.
            Console.Write("Enter ingredient name. "); // prompt for name
            IngredientObject.IngredientName = Console.ReadLine(); // set name field

            // Prompt user for expiry date, then read it from console.
            Console.Write("Enter expiry date (e.g. 2026-04-01): "); // prompt for expiry
            IngredientObject.ExpiryDate = Console.ReadLine(); // set expiry field

            // Add this constructed ingredient to the in-memory list.
            IngredientsList.Add(IngredientObject); // append to list

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
        string json = JsonSerializer.Serialize(IngredientsList);

        string filepath = Path.Combine(AppContext.BaseDirectory, "ingredients.json");

        File.WriteAllText(filepath, json);

        
        
    
    
    
    
    
    }
    
    


}