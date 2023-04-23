using ELPS.Domain.Entities;
using ELPS.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Transactions;

namespace ELPS.Helpers
{
    public class FileHelper
    {
        IFileRepository _fileRep;
        ICompany_DocumentRepository _compDocRep;
        IFacilityDocumentRepository _facDocRep;

        public FileHelper(IFileRepository fileRep, ICompany_DocumentRepository compDocRep,IFacilityDocumentRepository facDocRep)
        {
            _fileRep = fileRep;
            _compDocRep = compDocRep;
            _facDocRep=facDocRep;
        }
        public int UploadImage(HttpPostedFile file,  string userName, string Ip)
        {
            int docId = 0;

            if (file != null)
            {
                string username = userName.Replace(";", "");
                var uid = Guid.NewGuid().ToString();
                // HttpPostedFileBase file = request.Files[inputTagName];
                if (file.ContentLength > 0)
                {
                    //string picName = string.Format("{0}_image_{1}_{2}", property.Replace(" ", ""), uid, file.FileName); //new Common(appPath).GetPicName(Path.GetExtension(file.FileName));

                    string picName = string.Format("Image_{0}", uid + Path.GetExtension(file.FileName));

                    string location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/{0}", username));
                    string filePath = Path.Combine(location,
                     Path.GetFileName(picName.Replace(";", "")));
                    if (!Directory.Exists(location))
                        Directory.CreateDirectory(location);
                    file.SaveAs(filePath);
                    ELPS.Domain.Entities.File doc = new ELPS.Domain.Entities.File();
                    //img.Id = Guid.NewGuid();
                    doc.Source = string.Format(@"~/content/UploadedImages/{1}/{0}", picName, username);
                    doc.Name = file.FileName;
                    doc.Size = file.ContentLength.ToString();
                    doc.Mime = file.ContentType;
                    //doc.Model_Id = modelId;
                    //doc.Model_Name = "";

                    //doc.Sort_Order 
                    _fileRep.Add(doc);
                    _fileRep.Save(username, Ip);


                    docId = doc.Id;//string.Format("{0};{2}", doc.Id.ToString(), doc.Source);
                }

            }

            return docId;
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
        public int UploadCompDoc(HttpPostedFile file,  int docTypeId, int compId, string userName, string Ip, string docName ="", string uniqueid = "")
        {
            int docId = 0;

            if (file != null)
            {
                string username = userName.Replace(";", "");
                var uid = Guid.NewGuid().ToString();
                if (file.ContentLength > 0)
                {
                    //string picName = string.Format("{0}_image_{1}_{2}", property.Replace(" ", ""), uid, file.FileName); //new Common(appPath).GetPicName(Path.GetExtension(file.FileName));

                    string picName = string.Format("image_{0}", uid + Path.GetExtension(file.FileName));

                    string location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/{0}", username));
                    string filePath = Path.Combine(location,
                     Path.GetFileName(picName.Replace(";", "")));
                    if (!Directory.Exists(location))
                        Directory.CreateDirectory(location);
                    file.SaveAs(filePath);

                    var doc = new Company_Document();
                    //img.Id = Guid.NewGuid();
                    doc.Source = string.Format(@"~/content/UploadedImages/{1}/{0}", picName, username);
                    doc.Name = file.FileName;
                    doc.Document_Type_Id = docTypeId;
                    doc.Type = file.ContentType;
                    doc.Company_Id = compId;
                    doc.Date_Added = UtilityHelper.CurrentTime;
                    doc.Date_Modified = UtilityHelper.CurrentTime;
                    doc.Status = true;
                    if(!string.IsNullOrEmpty(docName))
                        doc.Document_Name = docName;
                    if(!string.IsNullOrEmpty(uniqueid))
                        doc.UniqueId = uniqueid;

                    //doc.Sort_Order 
                    _compDocRep.Add(doc);
                    _compDocRep.Save(username, Ip);

                    docId = doc.Id; 
                }
            }

            return docId;
        }


        public int UpdateCompDoc(HttpPostedFile file, int docId, int compId, string userName, string Ip, string docName = "")
        {
            UtilityHelper.LogMessage("Updating Coy Docs >>> Size: " + file.ContentLength + "; Type: " + file.ContentType);
            
            if (file != null && docId > 0 && file.ContentLength > 0 && file.ContentLength <= 4000000 && AllowedFileTypes().Contains(file.ContentType))
            {
                using (var trans = new TransactionScope())
                {

                    string username = "CDOC_" + compId; // userName.Replace("@", "");
                    var uid = Guid.NewGuid().ToString();
                    string picName = string.Format("image_{0}", uid + Path.GetExtension(file.FileName));
                    string location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/{0}", username));
                    string filePath = Path.Combine(location, Path.GetFileName(picName.Replace(";", "")));
                    try
                    {
                        var doc = _compDocRep.FindBy(a => a.Id == docId && a.Company_Id == compId).FirstOrDefault();
                        if (doc != null)
                        {
                            UtilityHelper.LogMessage("Updating Company File: " + doc.Document_Type_Id);
                            if (!Directory.Exists(location))
                                Directory.CreateDirectory(location);
                            file.SaveAs(filePath);
                            //img.Id = Guid.NewGuid();
                            doc.Source = string.Format(@"~/content/UploadedImages/{1}/{0}", picName, username);
                            doc.Name = file.FileName;
                            doc.Type = file.ContentType;
                            doc.Date_Modified = UtilityHelper.CurrentTime;
                            doc.Status = true;
                            doc.Document_Name = !string.IsNullOrEmpty(docName) ? docName : doc.Document_Name;

                            _compDocRep.Edit(doc);
                            _compDocRep.Save("System", Ip);

                            docId = doc.Id;
                            trans.Complete();
                            return docId;
                        }
                        else
                        {
                            //Try if its Facility Doc that needs updating
                            var fdoc = _facDocRep.FindBy(a => a.Id == docId && a.Company_Id == compId).FirstOrDefault();
                            if (fdoc != null)
                            {
                                UtilityHelper.LogMessage("Updating Facility File: " + fdoc.Document_Type_Id);
                                location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/Facility/{0}", "FAC_" + fdoc.FacilityId));
                                filePath = Path.Combine(location, Path.GetFileName(picName.Replace(";", "")));
                                if (!Directory.Exists(location))
                                    Directory.CreateDirectory(location);
                                file.SaveAs(filePath);

                                fdoc.Source = string.Format(@"~/content/UploadedImages/Facility/{1}/{0}", picName, "FAC_" + fdoc.FacilityId);
                                fdoc.Name = file.FileName;
                                fdoc.Date_Modified = UtilityHelper.CurrentTime;

                                _facDocRep.Edit(fdoc);
                                _facDocRep.Save("System", Ip);

                                docId = fdoc.Id;
                                trans.Complete();
                                return docId;
                            }
                            else
                            {
                                UtilityHelper.LogMessage("No document found for update");
                                trans.Dispose();
                                return 0;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        //throw;
                        trans.Dispose();
                        if ((System.IO.File.Exists(filePath)))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        UtilityHelper.LogMessage("Error occured while processing File Saving.");
                        return 0;
                    }
                }
            }
            else
                UtilityHelper.LogMessage("Content out of range OR invalid File type");
                return 0;
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
            if (file != null)
            {
                string username = userName.Replace(";", "");
                var uid = Guid.NewGuid().ToString();
                if (file.ContentLength > 0)
                {
                    //string picName = string.Format("{0}_image_{1}_{2}", property.Replace(" ", ""), uid, file.FileName); //new Common(appPath).GetPicName(Path.GetExtension(file.FileName));

                    string picName = string.Format("image_{0}", uid + Path.GetExtension(file.FileName));

                    string location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/Facility/{0}", "FAC_" + facilityId));
                    string filePath = Path.Combine(location,
                     Path.GetFileName(picName.Replace(";", "")));
                    if (!Directory.Exists(location))
                        Directory.CreateDirectory(location);
                    file.SaveAs(filePath);
                    var doc = new FacilityDocument();
                    //img.Id = Guid.NewGuid();
                    doc.Source = string.Format(@"~/content/UploadedImages/Facility/{1}/{0}", picName, "FAC_" + facilityId);
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
                    fr.source = string.Format(@"/content/UploadedImages/Facility/{1}/{0}", picName, "FAC_" + facilityId);
                }
            }

            return fr;
        }


        public int UpdateFacilityDoc(HttpPostedFile file, int docId, int compId, int facilityId, string userName, string Ip, string docName = "")
        {
            UtilityHelper.LogMessage("Updating Facility Docs >>> Size: " + file.ContentLength + "; Type: " + file.ContentType);

            if (file != null && docId > 0 && file.ContentLength > 0 && file.ContentLength <= 4000000 && AllowedFileTypes().Contains(file.ContentType))
            {
                using (var trans = new TransactionScope())
                {

                    string username = "FAC_" + facilityId; // userName.Replace("@", "");
                    var uid = Guid.NewGuid().ToString();
                    string picName = string.Format("image_{0}", uid + Path.GetExtension(file.FileName));
                    string location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/Facility/{0}/{1}", username, picName));
                    string filePath = Path.Combine(location, Path.GetFileName(picName.Replace(";", "")));
                    try
                    {
                        var doc = _compDocRep.FindBy(a => a.Id == docId && a.Company_Id == compId).FirstOrDefault();
                        if (doc != null)
                        {
                            UtilityHelper.LogMessage("Updating Company File: " + doc.Document_Type_Id);
                            if (!Directory.Exists(location))
                                Directory.CreateDirectory(location);
                            file.SaveAs(filePath);
                            //img.Id = Guid.NewGuid();
                            doc.Source = string.Format(@"~/content/UploadedImages/{1}/{0}", picName, username);
                            doc.Name = file.FileName;
                            doc.Type = file.ContentType;
                            doc.Date_Modified = UtilityHelper.CurrentTime;
                            doc.Status = true;
                            doc.Document_Name = !string.IsNullOrEmpty(docName) ? docName : doc.Document_Name;

                            _compDocRep.Edit(doc);
                            _compDocRep.Save("System", Ip);

                            docId = doc.Id;
                            trans.Complete();
                            return docId;
                        }
                        else
                        {
                            //Try if its Facility Doc that needs updating
                            var fdoc = _facDocRep.FindBy(a => a.Id == docId && a.Company_Id == compId).FirstOrDefault();
                            if (fdoc != null)
                            {
                                UtilityHelper.LogMessage("Updating Facility File: " + fdoc.Document_Type_Id);
                                location = HttpContext.Current.Server.MapPath(string.Format("~/content/UploadedImages/Facility/{0}", "FAC_" + fdoc.FacilityId));
                                filePath = Path.Combine(location, Path.GetFileName(picName.Replace(";", "")));
                                if (!Directory.Exists(location))
                                    Directory.CreateDirectory(location);
                                file.SaveAs(filePath);

                                fdoc.Source = string.Format(@"~/content/UploadedImages/Facility/{1}/{0}", picName, "FAC_" + fdoc.FacilityId);
                                fdoc.Name = file.FileName;
                                fdoc.Date_Modified = UtilityHelper.CurrentTime;

                                _facDocRep.Edit(fdoc);
                                _facDocRep.Save("System", Ip);

                                docId = fdoc.Id;
                                trans.Complete();
                                return docId;
                            }
                            else
                            {
                                UtilityHelper.LogMessage("No document found for update");
                                trans.Dispose();
                                return 0;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        //throw;
                        trans.Dispose();
                        if ((System.IO.File.Exists(filePath)))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        UtilityHelper.LogMessage("Error occured while processing File Saving.");
                        return 0;
                    }
                }
            }
            else
                UtilityHelper.LogMessage("Content out of range OR invalid File type");
            return 0;
        }


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
}