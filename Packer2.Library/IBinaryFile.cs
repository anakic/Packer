namespace Packer2.Library
{
    public interface IBinaryFile
    {
        byte [] Read();
        void Write(byte[] bytes);
    }
}
