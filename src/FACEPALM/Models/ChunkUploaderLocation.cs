using Commons.Interfaces;
using Npgsql;

namespace FACEPALM.Models
{
    public class ChunkUploaderLocation(string uploadedFileName, string uuid)
        : IDatabaseModels
    {
        //UploadedFileName = {serializedName}-{chunkId}.shas
        public string UploadedFileName { get; set; } = uploadedFileName;
        public string UploaderUuid { get; set; } = uuid;

        public static ChunkUploaderLocation Deserialize(NpgsqlDataReader reader)
        {
            var serializedName = reader.GetString(0);
            var uuid = reader.GetString(1);

            return new ChunkUploaderLocation(serializedName, uuid);
        }
    }
}