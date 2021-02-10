namespace Common.Minio
{
    public readonly struct MinioFile
    {
        public static MinioFile Of(string _directory, string _fileName) => new() {Directory = _directory, FileName = _fileName};
        public string Directory { get; init; }
        public string FileName { get; init; }
    }
}