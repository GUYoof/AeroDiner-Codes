using System.Collections.Generic;

public class RecipePreviewResult
{
    public string BestMatchedRecipeText;
    public FoodData CookedIngredient;
    public List<string> AvailableFoodIds;

    public static RecipePreviewResult None(string message)
    {
        return new RecipePreviewResult
        {
            BestMatchedRecipeText = message,
            CookedIngredient = null,
            AvailableFoodIds = new List<string>()
        };
    }
}
