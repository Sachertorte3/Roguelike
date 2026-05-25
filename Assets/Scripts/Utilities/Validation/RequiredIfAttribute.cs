using System;

namespace Utilities.Validation
{
    /// <summary>
    /// When <see cref="Condition"/> evaluates to true, the field must have a value (Odin Validator).
    /// Use the same expression syntax as <c>ShowIf</c> / <c>HideIf</c> (e.g. <c>@HasSkill</c> or <c>nameof(HasSkill)</c>).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class RequiredIfAttribute : Attribute
    {
        public string Condition { get; }
        public string ErrorMessage { get; }

        public RequiredIfAttribute(string condition)
        {
            Condition = condition;
        }

        public RequiredIfAttribute(string condition, string errorMessage)
        {
            Condition = condition;
            ErrorMessage = errorMessage;
        }
    }
}
