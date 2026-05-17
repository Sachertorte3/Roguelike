using System;

namespace Utilities.Validation
{
    /// <summary>
    /// When the field is visible in the inspector (e.g. via ShowIf / HideIf), it must have a value (Odin Validator).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class RequiredIfShownAttribute : Attribute
    {
        public string ErrorMessage { get; }

        public RequiredIfShownAttribute()
        {
        }

        public RequiredIfShownAttribute(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
