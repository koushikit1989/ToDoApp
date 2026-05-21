namespace ToDoManagementSystem.Shared.Constants;

/// <summary>Application-wide constants for roles, claims, and pagination.</summary>
public static class AppConstants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }

    public static class Claims
    {
        public const string UserId = "sub";
        public const string Email = "email";
        public const string FullName = "name";
        public const string Role = "role";
        public const string Jti = "jti";
    }

    public static class Pagination
    {
        public const int DefaultPageNumber = 1;
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
    }

    public static class Cache
    {
        public const int DefaultExpiryMinutes = 5;
    }
}
