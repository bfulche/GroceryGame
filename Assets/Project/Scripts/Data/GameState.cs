namespace GroceryGame.Core
{
    // Represents the different high-level states of the game
    public enum GameState
    {
        MainMenu,        // At the main menu
        RecipeSelection, // Choosing a recipe
        StoreSelection,  // Choosing a store to shop at
        Shopping,        // In the store shopping
        Checkout,        // At the checkout counter
        RecipeSuccess,   // Completed shopping successfully
        RecipeFailed     // Failed to get all ingredients
    }
}