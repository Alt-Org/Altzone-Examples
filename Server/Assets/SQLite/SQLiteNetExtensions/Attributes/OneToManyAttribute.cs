namespace SQLite.SQLiteNetExtensions.Attributes
{
    public class OneToManyAttribute : RelationshipAttribute
    {
        public OneToManyAttribute(string inverseForeignKey = null, string inverseProperty = null)
            : base(null, inverseForeignKey, inverseProperty)
        {
        }
    }
}