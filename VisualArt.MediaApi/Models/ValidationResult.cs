namespace VisualArt.MediaApi.Models;

public record ValidationResult(bool IsValid, string Message){
    static public ValidationResult Valid => new ValidationResult(true, string.Empty);
    static public ValidationResult Invalid(string message) => new ValidationResult(false, message);
}
