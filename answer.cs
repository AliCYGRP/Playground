// //4 tables
var atpObj = (
from ATP in _dbContext.AtpCredential
join CD in _dbContext.CompanyDetail
on ATP.CredentialId equals CD.CredentialId
where ATP.StatusId == (int)IdentityCredentialStatusModel.IdentityCredentialStatus.Pending

join C in _dbContext.Credentials
on CD.CredentialId equals C.Id

join PD in _dbContext.PartnerDetails
on C.PartnerDetail_Id equals PD.Id

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

//################# GetCredentialReports

var atpObject = (
    from ATPC in _dbContext.AtpCredential

    join ICS in _dbContext.IdentityCredentialStatus
    on ATPC.StatusId equals ICS.Id
    where statusName.Contains(ICS.Name) && ATPC.CreationDate <= Req.ToDate &&  ATPC.CreationDate >= Req.FromDate

    join CD in _dbContext.CompanyDetail
    on ATPC.CredentialId equals CD.Id

    join CT in _dbContext.CompanyTypes
    on CD.CompanyTypeId equals CT.Id

    join C in _dbContext.Credentials
    on ATPC.CredentialId equals C.Id

    join PD in _dbContext.PartnerDetails
    on C.PartnerDetail_Id equals PD.Id
    where partnerName.Contains(PD.PartnerName)
    select new CredentialReportResponse{
        Id = ATPC.Id + "_atp",
        CompanyName = CD.CompanyName,
        Partner = PD.PartnerName,
        RequestedOn = ATPC.CreationDate.ToString("MM/dd/yyyy HH:mm:ss"),
        TypeOfCredntial = "ATP",
        CompanyType = CT.CompanyType,
        Status = ATPC.Name
    }).ToList();

var identityObj = (
    from IC in _dbContext.IdentityCredentials

    join ICS in _dbContext.IdentityCredentialStatus
    on IC.IdentityCredentialStatus equals ICS.Id
    where statusName.Contains(ICS.Name) && IC.CreationDate <= Req.ToDate && IC.CreationDate >= Req.FromDate

    join CD in _dbContext.CompanyDetail
    on IC.CredentialId equals CD.Id

    join CT in _dbContext.CompanyTypes
    on CD.CompanyTypeId equals CT.Id

    join C in _dbContext.Credentials
    on IC.CredentialId equals C.Id

    join PD in _dbContext.PartnerDetails
    on C.PartnerDetail_Id equals PD.Id
    where partnerName.Contains(PD.PartnerName)
    select new CredentialReportResponse
    {
        Id = IC.Id + "_identity",
        CompanyName = CD.CompanyName,
        Partner = PD.PartnerName,
        RequestedOn = IC.CreationDate.ToString("MM/dd/yyyy HH:mm:ss"),
        TypeOfCredntial = "Identity",
        CompanyType = CT.CompanyType,
        Status = ICS.Name
    }).ToList();
