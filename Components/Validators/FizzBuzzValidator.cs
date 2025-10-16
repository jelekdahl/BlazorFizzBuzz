using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using BlazorFizzBuzz.Models;
using System.Text.RegularExpressions;

namespace BlazorFizzBuzz.Components.Validators
{
    public class FizzBuzzValidator : ComponentBase
    {
        private const string Message_Fizz_WRT_Buzz = "Fizz Value must be less than Buzz Value.";
        private const string Message_Buzz_WRT_Fizz = "Buzz Value must be greater than Fizz Value.";
        private const string Message_Fizz_WRT_Product = "Product of Fizz and Buzz Values (_) cannot exceed Stop Value.";
        private const string Message_Buzz_WRT_Product = "Product of Fizz and Buzz Values (_) cannot exceed Stop Value.";
        private const string Message_Stop_WRT_Product = "Stop Value must be at least _ (i.e. product of Fizz and Buzz Values).";
        private const string Number_Place_Marker = "_";
        private const string Number_Matcher = @"\b(?:\d{1,3}(?:,\d{3})+|\d+)\b";

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
            var fizzBuzz = GetModel();

            //Clear previous errors for the field
            validationMessageStore.Clear(fieldIdentifier);

            //Validate the field
            if (fieldIdentifier.FieldName == nameof(FizzBuzz.FizzValue))
            {
                if (fizzBuzz.FizzValue >= fizzBuzz.BuzzValue) AddMessage_Fizz_WRT_Buzz();
                else RemoveMessage_Buzz_WRT_Fizz();

                if (fizzBuzz.FizzValue * fizzBuzz.BuzzValue > fizzBuzz.StopValue)
                {
                    AddMessage_Fizz_WRT_Product(fizzBuzz);
                }

                RemoveMessage_Buzz_WRT_Product();
                RemoveMessage_Stop_WRT_Product();
            }
            else if (fieldIdentifier.FieldName == nameof(FizzBuzz.BuzzValue))
            {
                if (fizzBuzz.BuzzValue <= fizzBuzz.FizzValue) AddMessage_Buzz_WRT_Fizz();
                else RemoveMessage_Fizz_WRT_Buzz();

                if (fizzBuzz.FizzValue * fizzBuzz.BuzzValue > fizzBuzz.StopValue)
                {
                    AddMessage_Buzz_WRT_Product(fizzBuzz);
                }

                RemoveMessage_Fizz_WRT_Product();
                RemoveMessage_Stop_WRT_Product();
            }
            else if (fieldIdentifier.FieldName == nameof(FizzBuzz.StopValue))
            {
                if (fizzBuzz.StopValue < fizzBuzz.FizzValue * fizzBuzz.BuzzValue)
                {
                    AddMessage_Stop_WRT_Product(fizzBuzz);
                }

                RemoveMessage_Fizz_WRT_Product();
                RemoveMessage_Buzz_WRT_Product();
            }
        }

        private void ValidateAllFields()
        {
            var fizzBuzz = GetModel();

            //Clear all validation errors.
            validationMessageStore.Clear();

            //Validate all the fields.
            ValidateField(new FieldIdentifier(fizzBuzz, nameof(FizzBuzz.FizzValue)));
            ValidateField(new FieldIdentifier(fizzBuzz, nameof(FizzBuzz.BuzzValue)));
            ValidateField(new FieldIdentifier(fizzBuzz, nameof(FizzBuzz.StopValue)));

            //Notify the edit context of the validation state change
            CurrentEditContext.NotifyValidationStateChanged();
        }

        private FizzBuzz GetModel()
        {
            return CurrentEditContext.Model as FizzBuzz ??
                throw new InvalidOperationException(
                    $"{nameof(FizzBuzzValidator)} requires a Model of type {nameof(FizzBuzz)}"
                );
        }

        private void AddMessage_Fizz_WRT_Buzz()
        {
            validationMessageStore.Add(
                CurrentEditContext.Field(nameof(FizzBuzz.FizzValue)),
                Message_Fizz_WRT_Buzz
            );
        }

        private void AddMessage_Fizz_WRT_Product(FizzBuzz fizzBuzz)
        {
            validationMessageStore.Add(
                CurrentEditContext.Field(nameof(FizzBuzz.FizzValue)),
                Message_Fizz_WRT_Product.Replace(
                    Number_Place_Marker,
                    "" + (fizzBuzz.FizzValue * fizzBuzz.BuzzValue)
                )
            );
        }

        private void AddMessage_Buzz_WRT_Fizz()
        {
            validationMessageStore.Add(
                CurrentEditContext.Field(nameof(FizzBuzz.BuzzValue)),
                Message_Buzz_WRT_Fizz
            );
        }

        private void AddMessage_Buzz_WRT_Product(FizzBuzz fizzBuzz)
        {
            validationMessageStore.Add(
                CurrentEditContext.Field(nameof(FizzBuzz.BuzzValue)),
                Message_Buzz_WRT_Product.Replace(
                    Number_Place_Marker,
                    "" + (fizzBuzz.FizzValue * fizzBuzz.BuzzValue)
                )
            );
        }

        private void AddMessage_Stop_WRT_Product(FizzBuzz fizzBuzz)
        {
            validationMessageStore.Add(
                CurrentEditContext.Field(nameof(FizzBuzz.StopValue)),
                Message_Stop_WRT_Product.Replace(
                    Number_Place_Marker,
                    "" + (fizzBuzz.FizzValue * fizzBuzz.BuzzValue)
                )
            );
        }

        private void RemoveMessage_Fizz_WRT_Buzz()
        {
            RemoveMessage(CurrentEditContext.Field(nameof(FizzBuzz.FizzValue)), Message_Fizz_WRT_Buzz);
        }

        private void RemoveMessage_Fizz_WRT_Product()
        {
            RemoveMessage(CurrentEditContext.Field(nameof(FizzBuzz.FizzValue)), Message_Fizz_WRT_Product);
        }

        private void RemoveMessage_Buzz_WRT_Fizz()
        {
            RemoveMessage(CurrentEditContext.Field(nameof(FizzBuzz.BuzzValue)), Message_Buzz_WRT_Fizz);
        }

        private void RemoveMessage_Buzz_WRT_Product()
        {
            RemoveMessage(CurrentEditContext.Field(nameof(FizzBuzz.BuzzValue)), Message_Buzz_WRT_Product);
        }

        private void RemoveMessage_Stop_WRT_Product()
        {
            RemoveMessage(CurrentEditContext.Field(nameof(FizzBuzz.StopValue)), Message_Stop_WRT_Product);
        }

        private void RemoveMessage(FieldIdentifier fieldIdentifier, string message)
        {
            IEnumerable<string> fieldMessages = validationMessageStore[fieldIdentifier];
            IEnumerable<string> remainingMessages = fieldMessages.Where(
                fieldMessage => (message != Regex.Replace(fieldMessage, Number_Matcher, Number_Place_Marker))
            );
            validationMessageStore.Clear(fieldIdentifier);

            foreach (var item in remainingMessages)
            {
                validationMessageStore.Add(fieldIdentifier, item);
            }
        }
    }
}
