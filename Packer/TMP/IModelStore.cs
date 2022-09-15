using Packer.Model;

namespace Packer.TMP
{
    public interface IModelStore<T>
    {
        public T Read();
        public void Save(T model);
    }
}
