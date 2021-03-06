﻿using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using MixERP.Net.Common;
using MixERP.Net.Common.Helpers;
using MixERP.Net.WebControls.AttachmentFactory.Resources;

[assembly: WebResource("MixERP.Net.WebControls.AttachmentFactory.AttachmentFactory.js", "application/x-javascript", PerformSubstitution = true)]
[assembly: WebResource("MixERP.Net.WebControls.AttachmentFactory.ajax-file-upload.js", "application/x-javascript", PerformSubstitution = true)]

namespace MixERP.Net.WebControls.AttachmentFactory
{
    public sealed partial class Attachment
    {
        [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
        private void AddJavascript()
        {
            JSUtility.AddJSReference(this.Page, "MixERP.Net.WebControls.AttachmentFactory.ajax-file-upload.js", "AttachmentAjaxFileUpload", this.GetType());
            JSUtility.AddJSReference(this.Page, "MixERP.Net.WebControls.AttachmentFactory.AttachmentFactory.js", "Attachment", this.GetType());
        }

        private void RegisterJavascript()
        {
            string script = JSUtility.GetVar("allowedExtensions", Helpers.ConfigurationHelper.GetAllowedExtensions());

            script += "allowedExtensions = allowedExtensions.split(',');";
            script += JSUtility.GetVar("areYouSureLocalized", Messages.AreYouSure);
            script += JSUtility.GetVar("duplicateFileLocalized", Messages.DuplicateFile);
            script += JSUtility.GetVar("invalidFileLocalized", Messages.InvalidFile);
            script += JSUtility.GetVar("uploadedFilesDeletedLocalized", Messages.UploadFilesDeleted);
            script += JSUtility.GetVar("undoUploadServiceUrl", Helpers.ConfigurationHelper.GetUndoUploadServiceUrl());
            script += JSUtility.GetVar("uploadHandlerUrl", Helpers.ConfigurationHelper.GetUploadHandlerUrl());

            PageUtility.RegisterJavascript("AttachmentUserControl_Resources", script, this.Page, true);
        }
    }
}