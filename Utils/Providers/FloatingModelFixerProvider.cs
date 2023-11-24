using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace IsometricShooterWebApp.Utils.Providers
{
    public class CustomBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(double))
            {
                return new FloatingModelFixerProvider();
            }

            return null;
        }
    }

    public class FloatingModelFixerProvider : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == null)
            {
                return Task.CompletedTask;
            }

            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            // Remove unnecessary commas and spaces
            value = value.Replace(",", string.Empty).Trim();

            double myValue = 0;
            if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out myValue))
            {
                // Error
                bindingContext.ModelState.TryAddModelError(
                                        bindingContext.ModelName,
                                        "Could not parse MyValue.");
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(myValue);
            return Task.CompletedTask;
        }
    }
}
