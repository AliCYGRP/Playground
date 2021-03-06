using CredentialSolution.Database;
using CredentialSolution.IBLL;
using CredentialSolution.Model;
using CredentialSolution.Model.Comman;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace CredentialSolution.BL
{

    public class AdminManager : IAdmin
    {
        CredentialSolutionDbContext _dbContext = new CredentialSolutionDbContext();

        public List<PendingCredentialResponse> GetPendingCredentials()
        {
            var atpObj = _dbContext.AtpCredential
                        .Where(x => x.StatusId == (int)IdentityCredentialStatusModel.IdentityCredentialStatus.Pending)
                        .Join(
                         _dbContext.CompanyDetail,
                         atpCredentialObj => atpCredentialObj.CredentialId,
                         companyDetailsObj => companyDetailsObj.CredentialId,
                         (atpCredentialObj, companyDetailsObj) => new
                         {
                             atpCredentialObj,
                             companyDetailsObj
                         }
                        )
                        .Join(
                           _dbContext.Credentials,
                           atpCredentialsObj => atpCredentialsObj.atpCredentialObj.CredentialId,
                           credentialsObj => credentialsObj.Id,
                           (atpCredentialsCompanyDetailsObj, credentialsObj) => new
                           {
                               atpCredentialsCompanyDetailsObj,
                               credentialsObj
                           }
                   ).Join(
                      _dbContext.PartnerDetails,
                      credentialsObj => credentialsObj.credentialsObj.PartnerDetail_Id,
                      partnerDetailObj => partnerDetailObj.Id,
                       (credentialsAtpCompanyDetailsObj, partnerDetailObj) => new
                       {
                           credentialsAtpCompanyDetailsObj,
                           partnerDetailObj
                       }
                    ).AsEnumerable().
                    Select(x => new PendingCredentialResponse
                    {
                        Id = x.credentialsAtpCompanyDetailsObj.atpCredentialsCompanyDetailsObj.atpCredentialObj.Id + "_atp",
                        CompanyName = x.credentialsAtpCompanyDetailsObj.atpCredentialsCompanyDetailsObj.companyDetailsObj.CompanyName,
                        Partner = x.partnerDetailObj.PartnerName,
                        CreationDate = x.credentialsAtpCompanyDetailsObj.atpCredentialsCompanyDetailsObj.atpCredentialObj.CreationDate.ToString("MM/dd/yyyy HH:mm:ss"),
                        TypeOfCredential = "ATP",
                        ProofSource = "",
                        Status = Enum.GetName(typeof(IdentityCredentialStatusModel.IdentityCredentialStatus), x.credentialsAtpCompanyDetailsObj.atpCredentialsCompanyDetailsObj.atpCredentialObj.StatusId)
                    }).ToList();

            var identityObj = _dbContext.IdentityCredentials
                            .Where(x => x.IdentityCredentialStatusId == (int)IdentityCredentialStatusModel.IdentityCredentialStatus.Pending)
                            .Join(
                             _dbContext.CompanyDetail,
                             identityCredentialObj => identityCredentialObj.CredentialId,
                             companyDetailsObj => companyDetailsObj.CredentialId,
                             (identityCredentialObj, companyDetailsObj) => new
                             {
                                 identityCredentialObj,
                                 companyDetailsObj
                             }
                            ).Join(
                           _dbContext.Credentials,
                           identityCredentialObj => identityCredentialObj.identityCredentialObj.CredentialId,
                           credentialsObj => credentialsObj.Id,
                           (identityCredentialCompanyDetailsObj, credentialsObj) => new
                           {
                               identityCredentialCompanyDetailsObj,
                               credentialsObj
                           }
                            ).Join(
                            _dbContext.PartnerDetails,
                            identityCredentialCompanyDetailsObj => identityCredentialCompanyDetailsObj.credentialsObj.PartnerDetail_Id,
                            partnerDetailObj => partnerDetailObj.Id,
                            (identityCredentialCompanyDetailsCredentialsObj, partnerDetailObj) => new
                            {
                                identityCredentialCompanyDetailsCredentialsObj,
                                partnerDetailObj
                            }
                             ).Join(
                            _dbContext.VerifyIdentityVias,
                            identityCredentialCompanyDetailsObj => identityCredentialCompanyDetailsObj.identityCredentialCompanyDetailsCredentialsObj.identityCredentialCompanyDetailsObj.identityCredentialObj.VerifyIdentityViaId,
                            verifyMyIdentityObj => verifyMyIdentityObj.Id,
                            (identityCredentialCompanyDetailsCredentialsPartnerDetailObj, verifyMyIdentityObj) => new
                            {
                                identityCredentialCompanyDetailsCredentialsPartnerDetailObj,
                                verifyMyIdentityObj
                            }
                             ).AsEnumerable().
               Select(x => new PendingCredentialResponse
               {
                   Id = x.identityCredentialCompanyDetailsCredentialsPartnerDetailObj.identityCredentialCompanyDetailsCredentialsObj.identityCredentialCompanyDetailsObj.identityCredentialObj.Id + "_identity",
                   CompanyName = x.identityCredentialCompanyDetailsCredentialsPartnerDetailObj.identityCredentialCompanyDetailsCredentialsObj.identityCredentialCompanyDetailsObj.companyDetailsObj.CompanyName,
                   Partner = x.identityCredentialCompanyDetailsCredentialsPartnerDetailObj.partnerDetailObj.PartnerName,
                   CreationDate = x.identityCredentialCompanyDetailsCredentialsPartnerDetailObj.identityCredentialCompanyDetailsCredentialsObj.identityCredentialCompanyDetailsObj.identityCredentialObj.CreationDate.ToString("MM/dd/yyyy HH:mm:ss"),
                   TypeOfCredential = "Identity",
                   ProofSource = x.verifyMyIdentityObj.Name,
                   Status = Enum.GetName(typeof(IdentityCredentialStatusModel.IdentityCredentialStatus), x.identityCredentialCompanyDetailsCredentialsPartnerDetailObj.identityCredentialCompanyDetailsCredentialsObj.identityCredentialCompanyDetailsObj.identityCredentialObj.IdentityCredentialStatusId)
               }).ToList();
            identityObj.AddRange(atpObj);
            identityObj = identityObj.OrderByDescending(x => x.CreationDate).ToList();
            return identityObj;
        }
        public List<CredentialReportResponse> GetCredentialReports(CredentialReportRequest Req)
        {
            var partnerName = new List<string>();
            var statusName = new List<string>();
            CredentialReportResponse cr = new CredentialReportResponse();
            if (!string.IsNullOrEmpty(Req.PartnerName))
            {
                partnerName = Req.PartnerName.Split(',').ToList();
            }
            else
            {
                partnerName = _dbContext.PartnerDetails.Select(x => x.PartnerName).ToList();
            }
            if (!string.IsNullOrEmpty(Req.StatusName))
            {
                statusName = Req.StatusName.Split(',').ToList();
            }
            else
            {
                statusName = _dbContext.IdentityCredentialStatus.Select(x => x.Name).ToList();
            }

            var atpObj = _dbContext.AtpCredential.Join(
                                                        _dbContext.IdentityCredentialStatus,
                                                        atpCredentialObj => atpCredentialObj.StatusId,
                                                        statusObj => statusObj.Id,
                                                        (atpCredentialObj, statusObj) => new
                                                        {
                                                            atpCredentialObj,
                                                            statusObj
                                                        }).Where(
                                                            x => statusName.Contains(x.statusObj.Name) &&
                                                            x.atpCredentialObj.CreationDate <= Req.ToDate && x.atpCredentialObj.CreationDate >= Req.FromDate
                                                        ).Join(
                                                        _dbContext.CompanyDetail,
                                                        atpCredentialObj => atpCredentialObj.atpCredentialObj.CredentialId,
                                                        companyDetailsObj => companyDetailsObj.Id,
                                                        (atpCredentialStatusObj, companyDetailsObj) => new
                                                        {
                                                            atpCredentialStatusObj,
                                                            companyDetailsObj
                                                        }).Join(
                                                        _dbContext.CompanyTypes,
                                                        atpCredentialStatusObj => atpCredentialStatusObj.companyDetailsObj.CompanyTypeId,
                                                        companyTypeObj => companyTypeObj.Id,
                                                        (atpCredentialStatusCompanyDetailsObj, companyTypeObj) => new
                                                        {
                                                            atpCredentialStatusCompanyDetailsObj,
                                                            companyTypeObj
                                                        }).Join(
                                                        _dbContext.Credentials,
                                                        atpCredentialStatusCompanyDetailsObj => atpCredentialStatusCompanyDetailsObj.atpCredentialStatusCompanyDetailsObj.atpCredentialStatusObj.atpCredentialObj.CredentialId,
                                                        credentialObj => credentialObj.Id,
                                                        (atpCredentialStatusCompanyDetailsCompanyTypeObj, credentialObj) => new
                                                        {
                                                            atpCredentialStatusCompanyDetailsCompanyTypeObj,
                                                            credentialObj
                                                        }).Join(
                                                        _dbContext.PartnerDetails,
                                                        atpCredentialStatusCompanyDetailsCompanyTypeObj => atpCredentialStatusCompanyDetailsCompanyTypeObj.credentialObj.PartnerDetail_Id,
                                                        partnerDetailsObj => partnerDetailsObj.Id,
                                                        (atpCredentialStatusCompanyDetailsCompanyTypeCredentialsObj, partnerDetailsObj) => new
                                                        {
                                                            atpCredentialStatusCompanyDetailsCompanyTypeCredentialsObj,
                                                            partnerDetailsObj
                                                        }).Where(x => partnerName.Contains(x.partnerDetailsObj.PartnerName)).AsEnumerable()
                                                        .Select(
                                                        x => new CredentialReportResponse
                                                        {
                                                            Id = x.atpCredentialStatusCompanyDetailsCompanyTypeCredentialsObj.atpCredentialStatusCompanyDetailsCompanyTypeObj.atpCredentialStatusCompanyDetailsObj.atpCredentialStatusObj.atpCredentialObj.Id + "_atp",
                                                            CompanyName = x.atpCredentialStatusCompanyDetailsCompanyTypeCredentialsObj.atpCredentialStatusCompanyDetailsCompanyTypeObj.atpCredentialStatusCompanyDetailsObj.companyDetailsObj.CompanyName,
                                                            Partner = x.partnerDetailsObj.PartnerName,
                                                            RequestedOn = x.atpCredentialStatusCompanyDetailsCompanyTypeCredentialsObj.atpCredentialStatusCompanyDetailsCompanyTypeObj.atpCredentialStatusCompanyDetailsObj.atpCredentialStatusObj.atpCredentialObj.CreationDate.ToString("MM/dd/yyyy HH:mm:ss"),
                                                            TypeOfCredntial = "ATP",
                                                            CompanyType = x.atpCredentialStatusCompanyDetailsCompanyTypeCredentialsObj.atpCredentialStatusCompanyDetailsCompanyTypeObj.companyTypeObj.CompanyType,
                                                            Status = x.atpCredentialStatusCompanyDetailsCompanyTypeCredentialsObj.atpCredentialStatusCompanyDetailsCompanyTypeObj.atpCredentialStatusCompanyDetailsObj.atpCredentialStatusObj.statusObj.Name,
                                                            ApprovedOn = "",
                                                            SignedBy = "",
                                                            ProofSource = ""
                                                        }).ToList();

            var identityObj = _dbContext.IdentityCredentials.Join(
                                                        _dbContext.IdentityCredentialStatus,
                                                        identityCredentialObj => identityCredentialObj.IdentityCredentialStatusId,
                                                        statusObj => statusObj.Id,
                                                        (identityCredentialObj, statusObj) => new
                                                        {
                                                            identityCredentialObj,
                                                            statusObj
                                                        }).Where(
                                                            x => statusName.Contains(x.statusObj.Name) &&
                                                            x.identityCredentialObj.CreationDate <= Req.ToDate && x.identityCredentialObj.CreationDate >= Req.FromDate
                                                        ).Join(
                                                        _dbContext.CompanyDetail,
                                                        identityCredentialObj => identityCredentialObj.identityCredentialObj.CredentialId,
                                                        companyDetailsObj => companyDetailsObj.Id,
                                                        (identityCredentialStatusObj, companyDetailsObj) => new
                                                        {
                                                            identityCredentialStatusObj,
                                                            companyDetailsObj
                                                        }).Join(
                                                        _dbContext.CompanyTypes,
                                                        identityCredentialStatusObj => identityCredentialStatusObj.companyDetailsObj.CompanyTypeId,
                                                        companyTypeObj => companyTypeObj.Id,
                                                        (identityCredentialStatusCompanyDetailsObj, companyTypeObj) => new
                                                        {
                                                            identityCredentialStatusCompanyDetailsObj,
                                                            companyTypeObj
                                                        }).Join(
                                                        _dbContext.Credentials,
                                                        identityCredentialStatusCompanyDetailsObj => identityCredentialStatusCompanyDetailsObj.identityCredentialStatusCompanyDetailsObj.identityCredentialStatusObj.identityCredentialObj.CredentialId,
                                                        credentialObj => credentialObj.Id,
                                                        (identityCredentialStatusCompanyDetailsCompanyTypeObj, credentialObj) => new
                                                        {
                                                            identityCredentialStatusCompanyDetailsCompanyTypeObj,
                                                            credentialObj
                                                        }).Join(
                                                        _dbContext.PartnerDetails,
                                                        identityCredentialStatusCompanyDetailsCompanyTypeObj => identityCredentialStatusCompanyDetailsCompanyTypeObj.credentialObj.PartnerDetail_Id,
                                                        partnerDetailsObj => partnerDetailsObj.Id,
                                                        (identityCredentialStatusCompanyDetailsCompanyTypeCredentialsObj, partnerDetailsObj) => new
                                                        {
                                                            identityCredentialStatusCompanyDetailsCompanyTypeCredentialsObj,
                                                            partnerDetailsObj
                                                        }).Join(
                                                        _dbContext.VerifyIdentityVias,
                                                        identityCredentialStatusCompanyDetailsCompanyTypeObj =>              identityCredentialStatusCompanyDetailsCompanyTypeObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialsObj.identityCredentialStatusCompanyDetailsCompanyTypeObj.identityCredentialStatusCompanyDetailsObj.identityCredentialStatusObj.identityCredentialObj.VerifyIdentityViaId,
                                                        verifyMyIdentityViaObj => verifyMyIdentityViaObj.Id,
                                                        (identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssObj, verifyMyIdentityViaObj) => new
                                                        {
                                                            identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssObj,
                                                            verifyMyIdentityViaObj
                                                        }).Join(
                                                        _dbContext.Admin,
                                                        identityCredentialStatusCompanyDetailsCompanyTypeObj => identityCredentialStatusCompanyDetailsCompanyTypeObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialsObj.identityCredentialStatusCompanyDetailsCompanyTypeObj.identityCredentialStatusCompanyDetailsObj.identityCredentialStatusObj.identityCredentialObj.Id,
                                                        adminObj => adminObj.IdentityCredentialId,
                                                        (identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssVerifyIdentityObj, adminObj) => new
                                                        {
                                                            identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssVerifyIdentityObj,
                                                            adminObj
                                                        }).Where(x => partnerName.Contains(x.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssVerifyIdentityObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssObj.partnerDetailsObj.PartnerName)).AsEnumerable()
                                                        .Select(
                                                            x => new CredentialReportResponse
                                                            {
                                                                Id = x.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssVerifyIdentityObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialsObj.identityCredentialStatusCompanyDetailsCompanyTypeObj.identityCredentialStatusCompanyDetailsObj.identityCredentialStatusObj.identityCredentialObj.Id + "_identity",
                                                                CompanyName = x.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssVerifyIdentityObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialsObj.identityCredentialStatusCompanyDetailsCompanyTypeObj.identityCredentialStatusCompanyDetailsObj.companyDetailsObj.CompanyName,
                                                                Partner = x.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssVerifyIdentityObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssObj.partnerDetailsObj.PartnerName,
                                                                RequestedOn = x.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssVerifyIdentityObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialsObj.identityCredentialStatusCompanyDetailsCompanyTypeObj.identityCredentialStatusCompanyDetailsObj.identityCredentialStatusObj.identityCredentialObj.CreationDate.ToString("MM/dd/yyyy HH:mm:ss"),
                                                                TypeOfCredntial = "Identity",
                                                                CompanyType = x.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssVerifyIdentityObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialsObj.identityCredentialStatusCompanyDetailsCompanyTypeObj.companyTypeObj.CompanyType,
                                                                Status = x.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssVerifyIdentityObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssObj.identityCredentialStatusCompanyDetailsCompanyTypeCredentialsObj.identityCredentialStatusCompanyDetailsCompanyTypeObj.identityCredentialStatusCompanyDetailsObj.identityCredentialStatusObj.statusObj.Name,
                                                                ApprovedOn = x.adminObj.AproveRejectDate.ToString("MM/dd/yyyy HH:mm:ss"),
                                                                SignedBy = x.adminObj.Comments,
                                                                ProofSource = x.identityCredentialStatusCompanyDetailsCompanyTypeCredentialPartnerDetailssVerifyIdentityObj.verifyMyIdentityViaObj.Name
                                                            }).ToList();
            identityObj.AddRange(atpObj);
            identityObj = identityObj.OrderByDescending(x => x.RequestedOn).ToList();
            return identityObj;
        }
        public IdentityCredentialResponse GetIdentityCredentialData(string Id)
        {
            var result = new IdentityCredentialResponse();
            if (Id.Split('_')[1] == "identity")
            {
                int id = Convert.ToInt32(Id.Split('_')[0]);
                result = _dbContext.IdentityCredentials
                 .Where(x => x.Id == id).Join(
                        _dbContext.CompanyDetail,
                        identityCredentialObj => identityCredentialObj.CredentialId,
                        companyDetailObj => companyDetailObj.CredentialId,
                        (identityCredentialObj, companyDetailObj) => new
                        {
                            identityCredentialObj,
                            companyDetailObj
                        }).Join(
                        _dbContext.CompanyTypes,
                        identityCredentialCompanyDetailObj => identityCredentialCompanyDetailObj.companyDetailObj.CompanyTypeId,
                        companyTypeObj => companyTypeObj.Id,
                        (identityCredentialCompanyDetailObj, companyTypeObj) => new
                        {
                            identityCredentialCompanyDetailObj,
                            companyTypeObj
                        }).Join(
                        _dbContext.VerifyIdentityVias,
                        identityCredentialCompanyDetailCompanyTypeObj => identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.identityCredentialObj.VerifyIdentityViaId,
                        verifyIdentityViaObj => verifyIdentityViaObj.Id,
                        (identityCredentialCompanyDetailCompanyTypeObj, verifyIdentityViaObj) => new
                        {
                            identityCredentialCompanyDetailCompanyTypeObj,
                            verifyIdentityViaObj
                        }).Select(
                        x => new IdentityCredentialResponse
                        {
                            CompanyName = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.companyDetailObj.CompanyName,
                            AddressLine1 = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.companyDetailObj.AddressLine1,
                            AddressLine2 = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.companyDetailObj.AddressLine2,
                            City = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.companyDetailObj.City,
                            State = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.companyDetailObj.State,
                            Zip = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.companyDetailObj.Zip,
                            ContactPerson = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.companyDetailObj.ContactPerson,
                            VerifyIdentityVia = x.verifyIdentityViaObj.Name,
                            CompanyType = x.identityCredentialCompanyDetailCompanyTypeObj.companyTypeObj.CompanyType,
                            CertificatePassword = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.identityCredentialObj.CertificatePassword,
                            email = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.companyDetailObj.email,
                            contactPhone = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.companyDetailObj.contactPhone,
                            irsEmpIdentification = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.identityCredentialObj.irsEmpIdentification,
                            DunsNumber = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.identityCredentialObj.DunsNumber,
                            articlesOfIncorporation = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.identityCredentialObj.articlesOfIncorporation,
                            uploadDeaSignCert = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.identityCredentialObj.uploadDeaSignCert,
                            gln = x.identityCredentialCompanyDetailCompanyTypeObj.identityCredentialCompanyDetailObj.companyDetailObj.gln,
                        }).SingleOrDefault();
            }
            return result;
        }
        public DownloadCertificateResponse GetCertificateDetailsById(DownloadCertificateRequest req)
        {
            var certDetailsObj = new DownloadCertificateResponse();
            if (req.id.Split('_')[1] == "identity")
            {
                int Id = Convert.ToInt32(req.id.Split('_')[0]);
                certDetailsObj = _dbContext.IdentityCredentials.Where(x => x.Id == Id).Join(
                                                              _dbContext.Credentials,
                                                              icb => icb.CredentialId,
                                                              c => c.Id,
                                                              (icb, c) => new
                                                              {
                                                                  icb,
                                                                  c
                                                              }
              ).Select(x => new DownloadCertificateResponse
              {
                  CredentialToken = x.c.CredentialToken
              }
            ).SingleOrDefault();

            }
            return certDetailsObj;
        }
        public System.Net.Http.HttpResponseMessage DownloadFile(DownloadCertificateRequest Req)
        {
            var result = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK);
            DownloadCertificateResponse DownloadCertificateDetails = GetCertificateDetailsById(Req);
            var filePath = System.Configuration.ConfigurationManager.AppSettings["FileUploadPath"] + (DownloadCertificateDetails.CredentialToken) + @"\" + Req.fileName;
            var fileBytes = File.ReadAllBytes(filePath);
            var fileMemStream = new MemoryStream(fileBytes);
            result.Content = new StreamContent(fileMemStream);
            var headers = result.Content.Headers;
            headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            headers.ContentDisposition.FileName = DownloadCertificateDetails.fileName;
            headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            headers.ContentLength = fileMemStream.Length;

            return result;
        }
        public AdminModel SaveAdminData(AdminModel adminObj)
        {
            try
            {
                using (var context = new CredentialSolutionDbContext())
                {
                    var resultInDb = context.Admin.SingleOrDefault(b => b.IdentityCredentialId == adminObj.IdentityCredentialId);
                    if (resultInDb != null && resultInDb.IdentityCredentialId != 0)
                    {
                        resultInDb.Reason = adminObj.Reason;
                        resultInDb.SigningCertificate = adminObj.SigningCertificate;
                        resultInDb.SignInPassword = adminObj.SignInPassword;
                        resultInDb.Approve = adminObj.Approve;
                        resultInDb.Comments = adminObj.Comments;
                        context.SaveChanges();
                        return resultInDb;

                    }
                    else
                    {
                        context.Admin.Add(adminObj);
                        if (context.SaveChanges() > 0)
                        {
                            return adminObj;
                        }
                        else
                            return adminObj;
                    }

                }
            }
            catch (Exception e)
            {
                return adminObj;
            }
        }
        public Admin_DueDilligenceModel SaveAdminDueDilligence(Admin_DueDilligenceModel adminDueDilligenceObj)
        {
            try
            {
                using (var context = new CredentialSolutionDbContext())
                {
                    var resultInDb = context.AdminDueDilligence.SingleOrDefault(b => b.AdminId == adminDueDilligenceObj.AdminId);
                    if (resultInDb != null && resultInDb.AdminId != 0)
                    {
                        resultInDb.articlesOfIncAddress = adminDueDilligenceObj.articlesOfIncAddress;
                        resultInDb.articlesOfIncName = adminDueDilligenceObj.articlesOfIncName;
                        resultInDb.credentialReqestFormAddress = adminDueDilligenceObj.credentialReqestFormAddress;
                        resultInDb.credentialReqestFormName = adminDueDilligenceObj.credentialReqestFormName;
                        resultInDb.dunsNoAddress = adminDueDilligenceObj.dunsNoAddress;

                        resultInDb.dunsNoName = adminDueDilligenceObj.dunsNoName;
                        resultInDb.glnAddress = adminDueDilligenceObj.glnAddress;
                        resultInDb.glnName = adminDueDilligenceObj.glnName;
                        resultInDb.irsletterAddress = adminDueDilligenceObj.irsletterAddress;
                        resultInDb.irsletterName = adminDueDilligenceObj.irsletterName;

                        resultInDb.AdminId = adminDueDilligenceObj.AdminId;
                        context.SaveChanges();
                        return resultInDb;

                    }
                    else
                    {
                        context.AdminDueDilligence.Add(adminDueDilligenceObj);
                        if (context.SaveChanges() > 0)
                        {
                            return adminDueDilligenceObj;
                        }
                        else
                            return adminDueDilligenceObj;
                    }

                }
            }
            catch (Exception e)
            {
                return adminDueDilligenceObj;
            }
        }

    }
}
