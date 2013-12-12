using System;

namespace Micajah.AzureFileService.WebControls
{
    public interface IUploadControl
    {
        /// <summary>
        /// Gets or sets the name of the container the files are uploaded to.
        /// </summary>
        string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the type of the object which the files are associated with.
        /// </summary>
        string ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the object which the files are associated with.
        /// </summary>
        string ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the connection string to the storage.
        /// </summary>
        string StorageConnectionString { get; set; }
    }
}
