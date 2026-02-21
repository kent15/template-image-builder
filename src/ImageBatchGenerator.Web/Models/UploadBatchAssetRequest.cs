namespace ImageBatchGenerator.Web.Models;

public class UploadBatchAssetRequest
{
    public IFormFile ZipFile { get; set; } = null!;
    public string Category { get; set; } = string.Empty;
}
