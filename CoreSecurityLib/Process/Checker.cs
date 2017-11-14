using CoreModel;
using MirzaCryptoHelpers.Common;
namespace CoreSecurityLib.Process
{
    public class Checker
    {
        public static bool CompareProcess(ProcessModel sample, ProcessModel process)
        {
            bool fileLengthComparison = sample.FileLength == process.FileLength ? true : false;
            bool initialDataComparison = BitComparer.CompareBytes(sample.InitialBytes, process.InitialBytes);
            return fileLengthComparison && initialDataComparison;
        }

    }
}
