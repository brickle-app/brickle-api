using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Domain.Enums;

namespace BricklePlatform.Domain.DTOs;

public class CreateTokenizeAsset
{
  public required CreateCampaignDto Campaign { get; set; }
  public required CreateUserLeasingAgreementDto Leasing { get; set; }
}