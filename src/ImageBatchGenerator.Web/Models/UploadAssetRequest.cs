namespace ImageBatchGenerator.Web.Models;

public class UploadAssetRequest
{
    public IFormFile File { get; set; } = null!;
    public string Category { get; set; } = string.Empty;
}
