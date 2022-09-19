namespace Packer2.Library
{
    public interface IModelStore<T>
    {
        T Read();
        void Save(T model);
    }
}
