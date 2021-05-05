//4 tables
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
    Status = Enum.GetName(typeof(IdentityCredentialStatusModel.IdentityCredentialStatus), ATP.StatusId)
}).ToList();

//5 tables
var identityObj = (
    from IC in _dbContext.IdentityCredentials
    join CD in  _dbContext.CompanyDetail
    on IC.CredentialId equals CD.CredentialId

    join C in _dbContext.Credentials
    on CD.CredentialId equals C.Id

    join PD in _dbContext.PartnerDetails
    on C.PartnerDetail_Id equals PD.Id

    join VIV in _dbContext.VerifyIdentityVias
    on IC.VerifyIdentityViaId equals VIV.Id

    where IC.IdentityCredentialStatusId == (int)IdentityCredentialStatusModel.IdentityCredentialStatus.Pending
    select new PendingCredentialResponse
    {
        Id = IC.Id + "_atp",
        CompanyName = CD.CompanyName,
        Partner =PD.PartnerName,
        CreationDate = IC.CreationDate.ToString("MM/dd/yyyy HH:mm:ss"),
        TypeOfCredential = "Identity",
        ProofSource = VIV.Name,
        Status = Enum.GetName(typeof(IdentityCredentialStatusModel.IdentityCredentialStatus), IC.IdentityCredentialStatusId)
    }).ToList();