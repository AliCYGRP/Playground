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