namespace EasyHash.Specs.Targets
{
    using System;

    public class Dummy
    {
        public int Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LastNameField;
        public int GraduationYear { get; set; }
        public int GraduationYearField;
        public DateTime BirthDate { get; set; }
        public DateTime BirthDateField;
        public DateTimeKind Enum { get; set; }
        public DateTimeKind EnumField;
        public MyStruct Struct { get; set; }
        public MyStruct StructField;

        public static Dummy Default => new Dummy()
        {
            Age = 77,
            BirthDate = DateTime.Now,
            BirthDateField = DateTime.Now,
            Enum = DateTimeKind.Unspecified,
            EnumField = DateTimeKind.Utc,
            FirstName = "James",
            LastName = "Bond",
            GraduationYear = 1987,
            GraduationYearField = 2017,
            LastNameField = "van"
        };

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Age;
                hashCode = (hashCode * 397) ^ (FirstName != null ? FirstName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LastName != null ? LastName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ GraduationYear;
                hashCode = (hashCode * 397) ^ BirthDate.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Enum;
                hashCode = (hashCode * 397) ^ Struct.GetHashCode();

                hashCode = (hashCode * 397) ^ (LastNameField != null ? LastNameField.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ GraduationYearField;
                hashCode = (hashCode * 397) ^ BirthDateField.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)EnumField;
                hashCode = (hashCode * 397) ^ StructField.GetHashCode();
                return hashCode;
            }
        }

        public struct MyStruct
        {
            public override int GetHashCode() => 777;
        }
    }
}
