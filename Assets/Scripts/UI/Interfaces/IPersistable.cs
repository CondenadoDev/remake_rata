// IPersistable.cs
namespace UISystem.Core
{
    public interface IPersistable
    {
        string PersistenceKey { get; }
        void Save();
        void Load();
        bool HasSavedData();
    }
}