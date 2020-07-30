namespace SakuraUpdater
{
    public enum UpdateType
    {
        Binary,
        ZipPackage
    }

    public class UpdateJob
    {
        public string Name, URL, Hash, Target;
        public UpdateType Type = UpdateType.Binary;
    }
}
