namespace DataModelLoader
{
    public interface IModelStore<T>
    {
        T Read();
        void Save(T model);
    }
}
