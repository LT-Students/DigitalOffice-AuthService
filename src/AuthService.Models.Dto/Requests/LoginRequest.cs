namespace LT.DigitalOffice.AuthService.Models.Dto.Requests
{
  public record LoginRequest
  {
    public string LoginData { get; set; }
    public string Password { get; set; }
  }
}
