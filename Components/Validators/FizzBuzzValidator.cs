using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using BlazorFizzBuzz.Models;

namespace BlazorFizzBuzz.Components.Validators
{
    public class FizzBuzzValidator : ComponentBase
    {
        private ValidationMessageStore validationMessageStore;

        [CascadingParameter]
        private EditContext CurrentEditContext { get; set; }

        protected override void OnInitialized()
        {
            if (CurrentEditContext == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(FizzBuzzValidator)} requires a cascading parameter of type {nameof(EditContext)}"
                    + $". For example, you can use {nameof(FizzBuzzValidator)} inside an {nameof(EditForm)}."
                );
            }

            validationMessageStore = new ValidationMessageStore(CurrentEditContext);

            //Attach methods to validation events.
            CurrentEditContext.OnFieldChanged +=
                (sender, fieldChangedEventArgs) => ValidateField(fieldChangedEventArgs.FieldIdentifier);
            CurrentEditContext.OnValidationRequested +=
                (sender, eventArgs) => ValidateAllFields();
        }

        private void ValidateField(FieldIdentifier fieldIdentifier)
        {
            var fizzBuzz = CurrentEditContext.Model as FizzBuzz ??
                throw new InvalidOperationException(
                    $"{nameof(FizzBuzzValidator)} requires a Model of type {nameof(FizzBuzz)}"
                );

            //Clear previous errors for the field
            validationMessageStore.Clear(fieldIdentifier);

            //Validate the field
            if (fieldIdentifier.FieldName == nameof(FizzBuzz.FizzValue))
            {
                if (fizzBuzz.FizzValue >= fizzBuzz.BuzzValue)
                {
                    validationMessageStore.Add(fieldIdentifier,
                        "The Fizz Value must be less than the Buzz Value.");
                }
            }
            else if (fieldIdentifier.FieldName == nameof(FizzBuzz.BuzzValue))
            {
                if (fizzBuzz.BuzzValue <= fizzBuzz.FizzValue)
                {
                    validationMessageStore.Add(fieldIdentifier,
                        "The Buzz Value must be greater than the Fizz Value.");
                }
            }
            else if (fieldIdentifier.FieldName == nameof(FizzBuzz.StopValue))
            {
                int minStopValue = fizzBuzz.FizzValue * fizzBuzz.BuzzValue;
                if (fizzBuzz.StopValue < minStopValue)
                {
                    validationMessageStore.Add(fieldIdentifier,
                        $"The Stop Value must be at least {minStopValue} "
                        + $"(i.e. the product of the Fizz and Buzz Values)."
                    );
                }
            }
        }

        private void ValidateAllFields()
        {
            var fizzBuzz = CurrentEditContext.Model as FizzBuzz ??
                throw new InvalidOperationException(
                    $"{nameof(FizzBuzzValidator)} requires a Model of type {nameof(FizzBuzz)}"
                );

            //Clear all validation errors.
            validationMessageStore.Clear();

            //Validate all the fields.
            ValidateField(new FieldIdentifier(fizzBuzz, nameof(FizzBuzz.FizzValue)));
            ValidateField(new FieldIdentifier(fizzBuzz, nameof(FizzBuzz.BuzzValue)));
            ValidateField(new FieldIdentifier(fizzBuzz, nameof(FizzBuzz.StopValue)));

            //Notify the edit context of the validation state change
            CurrentEditContext.NotifyValidationStateChanged();
        }
    }
}
