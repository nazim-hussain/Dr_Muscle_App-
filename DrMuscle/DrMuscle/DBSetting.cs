using SQLite;

namespace DrMuscle
{
    [Table("Setting")]
    public class DBSetting
    {
        [PrimaryKey]
        public string Key { get; set; }
        public string Value { get; set; }
    }

    [Table("Reco")]
    public class DBReco
    {
        [PrimaryKey]
        public string Key { get; set; }
        public string Value { get; set; }
    }
}