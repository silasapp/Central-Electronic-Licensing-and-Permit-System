using ELPS.Domain.Entities;
using ELPS.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Transactions;
using Microsoft.Azure; // Namespace for CloudConfigurationManager

using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob;

namespace ELPS.Helpers
{

    public class FileHelper
    {
        IFileRepository _fileRep;
        ICompany_DocumentRepository _compDocRep;
        IFacilityDocumentRepository _facDocRep;
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnection"));
        private string location = "Content/UploadedImages/{0}/{1}";

        public FileHelper(IFileRepository fileRep, ICompany_DocumentRepository compDocRep, IFacilityDocumentRepository facDocRep)
        {
            _fileRep = fileRep;
            _compDocRep = compDocRep;
            _facDocRep = facDocRep;
        }

        public CloudBlockBlob SetupBlob(string picName, string ext)
        {
            try
            {
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                string containr = "content";
                CloudBlobContainer container = blobClient.GetContainerReference(containr);
                container.CreateIfNotExists(BlobContainerPublicAccessType.Blob);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(picName);
                //blockBlob.UploadFromStream(file.InputStream);

                ext = ext.ToLower();
                if (ext == ".pdf")
                {
                    blockBlob.Properties.ContentType = "application/pdf";
                }
                else if (ext == ".jpg" || ext == ".jpeg")
                {
                    blockBlob.Properties.ContentType = "image/jpeg";
                }
                else if (ext == ".png")
                {
                    blockBlob.Properties.ContentType = "image/png";
                }
                else if (ext == ".gif")
                {
                    blockBlob.Properties.ContentType = "image/gif";
                }
                else if (ext == ".bmp")
                {
                    blockBlob.Properties.ContentType = "image/bmp";
                }
                return blockBlob;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public FileResponse UploadImage(HttpPostedFile file, string userName, string Ip)
        {
            var fr = new FileResponse();
            int docId = 0;

            if (file != null)
            {
                string username = userName.Replace(";", "");
                var uid = Guid.NewGuid().ToString();
                // HttpPostedFileBase file = request.Files[inputTagName];
                if (file.ContentLength > 0)
                {
                    //string picName = string.Format("{0}_image_{1}_{2}", property.Replace(" ", ""), uid, file.FileName); //new Common(appPath).GetPicName(Path.GetExtension(file.FileName));


                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();


                    string containr = "content";
                    CloudBlobContainer container = blobClient.GetContainerReference(containr);


                    container.CreateIfNotExists(BlobContainerPublicAccessType.Blob);

                    //string picName = string.Format("image_{0}", uid + Path.GetExtension(file.FileName));

                    string picName = $"Content/UploadedImages/{username}/image_{uid + Path.GetExtension(file.FileName)}";
                    //upload a blob

                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(picName);

                    blockBlob.UploadFromStream(file.InputStream);

                    //UtilityHelper.LogMessage($"{ blockBlob.Uri.AbsolutePath} :: {blockBlob.Uri.AbsoluteUri}");

                    //string picName = string.Format("Image_{0}", uid + Path.GetExtension(file.FileName));

                    //string location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/{0}", username));
                    //string filePath = Path.Combine(location,
                    // Path.GetFileName(picName.Replace(";", "")));
                    //if (!Directory.Exists(location))
                    //    Directory.CreateDirectory(location);
                    //file.SaveAs(filePath);
                    ELPS.Domain.Entities.File doc = new ELPS.Domain.Entities.File();
                    //img.Id = Guid.NewGuid();
                    doc.Source = $"/{ picName}";// string.Format(@"~/content/UploadedImages/{1}/{0}", picName, username);
                    doc.Name = file.FileName;
                    doc.Size = file.ContentLength.ToString();
                    doc.Mime = file.ContentType;
                    //doc.Model_Id = modelId;
                    //doc.Model_Name = "";

                    //doc.Sort_Order 
                    _fileRep.Add(doc);
                    _fileRep.Save(username, Ip);
                    //docId = doc.Id;//string.Format("{0};{2}", doc.Id.ToString(), doc.Source);
                    fr.FileId = doc.Id;
                    fr.source = doc.Source;
                    fr.name = file.FileName;
                }

            }

            return fr;
        }

        /// <summary>
        /// Uploads a new Company Document
        /// </summary>
        /// <param name="file">Document file to save</param>
        /// <param name="docTypeId">Document Type Id</param>
        /// <param name="compId">Id of the company to upload document for</param>
        /// <param name="userName"></param>
        /// <param name="Ip"></param>
        /// <param name="docName">(Optional)For extra doxument under "Other Document" type</param>
        /// <param name="uniqueid">(Optional) Unique  </param>
        /// <returns>Returns the Document Id</returns>
        public FileResponse UploadCompDoc(HttpPostedFile file, int docTypeId, int compId, string userName, string Ip, string docName = "", string uniqueid = "")
        {
            int docId = 0;
            var fr = new FileResponse();
            //if (file != null)
            if (file != null && file.ContentLength > 0 && file.ContentLength <= 4000000 && AllowedFileTypes().Contains(file.ContentType))
            {
                //string username = userName.Replace(";", "");
                //var uid = Guid.NewGuid().ToString();
                if (file.ContentLength > 0)
                {
                    string username = "CDOC_" + compId;
                    var uid = Guid.NewGuid().ToString();
                    var ext = Path.GetExtension(file.FileName);
                    string picName = string.Format(location, username, $"file_{uid + ext}");
                    CloudBlockBlob blockBlob = SetupBlob(picName, ext);
                    if (blockBlob != null)
                    {
                        //CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                        //string containr = "content";
                        //CloudBlobContainer container = blobClient.GetContainerReference(containr);
                        //container.CreateIfNotExists(BlobContainerPublicAccessType.Blob);
                        //upload a blob
                        //CloudBlockBlob blockBlob = container.GetBlockBlobReference(picName);

                        blockBlob.UploadFromStream(file.InputStream);

                        var doc = new Company_Document();
                        doc.Source = $"/{picName}";// string.Format(@"/content/UploadedImages/{0}/{1}", username, picName);
                        doc.Name = file.FileName;
                        doc.Document_Type_Id = docTypeId;
                        doc.Type = file.ContentType;
                        doc.Company_Id = compId;
                        doc.Date_Added = UtilityHelper.CurrentTime;
                        doc.Date_Modified = UtilityHelper.CurrentTime;
                        doc.Status = true;
                        if (!string.IsNullOrEmpty(docName))
                            doc.Document_Name = docName;
                        if (!string.IsNullOrEmpty(uniqueid))
                            doc.UniqueId = uniqueid;

                        //doc.Sort_Order 
                        _compDocRep.Add(doc);
                        _compDocRep.Save(username, Ip);

                        fr.FileId = doc.Id;
                        fr.name = file.FileName;
                        fr.source = doc.Source;
                        //docId = doc.Id;
                    }
                }
            }
            else
            {
                fr.FileId = -1;
            }
            return fr;
        }

        /// <summary>
        /// Updates Company/Facility Document
        /// </summary>
        /// <param name="file"></param>
        /// <param name="docId"></param>
        /// <param name="compId"></param>
        /// <param name="userName"></param>
        /// <param name="Ip"></param>
        /// <param name="docName"></param>
        /// <returns></returns>
        public FileResponse UpdateCompDoc(HttpPostedFile file, int docId, int compId, string userName, string Ip, string docName = "", string docFor = "")
        {
            var fr = new FileResponse();
            if (file != null && docId > 0 && file.ContentLength > 0 && file.ContentLength <= 4000000 && AllowedFileTypes().Contains(file.ContentType))
            {

                if (string.IsNullOrEmpty(docFor) || docFor.ToLower() == "company") //(doc != null)
                {
                    var doc = _compDocRep.FindBy(a => a.Id == docId && a.Company_Id == compId).FirstOrDefault();

                    string username = "CDOC_" + compId;
                    var uid = Guid.NewGuid().ToString();
                    var ext = Path.GetExtension(file.FileName);
                    string picName = string.Format(location, username, $"file_{uid + ext}");
                    CloudBlockBlob blockBlob = SetupBlob(picName, ext);
                    if (blockBlob != null)
                    {
                        blockBlob.UploadFromStream(file.InputStream);
                        if (doc == null)
                        {
                            doc = new Company_Document();
                        }
                        UtilityHelper.LogMessage("Updating Company File: " + doc.Document_Type_Id);
                        doc.Source = $"/{ picName}";
                        doc.Name = file.FileName;
                        doc.Type = file.ContentType;
                        doc.Date_Modified = UtilityHelper.CurrentTime;
                        doc.Status = true;
                        doc.Document_Name = !string.IsNullOrEmpty(docName) ? docName : doc.Document_Name;
                        if (doc.Id > 0)
                            _compDocRep.Edit(doc);
                        else
                            _compDocRep.Add(doc);
                        _compDocRep.Save(username, Ip);

                        fr.FileId = doc.Id;
                        fr.source = doc.Source;
                        return fr;
                    }
                }
                else
                {
                    //Try if its Facility Doc that needs updating
                    var fdoc = _facDocRep.FindBy(a => a.Id == docId && a.Company_Id == compId).FirstOrDefault();

                    if (fdoc == null)
                    {
                        UtilityHelper.LogMessage($"Facility for comp ({compId}) File not found");
                    }
                    else
                    {
                        UtilityHelper.LogMessage("Updating Facility File: " + fdoc?.Document_Type_Id);
                        string username = "Facility/FAC_" + fdoc.FacilityId;
                        var uid = Guid.NewGuid().ToString();
                        var ext = Path.GetExtension(file.FileName);
                        string picName = string.Format(location, username, $"file_{uid + ext}");
                        CloudBlockBlob blockBlob = SetupBlob(picName, ext);

                        if (blockBlob != null)
                        {
                            blockBlob.UploadFromStream(file.InputStream);
                            if (fdoc == null)
                            {
                                fdoc = new FacilityDocument();
                            }
                            fdoc.Source = $"/{picName}";
                            fdoc.Name = file.FileName;
                            fdoc.Date_Modified = UtilityHelper.CurrentTime;
                            if (fdoc.Id > 0)
                                _facDocRep.Edit(fdoc);
                            else
                                _facDocRep.Add(fdoc);
                            _facDocRep.Save("System", Ip);

                            docId = fdoc.Id;

                            fr.FileId = fdoc.Id;
                            fr.source = fdoc.Source;
                            return fr;
                        }
                    }
                }

            }
            //if we get this far, something failed
            fr.FileId = -1;
            return fr;
        }

        /// <summary>
        /// Uploads a new facility Document for aCompany
        /// </summary>
        /// <param name="file">Document file to save</param>
        /// <param name="docTypeId">Document Type Id</param>
        /// <param name="compId">Id of the company that owns the Facility</param>
        /// <param name="facilityId">Id of the facility to upload document for</param>
        /// <param name="userName"></param>
        /// <param name="Ip"></param>
        /// <param name="docName">(Optional)For extra doxument under "Other Document" type</param>
        /// <param name="uniqueid">(Optional) Unique  </param>
        /// <returns>Returns the Document Id</returns>
        /// 
        public FileResponse UploadFacilityDoc(HttpPostedFile file, int docTypeId, int compId, int facilityId, string userName, string Ip, string docName = "", string uniqueid = "")
        {
            int docId = 0;
            var fr = new FileResponse();
            if (file != null && file.ContentLength > 0 && file.ContentLength <= 4000000 && AllowedFileTypes().Contains(file.ContentType))
            {
                string username = "FAC_" + compId;
                var uid = Guid.NewGuid().ToString();
                var ext = Path.GetExtension(file.FileName);
                string picName = string.Format(location, username, $"file_{uid + ext}");
                CloudBlockBlob blockBlob = SetupBlob(picName, ext);
                if (blockBlob != null)
                {
                    blockBlob.UploadFromStream(file.InputStream);
                    var doc = new FacilityDocument();

                    doc.Source = $"/{picName}";
                    doc.Name = file.FileName;
                    doc.Document_Type_Id = docTypeId;
                    doc.FacilityId = facilityId;
                    doc.Company_Id = compId;
                    doc.Date_Added = UtilityHelper.CurrentTime;
                    doc.Date_Modified = UtilityHelper.CurrentTime;
                    doc.Status = true;
                    doc.Archived = false;
                    if (!string.IsNullOrEmpty(docName))
                        doc.Document_Name = docName;
                    if (!string.IsNullOrEmpty(uniqueid))
                        doc.UniqueId = uniqueid;

                    //doc.Sort_Order 
                    _facDocRep.Add(doc);
                    _facDocRep.Save(username, Ip);

                    docId = doc.Id;

                    fr.name = file.FileName;
                    fr.FileId = doc.Id;
                    fr.source = doc.Source;  // string.Format(@"/content/UploadedImages/Facility/{1}/{0}", picName, username);
                }
            }
            else
            {
                fr.FileId = -1;
            }

            return fr;

            #region
            //if (file != null)
            //{
            //    string username = userName.Replace(";", "");
            //    var uid = Guid.NewGuid().ToString();
            //    if (file.ContentLength > 0)
            //    {
            //        //string picName = string.Format("{0}_image_{1}_{2}", property.Replace(" ", ""), uid, file.FileName); //new Common(appPath).GetPicName(Path.GetExtension(file.FileName));

            //        //CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();


            //        //string containr = $"content";
            //        //CloudBlobContainer container = blobClient.GetContainerReference(containr);


            //        //container.CreateIfNotExists(BlobContainerPublicAccessType.Blob);
            //        //string picName = $"Content/UploadedImages/Facility/{username}image_{uid + Path.GetExtension(file.FileName)}";
            //        ////string picName = string.Format("image_{0}", uid + Path.GetExtension(file.FileName));

            //        ////upload a blob

            //        //CloudBlockBlob blockBlob = container.GetBlockBlobReference(picName);

            //        //blockBlob.UploadFromStream(file.InputStream);

            //        //string picName = string.Format("image_{0}", uid + Path.GetExtension(file.FileName));

            //        //string location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/Facility/{0}", username));
            //        //string filePath = Path.Combine(location,
            //        // Path.GetFileName(picName.Replace(";", "")));
            //        //if (!Directory.Exists(location))
            //        //    Directory.CreateDirectory(location);
            //        //file.SaveAs(filePath);

            //        //var doc = new FacilityDocument();
            //        ////img.Id = Guid.NewGuid();
            //        //doc.Source = $"/{ picName}";// string.Format(@"~/content/UploadedImages/Facility/{1}/{0}", picName, username);
            //        //doc.Name = file.FileName;
            //        //doc.Document_Type_Id = docTypeId;
            //        //doc.FacilityId = facilityId;
            //        //doc.Company_Id = compId;
            //        //doc.Date_Added = UtilityHelper.CurrentTime;
            //        //doc.Date_Modified = UtilityHelper.CurrentTime;
            //        //doc.Status = true;
            //        //doc.Archived = false;
            //        //if (!string.IsNullOrEmpty(docName))
            //        //    doc.Document_Name = docName;
            //        //if (!string.IsNullOrEmpty(uniqueid))
            //        //    doc.UniqueId = uniqueid;

            //        ////doc.Sort_Order 
            //        //_facDocRep.Add(doc);
            //        //_facDocRep.Save(username, Ip);

            //        //docId = doc.Id;

            //        //fr.name = file.FileName;
            //        //fr.FileId = doc.Id;
            //        //fr.source = $"/{picName}";// string.Format(@"/content/UploadedImages/Facility/{1}/{0}", picName, username);
            //    }
            //}

            #endregion
        }

        /// <summary>
        /// Updates Company/Facility Document
        /// </summary>
        /// <param name="file"></param>
        /// <param name="docId"></param>
        /// <param name="facId"></param>
        /// <param name="compId"></param>
        /// <param name="userName"></param>
        /// <param name="Ip"></param>
        /// <param name="docName"></param>
        /// <returns></returns>
        public FileResponse UpdateFacilityDoc(HttpPostedFile file, int docId, int compId, int facId, string userName, string Ip)
        {
            var fr = new FileResponse();
            if (file != null && docId > 0 && file.ContentLength > 0 && file.ContentLength <= 4000000 && AllowedFileTypes().Contains(file.ContentType))
            {
                //Try if its Facility Doc that needs updating
                var fdoc = _facDocRep.FindBy(a => a.Id == docId && a.FacilityId == facId).FirstOrDefault();

                if (fdoc == null)
                {
                    UtilityHelper.LogMessage($"Facility for comp ({compId}) File not found - Create afresh");
                }
                else
                {
                    UtilityHelper.LogMessage("Updating Facility File: " + fdoc?.Document_Type_Id);
                }

                string username = "Facility/FAC_" + facId;
                var uid = Guid.NewGuid().ToString();
                var ext = Path.GetExtension(file.FileName);
                string picName = string.Format(location, username, $"file_{uid + ext}");
                CloudBlockBlob blockBlob = SetupBlob(picName, ext);

                if (blockBlob != null)
                {
                    blockBlob.UploadFromStream(file.InputStream);
                    if (fdoc == null)
                    {
                        fdoc = new FacilityDocument();
                    }
                    fdoc.Source = $"/{picName}"; // string.Format(@"~/content/UploadedImages/Facility/{1}/{0}", picName, "FAC_" + fdoc.FacilityId);
                    fdoc.Name = file.FileName;
                    fdoc.Date_Modified = UtilityHelper.CurrentTime;

                    if (fdoc.Company_Id != compId)
                    {
                        fdoc.Company_Id = compId;
                    }
                    if (fdoc.Id > 0)
                        _facDocRep.Edit(fdoc);
                    else
                        _facDocRep.Add(fdoc);
                    _facDocRep.Save("System", Ip);

                    docId = fdoc.Id;

                    fr.FileId = fdoc.Id;
                    fr.source = fdoc.Source;
                    return fr;
                }

            }

            fr.FileId = -1;
            return fr;
        }

        //public List<string> getContainer()
        //{
        //    var l = new List<string>();
        //    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

        //    var bd = storageAccount.CreateCloudBlobClient();

        //    string containr = $"content";
        //    CloudBlobContainer container = blobClient.GetContainerReference(containr);


        //    container.CreateIfNotExists(BlobContainerPublicAccessType.Blob);


        //    var blobDirectory = container.GetDirectoryReference("Content/UploadedImages/");//.GetBlobDirectoryReference("Path_to_dir");
        //    foreach (var item in blobDirectory.ListBlobs().OrderByDescending(a => a.Uri).Take(10))
        //    {
        //        l.Add(item.Uri.ToString());
        //    }

        //    //bool directoryExists = blobDirectory.ListBlobs().Any()
        //    //string picName = string.Format("image_{0}", uid + Path.GetExtension(file.FileName));

        //    //upload a blob
        //    return l;
        //    // CloudBlockBlob blockBlob = container.GetBlockBlobReference(picName);

        //}

        public List<string> AllowedFileTypes()
        {
            var list = new List<string>();
            list.Add("image/png");
            list.Add("image/jpg");
            list.Add("image/jpeg");
            list.Add("application/pdf");

            return list;
        }
    }

    //public class FileHelper
    //{
    //    IFileRepository _fileRep;
    //    ICompany_DocumentRepository _compDocRep;
    //    IFacilityDocumentRepository _facDocRep;

    //    public FileHelper(IFileRepository fileRep, ICompany_DocumentRepository compDocRep,IFacilityDocumentRepository facDocRep)
    //    {
    //        _fileRep = fileRep;
    //        _compDocRep = compDocRep;
    //        _facDocRep=facDocRep;
    //    }
    //    public int UploadImage(HttpPostedFile file,  string userName, string Ip)
    //    {
    //        int docId = 0;

    //        if (file != null)
    //        {
    //            string username = userName.Replace(";", "");
    //            var uid = Guid.NewGuid().ToString();
    //            // HttpPostedFileBase file = request.Files[inputTagName];
    //            if (file.ContentLength > 0)
    //            {
    //                //string picName = string.Format("{0}_image_{1}_{2}", property.Replace(" ", ""), uid, file.FileName); //new Common(appPath).GetPicName(Path.GetExtension(file.FileName));

    //                string picName = string.Format("Image_{0}", uid + Path.GetExtension(file.FileName));

    //                string location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/{0}", username));
    //                string filePath = Path.Combine(location,
    //                 Path.GetFileName(picName.Replace(";", "")));
    //                if (!Directory.Exists(location))
    //                    Directory.CreateDirectory(location);
    //                file.SaveAs(filePath);
    //                ELPS.Domain.Entities.File doc = new ELPS.Domain.Entities.File();
    //                //img.Id = Guid.NewGuid();
    //                doc.Source = string.Format(@"~/content/UploadedImages/{1}/{0}", picName, username);
    //                doc.Name = file.FileName;
    //                doc.Size = file.ContentLength.ToString();
    //                doc.Mime = file.ContentType;
    //                //doc.Model_Id = modelId;
    //                //doc.Model_Name = "";

    //                //doc.Sort_Order 
    //                _fileRep.Add(doc);
    //                _fileRep.Save(username, Ip);


    //                docId = doc.Id;//string.Format("{0};{2}", doc.Id.ToString(), doc.Source);
    //            }

    //        }

    //        return docId;
    //    }


    //    /// <summary>
    //    /// Uploads a new Company Document
    //    /// </summary>
    //    /// <param name="file">Document file to save</param>
    //    /// <param name="docTypeId">Document Type Id</param>
    //    /// <param name="compId">Id of the company to upload document for</param>
    //    /// <param name="userName"></param>
    //    /// <param name="Ip"></param>
    //    /// <param name="docName">(Optional)For extra doxument under "Other Document" type</param>
    //    /// <param name="uniqueid">(Optional) Unique  </param>
    //    /// <returns>Returns the Document Id</returns>
    //    public int UploadCompDoc(HttpPostedFile file,  int docTypeId, int compId, string userName, string Ip, string docName ="", string uniqueid = "")
    //    {
    //        int docId = 0;

    //        if (file != null)
    //        {
    //            string username = userName.Replace(";", "");
    //            var uid = Guid.NewGuid().ToString();
    //            if (file.ContentLength > 0)
    //            {
    //                //string picName = string.Format("{0}_image_{1}_{2}", property.Replace(" ", ""), uid, file.FileName); //new Common(appPath).GetPicName(Path.GetExtension(file.FileName));

    //                string picName = string.Format("image_{0}", uid + Path.GetExtension(file.FileName));

    //                string location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/{0}", username));
    //                string filePath = Path.Combine(location,
    //                 Path.GetFileName(picName.Replace(";", "")));
    //                if (!Directory.Exists(location))
    //                    Directory.CreateDirectory(location);
    //                file.SaveAs(filePath);

    //                var doc = new Company_Document();
    //                //img.Id = Guid.NewGuid();
    //                doc.Source = string.Format(@"~/content/UploadedImages/{1}/{0}", picName, username);
    //                doc.Name = file.FileName;
    //                doc.Document_Type_Id = docTypeId;
    //                doc.Type = file.ContentType;
    //                doc.Company_Id = compId;
    //                doc.Date_Added = UtilityHelper.CurrentTime;
    //                doc.Date_Modified = UtilityHelper.CurrentTime;
    //                doc.Status = true;
    //                if(!string.IsNullOrEmpty(docName))
    //                    doc.Document_Name = docName;
    //                if(!string.IsNullOrEmpty(uniqueid))
    //                    doc.UniqueId = uniqueid;

    //                //doc.Sort_Order 
    //                _compDocRep.Add(doc);
    //                _compDocRep.Save(username, Ip);

    //                docId = doc.Id; 
    //            }
    //        }

    //        return docId;
    //    }


    //    public int UpdateCompDoc(HttpPostedFile file, int docId, int compId, string userName, string Ip, string docName = "")
    //    {
    //        UtilityHelper.LogMessage("Updating Coy Docs >>> Size: " + file.ContentLength + "; Type: " + file.ContentType);

    //        if (file != null && docId > 0 && file.ContentLength > 0 && file.ContentLength <= 4000000 && AllowedFileTypes().Contains(file.ContentType))
    //        {
    //            using (var trans = new TransactionScope())
    //            {

    //                string username = "CDOC_" + compId; // userName.Replace("@", "");
    //                var uid = Guid.NewGuid().ToString();
    //                string picName = string.Format("image_{0}", uid + Path.GetExtension(file.FileName));
    //                string location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/{0}", username));
    //                string filePath = Path.Combine(location, Path.GetFileName(picName.Replace(";", "")));
    //                try
    //                {
    //                    var doc = _compDocRep.FindBy(a => a.Id == docId && a.Company_Id == compId).FirstOrDefault();
    //                    if (doc != null)
    //                    {
    //                        UtilityHelper.LogMessage("Updating Company File: " + doc.Document_Type_Id);
    //                        if (!Directory.Exists(location))
    //                            Directory.CreateDirectory(location);
    //                        file.SaveAs(filePath);
    //                        //img.Id = Guid.NewGuid();
    //                        doc.Source = string.Format(@"~/content/UploadedImages/{1}/{0}", picName, username);
    //                        doc.Name = file.FileName;
    //                        doc.Type = file.ContentType;
    //                        doc.Date_Modified = UtilityHelper.CurrentTime;
    //                        doc.Status = true;
    //                        doc.Document_Name = !string.IsNullOrEmpty(docName) ? docName : doc.Document_Name;

    //                        _compDocRep.Edit(doc);
    //                        _compDocRep.Save("System", Ip);

    //                        docId = doc.Id;
    //                        trans.Complete();
    //                        return docId;
    //                    }
    //                    else
    //                    {
    //                        //Try if its Facility Doc that needs updating
    //                        var fdoc = _facDocRep.FindBy(a => a.Id == docId && a.Company_Id == compId).FirstOrDefault();
    //                        if (fdoc != null)
    //                        {
    //                            UtilityHelper.LogMessage("Updating Facility File: " + fdoc.Document_Type_Id);
    //                            location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/Facility/{0}", "FAC_" + fdoc.FacilityId));
    //                            filePath = Path.Combine(location, Path.GetFileName(picName.Replace(";", "")));
    //                            if (!Directory.Exists(location))
    //                                Directory.CreateDirectory(location);
    //                            file.SaveAs(filePath);

    //                            fdoc.Source = string.Format(@"~/content/UploadedImages/Facility/{1}/{0}", picName, "FAC_" + fdoc.FacilityId);
    //                            fdoc.Name = file.FileName;
    //                            fdoc.Date_Modified = UtilityHelper.CurrentTime;

    //                            _facDocRep.Edit(fdoc);
    //                            _facDocRep.Save("System", Ip);

    //                            docId = fdoc.Id;
    //                            trans.Complete();
    //                            return docId;
    //                        }
    //                        else
    //                        {
    //                            UtilityHelper.LogMessage("No document found for update");
    //                            trans.Dispose();
    //                            return 0;
    //                        }
    //                    }
    //                }
    //                catch (Exception)
    //                {
    //                    //throw;
    //                    trans.Dispose();
    //                    if ((System.IO.File.Exists(filePath)))
    //                    {
    //                        System.IO.File.Delete(filePath);
    //                    }
    //                    UtilityHelper.LogMessage("Error occured while processing File Saving.");
    //                    return 0;
    //                }
    //            }
    //        }
    //        else
    //            UtilityHelper.LogMessage("Content out of range OR invalid File type");
    //            return 0;
    //    }



    //    /// <summary>
    //    /// Uploads a new facility Document for aCompany
    //    /// </summary>
    //    /// <param name="file">Document file to save</param>
    //    /// <param name="docTypeId">Document Type Id</param>
    //    /// <param name="compId">Id of the company that owns the Facility</param>
    //    /// <param name="facilityId">Id of the facility to upload document for</param>
    //    /// <param name="userName"></param>
    //    /// <param name="Ip"></param>
    //    /// <param name="docName">(Optional)For extra doxument under "Other Document" type</param>
    //    /// <param name="uniqueid">(Optional) Unique  </param>
    //    /// <returns>Returns the Document Id</returns>
    //    /// 
    //    public FileResponse UploadFacilityDoc(HttpPostedFile file, int docTypeId, int compId, int facilityId, string userName, string Ip, string docName = "", string uniqueid = "")
    //    {
    //        int docId = 0;
    //        var fr = new FileResponse();
    //        if (file != null)
    //        {
    //            string username = userName.Replace(";", "");
    //            var uid = Guid.NewGuid().ToString();
    //            if (file.ContentLength > 0)
    //            {
    //                //string picName = string.Format("{0}_image_{1}_{2}", property.Replace(" ", ""), uid, file.FileName); //new Common(appPath).GetPicName(Path.GetExtension(file.FileName));

    //                string picName = string.Format("image_{0}", uid + Path.GetExtension(file.FileName));

    //                string location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/Facility/{0}", "FAC_" + facilityId));
    //                string filePath = Path.Combine(location,
    //                 Path.GetFileName(picName.Replace(";", "")));
    //                if (!Directory.Exists(location))
    //                    Directory.CreateDirectory(location);
    //                file.SaveAs(filePath);
    //                var doc = new FacilityDocument();
    //                //img.Id = Guid.NewGuid();
    //                doc.Source = string.Format(@"~/content/UploadedImages/Facility/{1}/{0}", picName, "FAC_" + facilityId);
    //                doc.Name = file.FileName;
    //                doc.Document_Type_Id = docTypeId;
    //                doc.FacilityId = facilityId;
    //                doc.Company_Id = compId;
    //                doc.Date_Added = UtilityHelper.CurrentTime;
    //                doc.Date_Modified = UtilityHelper.CurrentTime;
    //                doc.Status = true;
    //                doc.Archived = false;
    //                if (!string.IsNullOrEmpty(docName))
    //                    doc.Document_Name = docName;
    //                if (!string.IsNullOrEmpty(uniqueid))
    //                    doc.UniqueId = uniqueid;

    //                //doc.Sort_Order 
    //                _facDocRep.Add(doc);
    //                _facDocRep.Save(username, Ip);

    //                docId = doc.Id;

    //                fr.name = file.FileName;
    //                fr.FileId = doc.Id;
    //                fr.source = string.Format(@"/content/UploadedImages/Facility/{1}/{0}", picName, "FAC_" + facilityId);
    //            }
    //        }

    //        return fr;
    //    }


    //    public int UpdateFacilityDoc(HttpPostedFile file, int docId, int compId, int facilityId, string userName, string Ip, string docName = "")
    //    {
    //        UtilityHelper.LogMessage("Updating Facility Docs >>> Size: " + file.ContentLength + "; Type: " + file.ContentType);

    //        if (file != null && docId > 0 && file.ContentLength > 0 && file.ContentLength <= 4000000 && AllowedFileTypes().Contains(file.ContentType))
    //        {
    //            using (var trans = new TransactionScope())
    //            {

    //                string username = "FAC_" + facilityId; // userName.Replace("@", "");
    //                var uid = Guid.NewGuid().ToString();
    //                string picName = string.Format("image_{0}", uid + Path.GetExtension(file.FileName));
    //                string location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/Facility/{0}/{1}", username, picName));
    //                string filePath = Path.Combine(location, Path.GetFileName(picName.Replace(";", "")));
    //                try
    //                {
    //                    var doc = _compDocRep.FindBy(a => a.Id == docId && a.Company_Id == compId).FirstOrDefault();
    //                    if (doc != null)
    //                    {
    //                        UtilityHelper.LogMessage("Updating Company File: " + doc.Document_Type_Id);
    //                        if (!Directory.Exists(location))
    //                            Directory.CreateDirectory(location);
    //                        file.SaveAs(filePath);
    //                        //img.Id = Guid.NewGuid();
    //                        doc.Source = string.Format(@"~/content/UploadedImages/{1}/{0}", picName, username);
    //                        doc.Name = file.FileName;
    //                        doc.Type = file.ContentType;
    //                        doc.Date_Modified = UtilityHelper.CurrentTime;
    //                        doc.Status = true;
    //                        doc.Document_Name = !string.IsNullOrEmpty(docName) ? docName : doc.Document_Name;

    //                        _compDocRep.Edit(doc);
    //                        _compDocRep.Save("System", Ip);

    //                        docId = doc.Id;
    //                        trans.Complete();
    //                        return docId;
    //                    }
    //                    else
    //                    {
    //                        //Try if its Facility Doc that needs updating
    //                        var fdoc = _facDocRep.FindBy(a => a.Id == docId && a.Company_Id == compId).FirstOrDefault();
    //                        if (fdoc != null)
    //                        {
    //                            UtilityHelper.LogMessage("Updating Facility File: " + fdoc.Document_Type_Id);
    //                            location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/Facility/{0}", "FAC_" + fdoc.FacilityId));
    //                            filePath = Path.Combine(location, Path.GetFileName(picName.Replace(";", "")));
    //                            if (!Directory.Exists(location))
    //                                Directory.CreateDirectory(location);
    //                            file.SaveAs(filePath);

    //                            fdoc.Source = string.Format(@"~/content/UploadedImages/Facility/{1}/{0}", picName, "FAC_" + fdoc.FacilityId);
    //                            fdoc.Name = file.FileName;
    //                            fdoc.Date_Modified = UtilityHelper.CurrentTime;

    //                            _facDocRep.Edit(fdoc);
    //                            _facDocRep.Save("System", Ip);

    //                            docId = fdoc.Id;
    //                            trans.Complete();
    //                            return docId;
    //                        }
    //                        else
    //                        {
    //                            UtilityHelper.LogMessage("No document found for update");
    //                            trans.Dispose();
    //                            return 0;
    //                        }
    //                    }
    //                }
    //                catch (Exception)
    //                {
    //                    //throw;
    //                    trans.Dispose();
    //                    if ((System.IO.File.Exists(filePath)))
    //                    {
    //                        System.IO.File.Delete(filePath);
    //                    }
    //                    UtilityHelper.LogMessage("Error occured while processing File Saving.");
    //                    return 0;
    //                }
    //            }
    //        }
    //        else
    //            UtilityHelper.LogMessage("Content out of range OR invalid File type");
    //        return 0;
    //    }



    //}
}