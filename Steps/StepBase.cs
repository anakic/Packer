namespace Packer.Steps
{
    class StepBase
    {
        public StepBase? Next { get; set; }

        public virtual void Extract(string pbitFilePath, string folderPath)
        {
            Next?.Extract(pbitFilePath, folderPath);
        }

        public virtual void Pack(string folderPath, string pbitFilePath)
        {
            Next?.Pack(folderPath, pbitFilePath);
        }
    }
}
