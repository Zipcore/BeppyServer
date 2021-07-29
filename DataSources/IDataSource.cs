namespace BeppyServer.DataSources {
    public interface IDataSource {
        Permissions Load();
        void Save(Permissions permissions);
    }
}