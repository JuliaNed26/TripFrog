using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ModelStateDictionary = Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary;

namespace TripFrogWebApi.Controllers;

public static class ModelValidator
{
    public static bool IsModelValid(this ControllerBase controller, object modelToValidate, out ModelStateDictionary modelsState)
    {
        var validationContext = new ValidationContext(modelToValidate, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(modelToValidate, validationContext, validationResults, validateAllProperties: true);

        if (!isValid)
        {
            foreach (var validationResult in validationResults)
            {
                if (validationResult.ErrorMessage != null)
                {
                    controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
                }
            }

            modelsState = controller.ModelState;
            return false;
        }

        modelsState = new ModelStateDictionary();
        return true;
    }
}
