
namespace BeppyServer {
    public interface INetPackage
    {
        void Decode(PooledBinaryReader reader);
    }
}