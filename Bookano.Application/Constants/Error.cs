namespace Bookano.Application.Constants;

public static class Error
{
    public const string RequiredField = "Required Field";
    public const string MaxLength = "{PropertyName} cannot be more than {MaxLength} characters.";
    public const string MaxMinLength =
        "{PropertyName} must be at least {MinLength} and at max {MaxLength} characters long.";
    public const string Duplicated = "Another record with the same {0} already exists!";
    public const string NotAllowedImageExtension =
        "Only (.jpg, .jpeg, .png, .webp) files are allowed.";
    public const string ImageMaxSizeLimit = "File cannot exceed 2MB!";
    public const string NotAllowFutureDates = "Date cannot be in the future!";
    public const string ShouldBeInRange = "{PropertyName} should be between {From} and {To}!";
    public const string PasswordNotMatch = "The password and confirmation password do not match.";
    public const string WeakPassword =
        "Password must be at least 8 characters and include uppercase, lowercase, number, and special character.";
    public const string InvalidUsername = "Username can only conatin letters or digits.";
    public const string OnlyEnglishLetters = "Only English letters are allowed.";
    public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
    public const string OnlyNumbersAndLetters =
        "Only Arabic/English letters or digits are allowed.";
    public const string DenySpecialCharacters = "Special characters are not allowed.";
    public const string InvalidMobileNumber = "Invalid mobile number.";
    public const string InvalidNationalId = "Invalid national ID.";
    public const string InvalidSerialNumber = "Invalid serial number.";
    public const string NotAvailableForRental = "This Book/Copy is not available for rental.";
    public const string InvalidAreaName =
        "Only Arabic or English letters, digits, spaces, and hyphens are allowed.";
    public const string AreaAlreadyExists = "Area already exist in that governorate";
    public const string EmptyImage = "Please select an image";
    public const string BlackListedSubscriber = "This subscriber is black listed.";
    public const string InactiveSubscriber = "This subscriber is inactive.";
    public const string MaxAllowedCopiesReached =
        "This Subscriber has reached maximum allowed rentals";
    public const string CopyIsInRental = "This copy is already rented.";
    public const string ExtendNotAllowedForBlackListed =
        "Rental cannot be extended for blacklisted subscribers.";
    public const string ExtendNotAllowedForInactive =
        "Rental cannot be extended for this subscriber before renewal.";
    public const string ExtendNotAllowed = "Rental cannot be extended.";
    public const string PenalityShouldBePaid = "Penality should be paid.";
    public const string InvalidDuration = "Invalid duration.";
    public const string InvalidEndDate = "Invalid end date.";
    public const string InvalidStartDate = "Invalid start date.";
    public const string ConcurrencyError = "Another user has updated this record. Please refresh and try again.";
}
