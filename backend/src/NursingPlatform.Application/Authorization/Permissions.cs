namespace NursingPlatform.Application.Authorization;

public static class Permissions
{
    public static class Users
    {
        public const string Create = "Users.Create";
        public const string View = "Users.View";
        public const string Edit = "Users.Edit";
        public const string Delete = "Users.Delete";
    }

    public static class Roles
    {
        public const string View = "Roles.View";
        public const string Manage = "Roles.Manage";
    }

    public static class PermissionsGroup
    {
        public const string View = "Permissions.View";
        public const string Manage = "Permissions.Manage";
    }

    public static class Countries
    {
        public const string View = "Countries.View";
        public const string Manage = "Countries.Manage";
    }

    public static class Languages
    {
        public const string View = "Languages.View";
        public const string Manage = "Languages.Manage";
    }

    public static class Exams
    {
        public const string View = "Exams.View";
        public const string Create = "Exams.Create";
        public const string Edit = "Exams.Edit";
        public const string Delete = "Exams.Delete";
    }

    public static class Questions
    {
        public const string View = "Questions.View";
        public const string Manage = "Questions.Manage";
    }

    public static class Nurses
    {
        public const string View = "Nurses.View";
    }

    public static class Employers
    {
        public const string View = "Employers.View";
    }

    public static readonly string[] All =
    [
        Users.Create, Users.View, Users.Edit, Users.Delete,
        Roles.View, Roles.Manage,
        PermissionsGroup.View, PermissionsGroup.Manage,
        Countries.View, Countries.Manage,
        Languages.View, Languages.Manage,
        Exams.View, Exams.Create, Exams.Edit, Exams.Delete,
        Questions.View, Questions.Manage,
        Nurses.View,
        Employers.View
    ];

    public static readonly string[] Admin = All;
}
