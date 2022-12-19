namespace Packer2.Library.Tools
{
    public static class HashSetExtensions
    {
        public static void AddRange<T>(this HashSet<T> me, IEnumerable<T> other)
        {
            foreach (var x in other)
                me.Add(x);
        }
    }

}
