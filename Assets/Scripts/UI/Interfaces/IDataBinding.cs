// IDataBinding.cs
namespace UISystem.Core
{
    public interface IDataBinding
    {
        void Bind(object source, string propertyPath);
        void Unbind();
        void UpdateSource();
        void UpdateTarget();
    }
}