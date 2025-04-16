namespace FaceRecognitionWebsite.Models;

public class FaceImage
{
    public int Id { get; set; }
    public string ImagePath { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; }
}