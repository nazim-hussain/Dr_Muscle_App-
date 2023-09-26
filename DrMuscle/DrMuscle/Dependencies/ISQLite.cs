using SQLite;

namespace DrMuscle.Dependencies
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection();
    }
}