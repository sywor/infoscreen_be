namespace Common.Minio
{
    public readonly struct MinioFile
    {
        public static MinioFile Of(string _bucket, string _directory, string _fileName) => new() {Bucket = _bucket, Directory = _directory, FileName = _fileName};
        public string Directory { get; init; }
        public string FileName { get; init; }
        public string Bucket { get; init; }
    }
}