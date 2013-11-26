using System;

namespace Micajah.AzureFileService.WebControls
{
    public interface IUploadControl
    {
        /// <summary>
        /// Gets or sets the unique identifier of the organization.
        /// </summary>
        Guid OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the instance.
        /// </summary>
        Guid InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the type of the object which the files are associated with.
        /// </summary>
        string LocalObjectType { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the object which the files are associated with.
        /// </summary>
        string LocalObjectId { get; set; }

        /// <summary>
        /// Gets or sets the connection string to the storage.
        /// </summary>
        string StorageConnectionString { get; set; }
    }
}
