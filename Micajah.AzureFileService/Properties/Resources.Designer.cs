﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Micajah.AzureFileService.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Micajah.AzureFileService.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to delete.
        /// </summary>
        internal static string FileList_DeleteText {
            get {
                return ResourceManager.GetString("FileList_DeleteText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Are you sure you want to delete it?.
        /// </summary>
        internal static string FileList_DeletingConfirmationText {
            get {
                return ResourceManager.GetString("FileList_DeletingConfirmationText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No files found..
        /// </summary>
        internal static string FileList_EmptyDataText {
            get {
                return ResourceManager.GetString("FileList_EmptyDataText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File Name.
        /// </summary>
        internal static string FileList_FileNameText {
            get {
                return ResourceManager.GetString("FileList_FileNameText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Size.
        /// </summary>
        internal static string FileList_SizeText {
            get {
                return ResourceManager.GetString("FileList_SizeText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Date Modified.
        /// </summary>
        internal static string FileList_UpdatedWhenText {
            get {
                return ResourceManager.GetString("FileList_UpdatedWhenText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to View all of these images at once.
        /// </summary>
        internal static string FileList_ViewAllAtOnceLink_Text {
            get {
                return ResourceManager.GetString("FileList_ViewAllAtOnceLink_Text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to cancel.
        /// </summary>
        internal static string FileUpload_CancelText {
            get {
                return ResourceManager.GetString("FileUpload_CancelText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Click or Drag files here to attach.
        /// </summary>
        internal static string FileUpload_DefaultMessage {
            get {
                return ResourceManager.GetString("FileUpload_DefaultMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid file size.
        /// </summary>
        internal static string FileUpload_InvalidFileSize {
            get {
                return ResourceManager.GetString("FileUpload_InvalidFileSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid file type.
        /// </summary>
        internal static string FileUpload_InvalidMimeType {
            get {
                return ResourceManager.GetString("FileUpload_InvalidMimeType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This is an invalid resource request..
        /// </summary>
        internal static string ResourceHandler_InvalidRequest {
            get {
                return ResourceManager.GetString("ResourceHandler_InvalidRequest", resourceCulture);
            }
        }
    }
}
