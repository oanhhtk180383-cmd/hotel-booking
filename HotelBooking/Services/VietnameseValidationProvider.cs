using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HotelBooking.Services;
// Razor and server validation use the same Vietnamese annotation messages.
// This provider supplies messages for ASP.NET Core's implicit non-nullable strings.
public sealed class VietnameseValidationProvider : IModelValidatorProvider
{
    public void CreateValidators(ModelValidatorProviderContext context)
    {
        foreach (var item in context.Results)
        {
            if (item.ValidatorMetadata is System.ComponentModel.DataAnnotations.RequiredAttribute required && required.ErrorMessage is null)
                required.ErrorMessage = "Vui lòng nhập {0}.";
        }
    }
}
