namespace Packer.TMP
{
    class RepoModelStore : IModelStore<DataModel>
    {
        public DataModel Read()
        {
            throw new NotImplementedException();
        }

        public void Save(DataModel model)
        {
            var clone = model.JObject.DeepClone();
            // todo: extract tables, tables->measures, tables->partitions->sources->expressions (and possibly other children) into own files
        }
    }
}
