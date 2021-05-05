var atpObject = (
from ATP in _dbContext.AtpCredential
join CD in _dbContext.CompanyDetail
on ATP.CredentialId equals CD.CredentialId
join C in _dbContext.Credentials
on CD.CredentialId equals C.Id
join PD in _dbContext.PartnerDetails
on C.PartnerDetail_Id equals PD.Id
where ATP.StatusId == (int)IdentityCredentialStatusModel.IdentityCredentialStatus.Pending
select new PendingCredentialResponse
{
    Id = ATP.Id + "_atp",
    CompanyName = CD.CompanyName,
    Partner =PD.PartnerName,
    CreationDate = ATP.CreationDate.ToString("MM/dd/yyyy HH:mm:ss"),
    TypeOfCredential = "ATP",
    ProofSource = "",
    Status = Enum.GetName(typeof(IdentityCredentialStatusModel.IdentityCredentialStatus), x.credentialsAtpCompanyDetailsObj.atpCredentialsCompanyDetailsObj.atpCredentialObj.StatusId)
}).ToList();

